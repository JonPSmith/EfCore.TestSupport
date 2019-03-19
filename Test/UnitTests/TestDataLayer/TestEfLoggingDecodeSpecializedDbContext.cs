// Copyright (c) 2017 Jon P Smith, GitHub: JonPSmith, web: http://www.thereformedprogrammer.net/
// Licensed under MIT licence. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using DataLayer.SpecialisedEntities;
using TestSupport.EfHelpers;
using Xunit;
using Xunit.Abstractions;
using Xunit.Extensions.AssertExtensions;

namespace Test.UnitTests.TestDataLayer
{
    public class TestEfLoggingDecodeSpecializedDbContext
    {
        private readonly ITestOutputHelper _output; 

        public TestEfLoggingDecodeSpecializedDbContext(ITestOutputHelper output) 
        {
            _output = output;
        }

        [Fact]
        public void TestAllTypesEntity()
        {
            //SETUP
            var logs = new List<LogOutput>();
            //var options = SqliteInMemory.CreateOptionsWithLogging<SpecializedDbContext>(log => logs.Add(log));
            var options = this.CreateUniqueClassOptionsWithLogging<SpecializedDbContext>(log => logs.Add(log));
            using (var context = new SpecializedDbContext(options))
            {
                context.Database.EnsureCreated();
                logs.Clear();

                var entity = new AllTypesEntity
                {
                    MyGuid = Guid.NewGuid(),
                    MyDateTime = new DateTime(2000, 1, 2),
                    MyDateTimeOffset = new DateTimeOffset(new DateTime(2004, 5, 6), new TimeSpan(1, 0, 0)),
                    MyTimeSpan = new TimeSpan(4, 5, 6),
                    MyByteArray = new byte[] {1, 2, 3}
                };
                context.Add(entity);
                context.SaveChanges();

                //ATTEMPT
                var decoded = logs.Last().DecodeMessage();

                //VERIFY  
                var sqlCommand = decoded.Split('\n').Skip(1).Select(x => x.Trim()).ToArray();
                int i = 0;
                sqlCommand[i++].ShouldEqual("SET NOCOUNT ON;");
                sqlCommand[i++].ShouldEqual("INSERT INTO [AllTypesEntities] ([MyAnsiNonNullString], [MyBool], [MyBoolNullable], [MyByteArray], [MyDateTime], [MyDateTimeNullable], [MyDateTimeOffset], [MyDecimal], [MyDouble], [MyGuid], [MyGuidNullable], [MyInt], [MyIntNullable], [MyString], [MyStringEmptyString], [MyStringNull], [MyTimeSpan])");
                //can't test for new Guid so do before and after
                sqlCommand[i].ShouldStartWith("VALUES ('ascii only', 1, NULL, '0x010203', '2000-01-02T00:00:00', NULL, '2004-05-06T00:00:00.0000000+01:00', '3456.789', '5678.9012', '");
                sqlCommand[i++].ShouldEndWith("', NULL, '1234', NULL, 'string with '' in it', NULL, NULL, '04:05:06');");
                sqlCommand[i++].ShouldEqual("SELECT [Id]");
                sqlCommand[i++].ShouldEqual("FROM [AllTypesEntities]");
                sqlCommand[i++].ShouldEqual("WHERE @@ROWCOUNT = 1 AND [Id] = scope_identity();");
            }
        }

    }
}