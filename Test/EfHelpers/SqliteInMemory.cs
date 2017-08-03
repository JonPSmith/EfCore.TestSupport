// Copyright (c) 2016 Jon P Smith, GitHub: JonPSmith, web: http://www.thereformedprogrammer.net/
// Licensed under MIT licence. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Collections.Immutable;
using DataLayer.EfCode;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.Extensions.Logging;

namespace Test.EfHelpers
{
    public class SqliteInMemory
    {
        private readonly List<string> _logs = new List<string>();

        public ImmutableList<string> Logs => _logs.ToImmutableList();

        public void ClearLogs() { _logs.Clear();}

        public DbContextWithSchema GetContextWithSetup()
        {
            var context = new DbContextWithSchema(CreateOptions<DbContextWithSchema>());
            //context.Database.OpenConnection();
            context.Database.EnsureCreated();

            SetupLogging(context, _logs);

            return context;
        }

        public static void SetupLogging(DbContext context, List<string> logs)
        {
            var loggerFactory = context.GetService<ILoggerFactory>();
            loggerFactory.AddProvider(new MyLoggerProvider(logs));
        }

        public static DbContextOptions<T> CreateOptions<T>() where T : DbContext
        {
            //Thanks to https://www.scottbrady91.com/Entity-Framework/Entity-Framework-Core-In-Memory-Testing
            var connectionStringBuilder =
                new SqliteConnectionStringBuilder { DataSource = ":memory:" };
            var connectionString = connectionStringBuilder.ToString();
            var connection = new SqliteConnection(connectionString);
            connection.Open();                //see https://github.com/aspnet/EntityFramework/issues/6968

            // create in-memory context
            var builder = new DbContextOptionsBuilder<T>();
            builder.UseSqlite(connection);

            return builder.Options;
        }

    }
}