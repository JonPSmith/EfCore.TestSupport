// Copyright (c) 2017 Jon P Smith, GitHub: JonPSmith, web: http://www.thereformedprogrammer.net/
// Licensed under MIT licence. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Scaffolding.Metadata;
using TestSupport.EfSchemeCompare.Internal;

namespace TestSupport.EfSchemeCompare
{
    public class DbContextComparer
    {
        private readonly IModel _model;
        private readonly Type _contextType;

        private readonly List<CompareLog> _logs = new List<CompareLog>();

        public IReadOnlyList<CompareLog> Logs => _logs.ToImmutableList();

        public DbContextComparer(DbContext context)
        {
            _model = context.Model;
            _contextType = context.GetType();
        }

        public void CompareModelToDatabase(DatabaseModel databaseModel)
        {
            var dbLogger = new CompareLogger(CompareType.DbContext, _contextType.Name, _logs);

            //Check things about the database, such as sequences
            dbLogger.MarkAsOk(_contextType.Name);
            CheckDatabaseOk(_logs.Last(), _model.Relational(), databaseModel);

            var tableDict = databaseModel.Tables.ToDictionary(x => x.Name);
            foreach (var entityType in _model.GetEntityTypes())
            {
                var eRel = entityType.Relational();
                var logger = new CompareLogger(CompareType.Table, entityType.Relational().FormSchemaTable(), _logs.Last().SubLogs);
                if (tableDict.ContainsKey(eRel.TableName))
                {
                    //Checks for table matching
                    var log = logger.MarkAsOk(eRel.TableName);
                    CompareColumns(log, entityType, tableDict[eRel.TableName]);
                    CompareForeignKeys(log, entityType, tableDict[eRel.TableName]);
                    CompareIndexes(log, entityType, tableDict[eRel.TableName]);
                }
                else
                {
                    logger.NotInDatabase(entityType.Relational().FormSchemaTable());
                }
            }
        }

        private void CheckDatabaseOk(CompareLog log, IRelationalModelAnnotations modelRel, DatabaseModel databaseModel)
        {
            //Check sequences
            //var logger = new CompareLogger(CompareType.Sequence, <sequence name>, _logs);
        }


        private void CompareForeignKeys(CompareLog log, IEntityType entityType, DatabaseTable table)
        {
            var fKeyDict = table.ForeignKeys.ToDictionary(x => x.Name);

            foreach (var entityFKey in entityType.GetForeignKeys())
            {
                var entityFKeyprops = entityFKey.Properties;
                var logger = new CompareLogger(CompareType.ForeignKey, entityFKeyprops.CombinedColNames(), log.SubLogs);
                var constraintName = entityFKey.Relational().Name;
                if (fKeyDict.ContainsKey(constraintName))
                {       
                    //Now check every foreign key
                    var error = false;
                    var thisKeyCols = fKeyDict[constraintName].Columns.ToDictionary(x => x.Name);
                    foreach (var fKeyProp in entityFKeyprops)
                    {
                        var pRel = fKeyProp.Relational();
                        if (!thisKeyCols.ContainsKey(pRel.ColumnName))
                        {
                            logger.NotInDatabase(pRel.ColumnName);
                            error = true;
                        }
                    }
                    error |= logger.CheckDifferent(entityFKey.DeleteBehavior.ToString(), fKeyDict[constraintName].OnDelete.ConvertNullableReferentialAction(), "delete behavour");
                    if (!error)
                        logger.MarkAsOk(constraintName);
                }
                else
                {
                    logger.NotInDatabase(constraintName, "constraint name");
                }
            }
        }

        private void CompareIndexes(CompareLog log, IEntityType entityType, DatabaseTable table)
        {
            var indexDict = table.Indexes.ToDictionary(x => x.Name);
            foreach (var entityIdx in entityType.GetIndexes())
            {
                var entityIdxprops = entityIdx.Properties;
                var logger = new CompareLogger(CompareType.ForeignKey, entityIdxprops.CombinedColNames(), log.SubLogs);
                var constraintName = entityIdx.Relational().Name;
                if (indexDict.ContainsKey(constraintName))
                {
                    //Now check every column in an index
                    var error = false;
                    var thisIdxCols = indexDict[constraintName].Columns.ToDictionary(x => x.Name);
                    foreach (var idxProp in entityIdxprops)
                    {
                        var pRel = idxProp.Relational();
                        if (!thisIdxCols.ContainsKey(pRel.ColumnName))
                        {
                            logger.NotInDatabase(pRel.ColumnName);
                            error = true;
                        } 
                    }
                    error |= logger.CheckDifferent(entityIdx.IsUnique.ToString(), indexDict[constraintName].IsUnique.ToString(), "unique");
                    if (!error)
                        logger.MarkAsOk(constraintName);
                }
                else
                {
                    logger.NotInDatabase(constraintName, "constraint name");
                }
            }
        }

        private void CompareColumns(CompareLog log, IEntityType entityType, DatabaseTable table)
        {
            var columnDict = table.Columns.ToDictionary(x => x.Name);
            var primaryKeyDict = table.PrimaryKey.Columns.ToDictionary(x => x.Name);
            foreach (var property in entityType.GetProperties())
            {
                var pRel = property.Relational();
                var logger = new CompareLogger(CompareType.Column, property.Relational().ColumnName, log.SubLogs);
                if (columnDict.ContainsKey(pRel.ColumnName))
                {
                    var error = ComparePropertyToColumn(logger, property, columnDict[pRel.ColumnName]);
                    //check for primary key
                    error |= logger.CheckDifferent(property.IsPrimaryKey().ToString(), primaryKeyDict.ContainsKey(pRel.ColumnName).ToString(), "primary key");

                    if (!error)
                    {
                        //There were no errors noted, so we mark it as OK
                        logger.MarkAsOk(pRel.ColumnName);
                    }
                }
                else
                {
                    logger.NotInDatabase(pRel.ColumnName);
                }
            }
        }

        private bool ComparePropertyToColumn(CompareLogger logger, IProperty property, DatabaseColumn column)
        {
            var error = logger.CheckDifferent(property.Relational().ColumnType, column.StoreType, "column type");
            error |= logger.CheckDifferent(property.IsNullable.NullableAsString(), column.IsNullable.NullableAsString(), "nullability");
            error |= logger.CheckDifferent(property.Relational().DefaultValueSql, column.DefaultValueSql, "sql default value");
            error |= logger.CheckDifferent(property.Relational().ComputedColumnSql, column.ComputedColumnSql, "computed column");
            error |= logger.CheckDifferent(property.ValueGenerated.ToString(), column.ValueGenerated.ConvertNullableValueGenerated(), "value generated");
            return error;
        }

    }
}