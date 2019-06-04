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
        private readonly Action _setErrorHasHappened;

        public CompareLogger(CompareType type, string defaultName, IList<CompareLog> compareLogs,
            IReadOnlyList<CompareLog> ignoreList, Action setErrorHasHappened)
        {
            _type = type;
            _defaultName = defaultName;
            _compareLogs = compareLogs;
            _ignoreList = ignoreList ?? new List<CompareLog>();
            _setErrorHasHappened = setErrorHasHappened;
        }

        public CompareLog MarkAsOk(string expected, string name = null)
        {
            var log = new CompareLog(_type, CompareState.Ok, name ?? _defaultName, CompareAttributes.NotSet, expected,
                null);
            _compareLogs.Add(log);
            return log;
        }

        public bool CheckDifferent(string expected, string found, CompareAttributes attribute, string name = null)
        {
            if (expected != found && expected?.Replace(" ", "") != found?.Replace(" ", ""))
            {
                return AddToLogsIfNotIgnored(new CompareLog(_type, CompareState.Different, name ?? _defaultName,
                    attribute, expected, found));
            }

            return false;
        }

        public void Different(string expected, string found, CompareAttributes attribute, string name = null)
        {
            AddToLogsIfNotIgnored(new CompareLog(_type, CompareState.Different, name ?? _defaultName, attribute,
                expected, found));
        }

        public void NotInDatabase(string expected, CompareAttributes attribute = CompareAttributes.NotSet,
            string name = null)
        {
            AddToLogsIfNotIgnored(new CompareLog(_type, CompareState.NotInDatabase, name ?? _defaultName, attribute,
                expected, null));
        }

        public void ExtraInDatabase(string found, CompareAttributes attribute, string name = null)
        {
            AddToLogsIfNotIgnored(new CompareLog(_type, CompareState.ExtraInDatabase, name ?? _defaultName, attribute,
                null, found));
        }

        /// <summary>
        /// This is for adding a warning.
        /// </summary>
        /// <param name="errorMessage">This should be the warning message</param>
        /// <param name="expected">add this if something was missing</param>
        /// <param name="found">add this if something extra was found</param>
        public void Warning(string errorMessage, string expected = null, string found = null)
        {
            AddToLogsIfNotIgnored(new CompareLog(CompareType.Database, CompareState.Warning, errorMessage,
                CompareAttributes.NotSet, expected, found));
        }

        //------------------------------------------------------
        //private methods

        /// <summary>
        /// Only adds the 
        /// </summary>
        /// <param name="log"></param>
        /// <returns></returns>
        private bool AddToLogsIfNotIgnored(CompareLog log)
        {
            if (!log.ShouldIIgnoreThisLog(_ignoreList))
            {
                _compareLogs.Add(log);
                _setErrorHasHappened();
                return true;
            }

            return false;
        }
    }
}