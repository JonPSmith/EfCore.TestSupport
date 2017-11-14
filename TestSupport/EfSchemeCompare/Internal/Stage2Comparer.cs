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
        public IReadOnlyList<CompareLog> Logs => _logs.ToImmutableList();

        public Stage2Comparer(DatabaseModel databaseModel)
        {
            _databaseModel = databaseModel;
        }

        public bool CompareLogsToDatabase(IReadOnlyList<CompareLog> firstStageLogs)
        {
            var logger = new CompareLogger(CompareType.Table, null, _logs, () => _hasErrors = true);
            LookForUnusedTables(firstStageLogs, logger);
            LookForUnusedColumns(firstStageLogs, logger);
            LookForUnusedIndexes(firstStageLogs, logger);

            return _hasErrors;
        }

        private void LookForUnusedTables(IReadOnlyList<CompareLog> firstStageLogs, CompareLogger logger)
        {
                     var tableDict = _databaseModel.Tables.ToDictionary(x => x.Name);
            var allEntityTableNames = firstStageLogs.SelectMany(p => p.SubLogs)
                .Where(x => x.State == CompareState.Ok && x.Type == CompareType.Entity)
                .Select(p => p.Expected).OrderBy(p => p).Distinct();
            var tablesNotUsed = allEntityTableNames.Where(p => !tableDict.ContainsKey(p));

            foreach (var tableName in tablesNotUsed)
            {
                logger.ExtraInDatabase(null, CompareAttributes.TableName, tableName);
            }
        }

        private void LookForUnusedColumns(IReadOnlyList<CompareLog> firstStageLogs, CompareLogger logger)
        {
            var tableDict = _databaseModel.Tables.ToDictionary(x => x.Name);
            foreach (var entityLog in firstStageLogs.SelectMany(p => p.SubLogs)
                .Where(x => x.State == CompareState.Ok && x.Type == CompareType.Entity))
            {
                if (tableDict.ContainsKey(entityLog.Expected))
                {
                    var colDict = tableDict[entityLog.Expected].Columns.ToDictionary(x => x.Name);
                    var allEfColNames = entityLog.SubLogs
                        .Where(x => x.State == CompareState.Ok && x.Type == CompareType.Property)
                        .Select(p => p.Expected).OrderBy(p => p).Distinct();
                    var colsNotUsed = allEfColNames.Where(p => !colDict.ContainsKey(p));
                    foreach (var colName in colsNotUsed)
                    {
                        logger.ExtraInDatabase(colName, CompareAttributes.ColumnName, entityLog.Expected);
                    }
                }               
            }
        }

        private void LookForUnusedIndexes(IReadOnlyList<CompareLog> firstStageLogs, CompareLogger logger)
        {
            var tableDict = _databaseModel.Tables.ToDictionary(x => x.Name);
            foreach (var entityLog in firstStageLogs.SelectMany(p => p.SubLogs)
                .Where(x => x.State == CompareState.Ok && x.Type == CompareType.Entity))
            {
                if (tableDict.ContainsKey(entityLog.Expected))
                {
                    var indexCol = tableDict[entityLog.Expected].Indexes.ToDictionary(x => x.Name);
                    var allEfIndexNames = entityLog.SubLogs
                        .Where(x => x.State == CompareState.Ok && x.Type == CompareType.Index)
                        .Select(p => p.Expected).OrderBy(p => p).Distinct();
                    var indexesNotUsed = allEfIndexNames.Where(p => !indexCol.ContainsKey(p));
                    foreach (var indexName in indexesNotUsed)
                    {
                        logger.ExtraInDatabase(indexName, CompareAttributes.IndexConstraintName, entityLog.Expected);
                    }
                }
            }
        }

    }
}