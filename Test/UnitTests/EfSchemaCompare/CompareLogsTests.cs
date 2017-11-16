using System.Collections;
using System.Collections.Generic;
using TestSupport.EfSchemeCompare;
using Xunit;
using Xunit.Abstractions;
using Xunit.Extensions.AssertExtensions;

namespace Test.UnitTests.EfSchemaCompare
{
    public class CompareLogsTests
    {
        private readonly ITestOutputHelper _output;

        public CompareLogsTests(ITestOutputHelper output)
        {
            _output = output;
        }

        [Fact]
        public void DecodeStringToCompareLog()
        {
            //SETUP
            const string logStr1 =
                @"NOT IN DATABASE: BookDetail->ForeignKey 'FK_Books_Books_BookSummaryId', constraint name. Expected = FK_Books_Books_BookSummaryId";
            const string logStr2 =
                @"DIFFERENT: BookSummary->Property 'BookSummaryId', value generated. Expected = OnAdd, found = Never";
            const string logStr3 = @"OK: DbContext 'BookContext'";
            const string logStr4 =
                @"NOT IN DATABASE: BookDetail->ForeignKey 'FK_Books_Books_BookSummaryId', constraint name. Expected = FK_Books_Books_BookSummaryId";

            //ATTEMPT

            //VERIFY
            CompareLog.DecodeCompareTextToCompareLog(logStr1).ToString().ShouldEqual(logStr1.Replace("BookDetail->",""));
            CompareLog.DecodeCompareTextToCompareLog(logStr2).ToString().ShouldEqual(logStr2.Replace("BookSummary->", ""));
            CompareLog.DecodeCompareTextToCompareLog(logStr3).ToString().ShouldEqual(logStr3);
            CompareLog.DecodeCompareTextToCompareLog(logStr4).ToString().ShouldEqual(logStr4.Replace("BookDetail->", ""));
        }

        private class CompareIgnoreLogs : IEnumerable<object[]>
        {
            private readonly List<object[]> _data = new List<object[]>
            {
                new object[] {new CompareLog(CompareType.Column, CompareState.Different, null), true},
                new object[] {new CompareLog(CompareType.Column, CompareState.Different, "Name"), true},
                new object[] {new CompareLog(CompareType.Column, CompareState.Different, "DiffName"), false},
                new object[] {new CompareLog(CompareType.Column, CompareState.Different, "Name", CompareAttributes.ColumnName, "Expected"), true},
                new object[] {new CompareLog(CompareType.Column, CompareState.Different, "Name", CompareAttributes.ColumnType, "Expected"), false},
                new object[] {new CompareLog(CompareType.Column, CompareState.Different, "Name", CompareAttributes.ColumnType, "DiffExpected"), false},
            };

            public IEnumerator<object[]> GetEnumerator() => _data.GetEnumerator();
            IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
        }

        [Theory]
        [ClassData(typeof(CompareIgnoreLogs))]
        public void CheckIgnore(CompareLog ignoreItem, bool shouldIgnore)
        {
            //SETUP
            var list = new List<CompareLog>
            {
                ignoreItem
            };
            var log = new CompareLog(CompareType.Column, CompareState.Different, "Name", CompareAttributes.ColumnName,
                "Expected", "Found");

            //ATTEMPT
            var ignoreThis = log.ShouldIIgnoreThisLog(list);

            //VERIFY
            ignoreThis.ShouldEqual(shouldIgnore);

        }



    }
}
