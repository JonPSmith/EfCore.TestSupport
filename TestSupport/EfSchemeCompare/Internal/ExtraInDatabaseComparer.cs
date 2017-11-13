// Copyright (c) 2017 Jon P Smith, GitHub: JonPSmith, web: http://www.thereformedprogrammer.net/
// Licensed under MIT licence. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using Microsoft.EntityFrameworkCore.Scaffolding.Metadata;

namespace TestSupport.EfSchemeCompare.Internal
{
    internal class ExtraInDatabaseComparer
    {
        private readonly DatabaseModel _databaseModel;

        private bool _hasErrors;

        private readonly List<CompareLog> _logs = new List<CompareLog>();
        public IReadOnlyList<CompareLog> Logs => _logs.ToImmutableList();

        public ExtraInDatabaseComparer(DatabaseModel databaseModel)
        {
            _databaseModel = databaseModel;
        }

        public bool CompareLogsToDatabase(IReadOnlyList<CompareLog> firstStageLogs)
        {
            var logger = new CompareLogger(CompareType.Table, null, _logs, () => _hasErrors = true);

            var tableDict = _databaseModel.Tables.ToDictionary(x => x.Name);
            var allEntityTableNames = firstStageLogs.SelectMany(p => p.SubLogs)
                .Where(x => x.State == CompareState.Ok && x.Type == CompareType.Entity)
                .Select(p => p.Expected).OrderBy(p => p).Distinct();
            var tablesNotUsed = allEntityTableNames.Where(p => !tableDict.ContainsKey(p));

            foreach (var tableName in tablesNotUsed)
            {
                logger.ExtraInDatabase(null, CompareAttributes.TableName, tableName);
            }

            return _hasErrors;
        }

    }
}