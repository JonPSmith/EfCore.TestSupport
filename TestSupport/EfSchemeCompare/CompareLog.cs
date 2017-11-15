// Copyright (c) 2017 Jon P Smith, GitHub: JonPSmith, web: http://www.thereformedprogrammer.net/
// Licensed under MIT licence. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using TestSupport.EfSchemeCompare.Internal;

namespace TestSupport.EfSchemeCompare
{
    /// <summary>
    /// This is used to define what is being compared
    /// </summary>
    public enum CompareType { NoSet,
        //Software side
        DbContext, Entity, Property,
        //Database side (used for ExtraInDatabase)
        Table,
        //Used for both
        PrimaryKey, ForeignKey, Index}
    /// <summary>
    /// This defines the result of a comparision
    /// </summary>
    public enum CompareState { NoSet, Ok, Different, NotInDatabase, ExtraInDatabase }
    /// <summary>
    /// This contains extra information on what exactly was compared
    /// </summary>
    public enum CompareAttributes { NotSet,
        //column items
        ColumnName, ColumnType, Nullability, DefaultValueSql, ComputedColumnSql, ValueGenerated,
        //Tables
        TableName,
        //keys - primary, foreign, alternative
        PrimaryKey, ConstraintName, IndexConstraintName, Unique, DeleteBehaviour,
        //Others
    }

    /// <summary>
    /// This holds the log of each compare done
    /// </summary>
    public class CompareLog
    {
        /// <summary>
        /// Because an EF Core DbContext has a hierarchy then the logs are also in a hierarchy
        /// For EF Core this is DbContext->Entity classes->Properties
        /// </summary>
        public List<CompareLog> SubLogs { get; } = new List<CompareLog>();

        [JsonConstructor]
        internal CompareLog(CompareType type, CompareState state, string name, CompareAttributes attribute, string expected, string found)
        {
            Type = type;
            State = state;
            Name = name;
            Attribute = attribute;
            Expected = expected;
            Found = found;
        }

        /// <summary>
        /// This holds what it is comparing
        /// </summary>
        public CompareType Type { get; }
        /// <summary>
        /// This holds the result of the comparison
        /// </summary>
        public CompareState State { get; }
        /// <summary>
        /// This holds the name of the primary thing it is comparing, e.g. MyEntity, MyProperty
        /// </summary>
        public string Name { get; }
        /// <summary>
        /// This contains extra information to define exactly what is being compared, for instance the ColumnName that a property is mapped to
        /// </summary>
        public CompareAttributes Attribute { get; }
        /// <summary>
        /// This holds what EF Core expects to see
        /// </summary>
        public string Expected { get; }
        /// <summary>
        /// This holds what was found in the database
        /// </summary>
        public string Found { get; }

        /// <summary>
        /// This provides a human-readable version of a CompareLog
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            //Typical output would be: DIFFERENT: Column 'Id', column type. Expected = varchar(20), Found = nvarchar(20)
            return ToStringDifferentStart($"{State.SplitCamelCaseToUpper()}: ");
        }

        /// <summary>
        /// This returns all the logs, with an indentation for each level in the hierarchy
        /// </summary>
        /// <param name="logs"></param>
        /// <param name="indent"></param>
        /// <returns></returns>
        public static IEnumerable<string> AllResultsIndented(IReadOnlyList<CompareLog> logs, string indent = "")
        {
            foreach (var log in logs)
            {
                yield return (indent + log);
                if (log.SubLogs.Any())
                {
                    foreach (var text in AllResultsIndented(log.SubLogs, indent + "   "))
                    {
                        yield return text;
                    }
                }
            }
        }

        /// <summary>
        /// This returns a string per error, in human-readable form
        /// </summary>
        /// <param name="logs"></param>
        /// <param name="parentNames"></param>
        /// <returns></returns>
        public static IEnumerable<string> ListAllErrors(IReadOnlyList<CompareLog> logs, Stack<string> parentNames = null)
        {
            //This only includes the DbContext if there were mutiple DbContexts at the top layer
            var firstCall = parentNames == null;
            var doPushPop = !(firstCall && logs.Count == 1);
            if (firstCall)
            {
                parentNames = new Stack<string>();                 
            }

            foreach (var log in logs)
            {

                if (log.State != CompareState.Ok)
                    yield return FormFullRefError(log, parentNames);
                if (log.SubLogs.Any())
                {
                    if (doPushPop) parentNames.Push(log.Name);
                    foreach (var errors in ListAllErrors(log.SubLogs, parentNames))
                    {
                        yield return errors;
                    }
                    if (doPushPop) parentNames.Pop();
                }
            }
        }

        //-------------------------------------------------------
        //private

        private string ToStringDifferentStart(string start)
        {
            //Typical output would be: Column 'Id', column type is Different : Expected = varchar(20), Found = nvarchar(20)
            var result = $"{start}{Type} '{Name}'";
            if (Attribute != CompareAttributes.NotSet)
                result += $", {Attribute.SplitCamelCaseToLower()}";
            if (State == CompareState.Ok)
                return result;

            var sep = ". F";
            if (State != CompareState.ExtraInDatabase)
            {
                result += $". Expected = {Expected ?? "<null>"}";
                sep = ", f";
            }
            if (Found != null || State == CompareState.Different)
                result += $"{sep}ound = {Found ?? "<null>"}";
            return result;
        }
        private static string FormFullRefError(CompareLog log, Stack<string> parents)
        {
            string start = $"{log.State.SplitCamelCaseToUpper()}: ";
            if (parents.Any())
                start += string.Join("->", parents.ToArray().Reverse()) + "->";
            return log.ToStringDifferentStart(start);
        }
    }  
    
}