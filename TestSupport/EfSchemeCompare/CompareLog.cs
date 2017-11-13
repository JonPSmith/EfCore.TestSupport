// Copyright (c) 2017 Jon P Smith, GitHub: JonPSmith, web: http://www.thereformedprogrammer.net/
// Licensed under MIT licence. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Linq;
using TestSupport.EfSchemeCompare.Internal;

namespace TestSupport.EfSchemeCompare
{
    public enum CompareType { NoSet, DbContext, Entity, Property, ForeignKey, Index}
    public enum CompareState { NoSet, Ok, Different, NotInDatabase, ExtraInDatabase }
    public enum CompareAttributes { NotSet,
        //column items
        ColumnType, Nullability, DefaultValueSql, ComputedColumnSql, ValueGenerated,
        //Tables
        TableName,
        //keys - primary, foreign, alternative
        PrimaryKey, PrimaryKeyConstraintName, IndexConstraintName, ForeignKeyIndexConstraintName, Unique, DeleteBehaviour,
        //Others
    }
    public class CompareLog
    {
        public List<CompareLog> SubLogs { get; } = new List<CompareLog>();

        internal CompareLog(CompareType type, CompareState state, string name, CompareAttributes attribute, string expected, string found)
        {
            Type = type;
            State = state;
            Name = name;
            Attribute = attribute;
            Expected = expected;
            Found = found;
        }

        //Name holds the type of 
        public CompareType Type { get; }
        public CompareState State { get; }
        public string Name { get; }
        public CompareAttributes Attribute { get; }

        public string Expected { get; }
        public string Found { get; }

        public override string ToString()
        {
            //Typical output would be: Column 'Id', column type is Different : Expected = varchar(20), Found = nvarchar(20)
            var result = $"{State.SplitCamelCaseToUpper()}: {Type} '{Name}'";
            if (Attribute != CompareAttributes.NotSet)
                result += $", {Attribute.SplitCamelCaseToLower()}";
            if (State == CompareState.Ok)
                return result;

            result += $". Expected = {Expected}";
            if (Found != null)
                result += $" , Found = {Found}";
            return result;
        }

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

        /// <summary>
        /// This returns true if there were any errors in the Logs
        /// </summary>
        /// <param name="logs"></param>
        /// <returns></returns>
        public static bool HadErrors(IReadOnlyList<CompareLog> logs)
        {
            foreach (var log in logs)
            {
                if (log.State == CompareState.Ok)
                {
                    if (log.SubLogs.Any())
                    {
                        return HadErrors(log.SubLogs);
                    }
                }
                else
                {
                    return true;
                }
            }
            return false;
        }

        //-------------------------------------------------------
        //private
        private static string FormFullRefError(CompareLog log, Stack<string> parents)
        {
            string result = "";
            if (parents.Any())
                result += string.Join("->", parents.ToArray().Reverse()) + "->";
            return result + log;
        }
    }  
    
}