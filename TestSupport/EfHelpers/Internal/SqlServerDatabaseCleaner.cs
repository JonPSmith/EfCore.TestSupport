// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.
// Altered by Jon P Smith, GitHub @JonPSmith, https://www.thereformedprogrammer.net/

using System.Runtime.CompilerServices;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations.Operations;
using Microsoft.EntityFrameworkCore.Scaffolding;
using Microsoft.EntityFrameworkCore.Scaffolding.Metadata;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

[assembly: InternalsVisibleTo("Test")]

namespace TestSupport.EfHelpers.Internal
{
    internal class SqlServerDatabaseCleaner : RelationalDatabaseCleaner
    {
        private readonly DatabaseFacade _databaseFacade;
        public SqlServerDatabaseCleaner(DatabaseFacade databaseFacade)
        {
            _databaseFacade = databaseFacade;
        }

        //protected override IDatabaseModelFactory CreateDatabaseModelFactory(ILoggerFactory loggerFactory)
        //    => new SqlServerDatabaseModelFactory(
        //        new DiagnosticsLogger<DbLoggerCategory.Scaffolding>(
        //            loggerFactory,
        //            new LoggingOptions(),
        //            new DiagnosticListener("Fake"),
        //            new SqlServerLoggingDefinitions(),
        //            new NullDbContextLogger()));

        protected override IDatabaseModelFactory CreateDatabaseModelFactory(ILoggerFactory loggerFactory)
        {
            var designTimeService = _databaseFacade.GetDesignTimeService();
            var serviceProvider = designTimeService.GetDesignTimeProvider();
            return serviceProvider.GetService<IDatabaseModelFactory>();
        }

        protected override bool AcceptTable(DatabaseTable table) => !(table is DatabaseView);

        protected override bool AcceptIndex(DatabaseIndex index)
            => false;

        private readonly string _dropViewsSql = @"
DECLARE @name VARCHAR(MAX) = '__dummy__', @SQL VARCHAR(MAX) = '';

WHILE @name IS NOT NULL
BEGIN
    SELECT @name =
    (SELECT TOP 1 QUOTENAME(s.[name]) + '.' + QUOTENAME(o.[name])
     FROM sysobjects o
     INNER JOIN sys.views v ON o.id = v.object_id
     INNER JOIN sys.schemas s ON s.schema_id = v.schema_id
     WHERE (s.name = 'dbo' OR s.principal_id <> s.schema_id) AND o.[type] = 'V' AND o.category = 0 AND o.[name] NOT IN
     (
        SELECT referenced_entity_name
        FROM sys.sql_expression_dependencies AS sed
        INNER JOIN sys.objects AS o ON sed.referencing_id = o.object_id
     )
     ORDER BY v.[name])

    SELECT @SQL = 'DROP VIEW ' + @name
    EXEC (@SQL)
END";

        protected override string BuildCustomSql(DatabaseModel databaseModel)
            => _dropViewsSql;

        protected override string BuildCustomEndingSql(DatabaseModel databaseModel)
            => _dropViewsSql
                + @"
GO

DECLARE @SQL VARCHAR(MAX) = '';
SELECT @SQL = @SQL + 'DROP FUNCTION ' + QUOTENAME(ROUTINE_SCHEMA) + '.' + QUOTENAME(ROUTINE_NAME) + ';'
  FROM [INFORMATION_SCHEMA].[ROUTINES] WHERE ROUTINE_TYPE = 'FUNCTION' AND ROUTINE_BODY = 'SQL';
EXEC (@SQL);

SET @SQL ='';
SELECT @SQL = @SQL + 'DROP AGGREGATE ' + QUOTENAME(ROUTINE_SCHEMA) + '.' + QUOTENAME(ROUTINE_NAME) + ';'
  FROM [INFORMATION_SCHEMA].[ROUTINES] WHERE ROUTINE_TYPE = 'FUNCTION' AND ROUTINE_BODY = 'EXTERNAL';
EXEC (@SQL);

SET @SQL ='';
SELECT @SQL = @SQL + 'DROP PROC ' + QUOTENAME(schema_name(schema_id)) + '.' + QUOTENAME(name) + ';' FROM sys.procedures;
EXEC (@SQL);

SET @SQL ='';
SELECT @SQL = @SQL + 'DROP TYPE ' + QUOTENAME(schema_name(schema_id)) + '.' + QUOTENAME(name) + ';' FROM sys.types WHERE is_user_defined = 1;
EXEC (@SQL);

SET @SQL ='';
SELECT @SQL = @SQL + 'DROP SCHEMA ' + QUOTENAME(name) + ';' FROM sys.schemas WHERE principal_id <> schema_id;
EXEC (@SQL);";

        protected override MigrationOperation Drop(DatabaseTable table)
            => AddMemoryOptimizedAnnotation(base.Drop(table), table);

        protected override MigrationOperation Drop(DatabaseForeignKey foreignKey)
            => AddMemoryOptimizedAnnotation(base.Drop(foreignKey), foreignKey.Table);

        protected override MigrationOperation Drop(DatabaseIndex index)
            => AddMemoryOptimizedAnnotation(base.Drop(index), index.Table);

        private const string SqlServerMemoryOptimized = "SqlServer:MemoryOptimized";

        private static TOperation AddMemoryOptimizedAnnotation<TOperation>(TOperation operation, DatabaseTable table)
            where TOperation : MigrationOperation
        {
            operation[SqlServerMemoryOptimized]
                = table[SqlServerMemoryOptimized] as bool?;

            return operation;
        }
    }
}
