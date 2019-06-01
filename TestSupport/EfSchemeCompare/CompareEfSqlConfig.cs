// Copyright (c) 2017 Jon P Smith, GitHub: JonPSmith, web: http://www.thereformedprogrammer.net/
// Licensed under MIT licence. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Collections.Immutable;

namespace TestSupport.EfSchemeCompare
{
    /// <summary>
    /// This class holds the configuration information for the CompareEfSql class
    /// </summary>
    public class CompareEfSqlConfig
    {
        private readonly List<CompareLog> _logsToIgnore = new List<CompareLog>();

        /// <summary>
        /// This allows you to ignore tables that your EF Core context doesn't use. There are three settings
        /// 1. null - this will only check the tables that the DbContexts are mapped to.
        /// 2. "" - This will check all tables in the database against the entity classes in the DbContexts.
        /// 3. A comma delimited list of tables, with optional schema, to ignore. 
        ///    Typical format: "MyTable,MyOtherTable,MySchema.MyTableWithSchema" (note: the schema/table match is case insensitive)
        /// </summary>
        public string TablesToIgnoreCommaDelimited { get; set; }

        /// <summary>
        /// This contains all the log types that should be ignored by the comparer
        /// </summary>
        public IReadOnlyList<CompareLog> LogsToIgnore => _logsToIgnore.ToImmutableList();

        /// <summary>
        /// This allows you to clip a set of errors strings and add them as ignore items
        /// </summary>
        /// <param name="textWithNewlineBetweenErrors"></param>
        public void IgnoreTheseErrors(string textWithNewlineBetweenErrors)
        {
            foreach (var errorString in textWithNewlineBetweenErrors.Split('\n'))
            {
                AddIgnoreCompareLog(CompareLog.DecodeCompareTextToCompareLog(errorString));
            }
        }

        /// <summary>
        /// This allows you to add a log with setting that will be used to decide if a log is ignored
        /// The Type and State must be set, but any strings set to null with automatically match anything 
        /// and the Attribute has a MatchAnything setting too.
        /// </summary>
        /// <param name="logTypeToIgnore"></param>
        public void AddIgnoreCompareLog(CompareLog logTypeToIgnore)
        {
            if (logTypeToIgnore.State <= CompareState.Ok)
                throw new ArgumentException("You cannot ignore logs with a State of OK (or lower).");
            _logsToIgnore.Add(logTypeToIgnore);
        }
    }
}