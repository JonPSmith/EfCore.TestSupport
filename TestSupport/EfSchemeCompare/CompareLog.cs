// Copyright (c) 2017 Jon P Smith, GitHub: JonPSmith, web: http://www.thereformedprogrammer.net/
// Licensed under MIT licence. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using Newtonsoft.Json;
using TestSupport.EfSchemeCompare.Internal;

[assembly: InternalsVisibleTo("Test")]

namespace TestSupport.EfSchemeCompare
{
#pragma warning disable 1591
    /// <summary>
    /// This is used to define what is being compared
    /// </summary>
    public enum CompareType { NoSet,
        MatchAnything,
        //Software side
        DbContext, Entity, Property,
        //Database side (used for ExtraInDatabase)
        Database, Table, Column,
        //Used for both
        PrimaryKey, ForeignKey, Index}
    /// <summary>
    /// This defines the result of a comparision
    /// </summary>
    public enum CompareState { Debug, Ok, Warning, Different, NotInDatabase, ExtraInDatabase }
    /// <summary>
    /// This contains extra information on what exactly was compared
    /// </summary>
    public enum CompareAttributes { NotSet,
        //This is used to match any attribute for the ignore list
        MatchAnything,
        //column items
        ColumnName, ColumnType, Nullability, DefaultValueSql, ComputedColumnSql, ValueGenerated,
        //Tables
        TableName,
        //keys - primary, foreign, alternative
        PrimaryKey, ConstraintName, IndexConstraintName, Unique, DeleteBehaviour,
        //Others
    }
#pragma warning restore 1591

    /// <summary>
    /// This holds the log of each compare done
    /// </summary>
    public class CompareLog
    {
        /// <summary>
        /// Because an EF Core DbContext has a hierarchy then the logs are also in a hierarchy
        /// For EF Core this is DbContext->Entity classes->Properties
        /// </summary>
        public List<CompareLog> SubLogs { get; }

        /// <summary>
        /// This constuctor either creates a new log (used internally) or allows the user to create a log for ignore matching
        /// </summary>
        /// <param name="type"></param>
        /// <param name="state"></param>
        /// <param name="name"></param>
        /// <param name="attribute"></param>
        /// <param name="expected"></param>
        /// <param name="found"></param>
        [JsonConstructor]
        public CompareLog(CompareType type, CompareState state, string name, 
            CompareAttributes attribute = CompareAttributes.MatchAnything, string expected = null, string found = null)
        {
            Type = type;
            State = state;
            Name = name;
            Attribute = attribute;
            Expected = expected;
            Found = found;
            SubLogs = new List<CompareLog>();
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
            //This only includes the DbContext if there were multiple DbContexts at the top layer
            var firstCall = parentNames == null;
            var doPushPop = !(firstCall && logs.GroupBy(x => x.Type).Count() < 2);
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
        //internal

        internal static CompareLog DecodeCompareTextToCompareLog(string str)
        {
            str = str.Trim();
            var indexOfColon = str.IndexOf(':');
            var indexOfArrow = str.IndexOf("->", StringComparison.Ordinal);
            if (indexOfArrow < 0)
                indexOfArrow = indexOfColon - 1;
            var indexOfFirstQuote = str.IndexOf('\'');
            var indexOfSecondQuote = str.Substring(indexOfFirstQuote + 1).IndexOf('\'') + indexOfFirstQuote + 1;

            var state = (CompareState)Enum.Parse(typeof(CompareState), str.Substring(0, indexOfColon).Replace(" ", ""), true);
            var type = (CompareType)Enum.Parse(typeof(CompareType), str.Substring(indexOfArrow + 2, indexOfFirstQuote - indexOfArrow - 3));
            var name = str.Substring(indexOfFirstQuote + 1, indexOfSecondQuote - indexOfFirstQuote - 1);
            var attribute = CompareAttributes.NotSet;
            string expected = null;
            string found = null;
            if (str.Length - 1 > indexOfSecondQuote)
            {
                var charIndex = indexOfSecondQuote + 1;
                //we have more information
                if (str[charIndex] == ',')
                {
                    var indexFullStop = str.Substring(charIndex).IndexOf('.') + charIndex; //warning, dot can be in name
                    attribute = (CompareAttributes)Enum.Parse(typeof(CompareAttributes), 
                        str.Substring(charIndex + 1, indexFullStop - charIndex - 1).Replace(" ",""), true);
                    charIndex = indexFullStop;
                }
                if (str.Substring(charIndex, 3) == ". E")
                {
                    var endCharIndex = str.Substring(charIndex + 3).IndexOf(", f");
                    endCharIndex = endCharIndex < 0 ? str.Length : endCharIndex + charIndex + 3;
                    var startOfString = charIndex + nameof(Expected).Length + 5;
                    expected = ReplaceNullTokenWithNull(str.Substring(startOfString, endCharIndex - startOfString));
                    charIndex = endCharIndex;
                }
                if (charIndex + 3 < str.Length && str.Substring(charIndex+1, 2).Equals(" F", StringComparison.CurrentCultureIgnoreCase))
                {
                    var startOfString = charIndex + nameof(Found).Length + 5;
                    found = ReplaceNullTokenWithNull( str.Substring(startOfString));
                }
            }

            return new CompareLog(type, state, name, attribute, expected, found);
        }

        internal bool ShouldIIgnoreThisLog(IReadOnlyList<CompareLog> ignoreList)
        {
            return ignoreList.Any() && ignoreList.Any(ShouldBeIgnored);
        }

        //-------------------------------------------------------
        //private

        private bool ShouldBeIgnored(CompareLog ignoreItem)
        {
            if (ignoreItem.State != State)
                return false;

            return (ignoreItem.Type == CompareType.MatchAnything || ignoreItem.Type == Type)
                && (ignoreItem.Attribute == CompareAttributes.MatchAnything || ignoreItem.Attribute == Attribute)
                && (ignoreItem.Name == null || ignoreItem.Name == Name)
                && (ignoreItem.Expected == null || ignoreItem.Expected == Expected)
                && (ignoreItem.Found == null || ignoreItem.Found == Found);
        }

        private static string ReplaceNullTokenWithNull(string str)
        {
            return str == "<null>" ? null : str;
        }

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