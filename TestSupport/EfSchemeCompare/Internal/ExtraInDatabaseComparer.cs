// Copyright (c) 2017 Jon P Smith, GitHub: JonPSmith, web: http://www.thereformedprogrammer.net/
// Licensed under MIT licence. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Collections.Immutable;
using Microsoft.EntityFrameworkCore.Scaffolding.Metadata;

namespace TestSupport.EfSchemeCompare.Internal
{
    internal class ExtraInDatabaseComparer
    {
        private DatabaseModel _databaseModel;

        private readonly List<CompareLog> _logs = new List<CompareLog>();
        public IReadOnlyList<CompareLog> Logs => _logs.ToImmutableList();

        public ExtraInDatabaseComparer(DatabaseModel databaseModel)
        {
            _databaseModel = databaseModel;
        }

        public void CompareLotsToDatabase(IReadOnlyList<CompareLog> firstStageLogs)
        {
            
        }

    }
}