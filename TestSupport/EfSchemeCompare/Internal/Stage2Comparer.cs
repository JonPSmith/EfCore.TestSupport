// Copyright (c) 2017 Jon P Smith, GitHub: JonPSmith, web: http://www.thereformedprogrammer.net/
// Licensed under MIT licence. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using Microsoft.EntityFrameworkCore.Scaffolding.Metadata;

namespace TestSupport.EfSchemeCompare.Internal
{
    internal class Stage2Comparer
    {
        private readonly DatabaseModel _databaseModel;

        private bool _hasErrors;

        private readonly List<CompareLog> _logs = new List<CompareLog>();
        private readonly IReadOnlyList<CompareLog> _ignoreList;
        public IReadOnlyList<CompareLog> Logs => _logs.ToImmutableList();

        public Stage2Comparer(DatabaseModel databaseModel, IReadOnlyList<CompareLog> ignoreList = null)
        {
            _databaseModel = databaseModel;
            _ignoreList = ignoreList ?? new List<CompareLog>();
        }

        public bool CompareLogsToDatabase(IReadOnlyList<CompareLog> firstStageLogs)
        {
            var logger = new CompareLogger(CompareType.Database, _databaseModel.DatabaseName, _logs, _ignoreList, () => _hasErrors = true);
            logger.MarkAsOk(null);
            LookForUnusedTables(firstStageLogs, _logs.Last());
            LookForUnusedColumns(firstStageLogs, _logs.Last());
            LookForUnusedIndexes(firstStageLogs, _logs.Last());

            return _hasErrors;
        }

        private void LookForUnusedTables(IReadOnlyList<CompareLog> firstStageLogs, CompareLog log)
        {
            var logger = new CompareLogger(CompareType.Table, null, log.SubLogs, _ignoreList, () => _hasErrors = true);
            var databaseTableNames = _databaseModel.Tables.Select(x => x.Name);
            var allEntityTableNames = firstStageLogs.SelectMany(p => p.SubLogs)
                .Where(x => x.State == CompareState.Ok && x.Type == CompareType.Entity)
                .Select(p => p.Expected).OrderBy(p => p).Distinct().ToList();
            var tablesNotUsed = databaseTableNames.Where(p => !allEntityTableNames.Contains(p));

            foreach (var tableName in tablesNotUsed)
            {
                logger.ExtraInDatabase(null, CompareAttributes.NotSet, tableName);
            }
        }

        private void LookForUnusedColumns(IReadOnlyList<CompareLog> firstStageLogs, CompareLog log)
        {
            var logger = new CompareLogger(CompareType.Column, null, log.SubLogs, _ignoreList, () => _hasErrors = true);
            var tableDict = _databaseModel.Tables.ToDictionary(x => x.Name);
            //because of table splitting and TPH we need to groups properties by table name to correctly say what columns are missed
            var entityColsGrouped = firstStageLogs.SelectMany(p => p.SubLogs)
                .Where(x => x.State == CompareState.Ok && x.Type == CompareType.Entity)
                .GroupBy(x => x.Expected, y => y.SubLogs
                    .Where(x => x.State == CompareState.Ok && x.Type == CompareType.Property)
                    .Select(p => p.Expected));
            var entityColsByTableDict = entityColsGrouped.ToDictionary(x => x.Key, y => y.SelectMany(x => x.ToList()));

            foreach (var entityLog in firstStageLogs.SelectMany(p => p.SubLogs)
                .Where(x => x.State == CompareState.Ok && x.Type == CompareType.Entity))
            {
                if (tableDict.ContainsKey(entityLog.Expected))
                {
                    var dbColNames = tableDict[entityLog.Expected].Columns.Select(x => x.Name);
                    var colsNotUsed = dbColNames.Where(p => !entityColsByTableDict[entityLog.Expected].Contains(p));
                    foreach (var colName in colsNotUsed)
                    {
                        logger.ExtraInDatabase(colName, CompareAttributes.ColumnName, entityLog.Expected);
                    }
                }               
            }
        }

        private void LookForUnusedIndexes(IReadOnlyList<CompareLog> firstStageLogs, CompareLog log)
        {
            var logger = new CompareLogger(CompareType.Index, null, log.SubLogs, _ignoreList, () => _hasErrors = true);
            var tableDict = _databaseModel.Tables.ToDictionary(x => x.Name);
            foreach (var entityLog in firstStageLogs.SelectMany(p => p.SubLogs)
                .Where(x => x.State == CompareState.Ok && x.Type == CompareType.Entity))
            {
                if (tableDict.ContainsKey(entityLog.Expected))
                {
                    var indexCol = tableDict[entityLog.Expected].Indexes.Select(x => x.Name);
                    var allEfIndexNames = entityLog.SubLogs
                        .Where(x => x.State == CompareState.Ok && x.Type == CompareType.Index)
                        .Select(p => p.Expected).OrderBy(p => p).Distinct().ToList();
                    var indexesNotUsed = indexCol.Where(p => !allEfIndexNames.Contains(p));
                    foreach (var indexName in indexesNotUsed)
                    {
                        logger.ExtraInDatabase(indexName, CompareAttributes.IndexConstraintName, entityLog.Expected);
                    }
                }
            }
        }

    }
}