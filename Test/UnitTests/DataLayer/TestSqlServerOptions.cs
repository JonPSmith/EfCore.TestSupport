// Copyright (c) 2017 Jon P Smith, GitHub: JonPSmith, web: http://www.thereformedprogrammer.net/
// Licensed under MIT licence. See License.txt in the project root for license information.

using System.Data.SqlClient;
using DataLayer.EfCode;
using Microsoft.EntityFrameworkCore;
using TestSupport.EfHelpers;
using Xunit;
using Xunit.Extensions.AssertExtensions;

namespace Test.UnitTests.DataLayer
{
    public class TestSqlServerOptions
    {

        [Fact]
        public void TestSqlServerUniqueClassOk()
        {
            //SETUP
            //ATTEMPT
            var options = this.CreateUniqueClassOptions<EfCoreContext>();
            using (var context = new EfCoreContext(options))
            {
                //VERIFY
                var builder = new SqlConnectionStringBuilder(context.Database.GetDbConnection().ConnectionString);
                builder.InitialCatalog.ShouldEndWith(GetType().Name);
            }
        }

        [Fact]
        public void TestSqlServerUniqueMethodOk()
        {
            //SETUP
            //ATTEMPT
            var options = this.CreateUniqueMethodOptions<EfCoreContext>();
            using (var context = new EfCoreContext(options))
            {

                //VERIFY
                var builder = new SqlConnectionStringBuilder(context.Database.GetDbConnection().ConnectionString);
                builder.InitialCatalog
                    .ShouldEndWith($"{GetType().Name}.{nameof(TestSqlServerUniqueMethodOk)}" );
            }
        }

    }
}