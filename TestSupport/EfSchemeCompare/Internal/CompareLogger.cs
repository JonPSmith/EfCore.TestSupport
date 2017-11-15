// Copyright (c) 2017 Jon P Smith, GitHub: JonPSmith, web: http://www.thereformedprogrammer.net/
// Licensed under MIT licence. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;

namespace TestSupport.EfSchemeCompare.Internal
{

    internal class CompareLogger
    {
        private readonly CompareType _type;
        private readonly string _defaultName;
        private readonly IList<CompareLog> _compareLogs;
        private readonly IReadOnlyList<CompareLog> _ignoreList;
        private readonly Func<bool> _setErrorHasHappened;

        public CompareLogger(CompareType type, string defaultName, IList<CompareLog> compareLogs, IReadOnlyList<CompareLog> ignoreList, Func<bool> setErrorHasHappened)
        {
            _type = type;
            _defaultName = defaultName;
            _compareLogs = compareLogs;
            _ignoreList = ignoreList ?? new List<CompareLog>();
            _setErrorHasHappened = setErrorHasHappened;
        }

        public CompareLog MarkAsOk(string expected, string name = null)
        {
            var log = new CompareLog(_type, CompareState.Ok, name ?? _defaultName, CompareAttributes.NotSet, expected, null);
            _compareLogs.Add(log);
            return log;
        }

        public bool CheckDifferent(string expected, string found, CompareAttributes attribute, string name = null)
        {
            if (expected != found && expected?.Replace(" ", "") != found?.Replace(" ", ""))
            {
                AddToLogsIfNotIgnored(new CompareLog(_type, CompareState.Different, name ?? _defaultName, attribute, expected, found));
                return true;
            }
            return false;
        }

        public void Different(string expected, string found, CompareAttributes attribute, string name = null)
        {
            AddToLogsIfNotIgnored(new CompareLog(_type, CompareState.Different, name ?? _defaultName, attribute, expected, found));
        }

        public void NotInDatabase(string expected, CompareAttributes attribute = CompareAttributes.NotSet, string name = null)
        {
            AddToLogsIfNotIgnored(new CompareLog(_type, CompareState.NotInDatabase, name ?? _defaultName, attribute, expected, null));
        }

        public void ExtraInDatabase(string found, CompareAttributes attribute, string name = null)
        {
            AddToLogsIfNotIgnored(new CompareLog(_type, CompareState.ExtraInDatabase, name ?? _defaultName, attribute, null, found));
        }

        //------------------------------------------------------
        //private methods

        private void AddToLogsIfNotIgnored(CompareLog log)
        {
            if (!log.ShouldIIgnoreThisLog(_ignoreList))
            {
                _compareLogs.Add(log);
                _setErrorHasHappened();
            }
        }
    }
}