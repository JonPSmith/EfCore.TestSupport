using System.Collections;
using System.Collections.Generic;
using TestSupport.EfSchemeCompare;
using TestSupport.EfSchemeCompare.Internal;
using Xunit;
using Xunit.Abstractions;
using Xunit.Extensions.AssertExtensions;

namespace Test.UnitTests.EfSchemaCompare
{
    public class CompareLoggerTests
    {
        private readonly ITestOutputHelper _output;

        public CompareLoggerTests(ITestOutputHelper output)
        {
            _output = output;
        }

        [Fact]
        public void LogOK()
        {
            //SETUP
            bool errorLogged = false;
            var logs = new List<CompareLog>();
            var logger = new CompareLogger(CompareType.Entity, "Test", logs, null,
                () => { errorLogged = true; });

            //ATTEMPT
            logger.MarkAsOk("MyValue");

            //VERIFY
            errorLogged.ShouldBeFalse();
            logs.Count.ShouldEqual(1);
            logs[0].ToString().ShouldEqual("OK: Entity 'Test'");
        }

        [Fact]
        public void LogDifference()
        {
            //SETUP
            bool errorLogged = false;
            var logs = new List<CompareLog>();
            var logger = new CompareLogger(CompareType.Entity, "Test", logs, null,
                () => { errorLogged = true; });

            //ATTEMPT
            logger.Different("MyValue", "OtherValue", CompareAttributes.ColumnName);

            //VERIFY
            errorLogged.ShouldBeTrue();
            logs.Count.ShouldEqual(1);
            logs[0].ToString().ShouldEqual("DIFFERENT: Entity 'Test', column name. Expected = MyValue, found = OtherValue");
        }

        [Fact]
        public void LogNotInDatabase()
        {
            //SETUP
            bool errorLogged = false;
            var logs = new List<CompareLog>();
            var logger = new CompareLogger(CompareType.Entity, "Test", logs, null,
                () => { errorLogged = true; });

            //ATTEMPT
            logger.NotInDatabase("MyValue");

            //VERIFY
            errorLogged.ShouldBeTrue();
            logs.Count.ShouldEqual(1);
            logs[0].ToString().ShouldEqual("NOT IN DATABASE: Entity 'Test'. Expected = MyValue");
        }

        [Fact]
        public void LogExtraInDatabase()
        {
            //SETUP
            bool errorLogged = false;
            var logs = new List<CompareLog>();
            var logger = new CompareLogger(CompareType.Entity, "Test", logs, null,
                () => { errorLogged = true; });

            //ATTEMPT
            logger.ExtraInDatabase("MyValue", CompareAttributes.ColumnName);

            //VERIFY
            errorLogged.ShouldBeTrue();
            logs.Count.ShouldEqual(1);
            logs[0].ToString().ShouldEqual("EXTRA IN DATABASE: Entity 'Test', column name. Found = MyValue");
        }


        [Fact]
        public void LogNotInDatabaseIgnore()
        {
            //SETUP
            bool errorLogged = false;
            var logs = new List<CompareLog>();
            var config = new CompareEfSqlConfig();
            config.AddIgnoreCompareLog(new CompareLog(CompareType.Entity, CompareState.NotInDatabase, null));

            //ATTEMPT
            var logger = new CompareLogger(CompareType.Entity, "Test", logs, config.LogsToIgnore,
                () => { errorLogged = true; });
            logger.NotInDatabase("MyValue");

            //VERIFY
            errorLogged.ShouldBeFalse();
            logs.Count.ShouldEqual(0);
        }

    }
}
