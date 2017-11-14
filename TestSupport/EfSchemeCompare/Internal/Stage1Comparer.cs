// Copyright (c) 2017 Jon P Smith, GitHub: JonPSmith, web: http://www.thereformedprogrammer.net/
// Licensed under MIT licence. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Runtime.CompilerServices;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Scaffolding.Metadata;

[assembly: InternalsVisibleTo("Test")]

namespace TestSupport.EfSchemeCompare.Internal
{
    internal class Stage1Comparer
    {
        private readonly IModel _model;
        private readonly string _dbContextName;

        private bool _hasErrors;

        private readonly List<CompareLog> _logs;
        public IReadOnlyList<CompareLog> Logs => _logs.ToImmutableList();

        public Stage1Comparer(IModel model, string dbContextName, List<CompareLog> logs = null)
        {
            _model = model;
            _dbContextName = dbContextName;
            _logs = logs ?? new List<CompareLog>();
        }

        public bool CompareModelToDatabase(DatabaseModel databaseModel)
        {
            var dbLogger = new CompareLogger(CompareType.DbContext, _dbContextName, _logs, () => _hasErrors = true);

            //Check things about the database, such as sequences
            dbLogger.MarkAsOk(_dbContextName);
            CheckDatabaseOk(_logs.Last(), _model.Relational(), databaseModel);

            var tableDict = databaseModel.Tables.ToDictionary(x => x.Name);
            foreach (var entityType in _model.GetEntityTypes())
            {
                var eRel = entityType.Relational();
                var logger = new CompareLogger(CompareType.Entity, entityType.ClrType.Name, _logs.Last().SubLogs, () => _hasErrors = true);
                if (tableDict.ContainsKey(eRel.TableName))
                {
                    var databaseTable = tableDict[eRel.TableName];
                    //Checks for table matching
                    var log = logger.MarkAsOk(eRel.TableName);
                    logger.CheckDifferent(entityType.FindPrimaryKey().Relational().Name, databaseTable.PrimaryKey.Name,
                        CompareAttributes.ConstraintName);
                    CompareColumns(log, entityType, databaseTable);
                    CompareForeignKeys(log, entityType, databaseTable);
                    CompareIndexes(log, entityType, databaseTable);
                }
                else
                {
                    logger.NotInDatabase(entityType.Relational().FormSchemaTable(), CompareAttributes.TableName);
                }
            }
            return _hasErrors;
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
                var logger = new CompareLogger(CompareType.ForeignKey, entityFKeyprops.CombinedColNames(), log.SubLogs, () => _hasErrors = true);
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
                    error |= logger.CheckDifferent(entityFKey.DeleteBehavior.ToString(), 
                        fKeyDict[constraintName].OnDelete.ConvertNullableReferentialAction(), CompareAttributes.DeleteBehaviour);
                    if (!error)
                        logger.MarkAsOk(constraintName);
                }
                else
                {
                    logger.NotInDatabase(constraintName, CompareAttributes.ConstraintName);
                }
            }
        }

        private void CompareIndexes(CompareLog log, IEntityType entityType, DatabaseTable table)
        {
            var indexDict = table.Indexes.ToDictionary(x => x.Name);
            foreach (var entityIdx in entityType.GetIndexes())
            {
                var entityIdxprops = entityIdx.Properties;
                var logger = new CompareLogger(CompareType.Index, entityIdxprops.CombinedColNames(), log.SubLogs, () => _hasErrors = true);
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
                    error |= logger.CheckDifferent(entityIdx.IsUnique.ToString(), 
                        indexDict[constraintName].IsUnique.ToString(), CompareAttributes.Unique);
                    if (!error)
                        logger.MarkAsOk(constraintName);
                }
                else
                {
                    logger.NotInDatabase(constraintName, CompareAttributes.ConstraintName);
                }
            }
        }

        private void CompareColumns(CompareLog log, IEntityType entityType, DatabaseTable table)
        {
            var columnDict = table.Columns.ToDictionary(x => x.Name);
            var primaryKeyDict = table.PrimaryKey.Columns.ToDictionary(x => x.Name);

            var efPKeyConstraintName = entityType.FindPrimaryKey().Relational().Name;
            bool pKeyError = false;
            var pKeyLogger = new CompareLogger(CompareType.PrimaryKey, efPKeyConstraintName, log.SubLogs, 
                () =>
                {
                    pKeyError = true;  //extra set of pKeyError
                    return _hasErrors = true;
                });
            pKeyLogger.CheckDifferent(efPKeyConstraintName, table.PrimaryKey.Name, CompareAttributes.ConstraintName);
            foreach (var property in entityType.GetProperties())
            {
                var pRel = property.Relational();
                var colLogger = new CompareLogger(CompareType.Property, property.Name, log.SubLogs, () => _hasErrors = true);

                if (columnDict.ContainsKey(pRel.ColumnName))
                {
                    var error = ComparePropertyToColumn(colLogger, property, columnDict[pRel.ColumnName]);
                    //check for primary key
                    if (property.IsPrimaryKey() != primaryKeyDict.ContainsKey(pRel.ColumnName))
                    {
                        if (!primaryKeyDict.ContainsKey(pRel.ColumnName))
                        {
                            pKeyLogger.NotInDatabase(pRel.ColumnName, CompareAttributes.ColumnName);
                            error = true;
                        }
                        else
                        {
                            pKeyLogger.ExtraInDatabase(pRel.ColumnName, CompareAttributes.ColumnName,
                                table.PrimaryKey.Name);
                        }
                    }

                    if (!error)
                    {
                        //There were no errors noted, so we mark it as OK
                        colLogger.MarkAsOk(pRel.ColumnName);
                    }
                }
                else
                {
                    colLogger.NotInDatabase(pRel.ColumnName, CompareAttributes.ColumnName);
                }
            }
            if (!pKeyError)
                pKeyLogger.MarkAsOk(efPKeyConstraintName);
        }

        private bool ComparePropertyToColumn(CompareLogger logger, IProperty property, DatabaseColumn column)
        {
            var error = logger.CheckDifferent(property.Relational().ColumnType, column.StoreType, CompareAttributes.ColumnType);
            error |= logger.CheckDifferent(property.IsNullable.NullableAsString(), column.IsNullable.NullableAsString(), CompareAttributes.Nullability);
            error |= logger.CheckDifferent(property.Relational().DefaultValueSql.RemoveUnnecessaryBrackets(), 
                column.DefaultValueSql.RemoveUnnecessaryBrackets(), CompareAttributes.DefaultValueSql);
            error |= logger.CheckDifferent(property.Relational().ComputedColumnSql.RemoveUnnecessaryBrackets(), 
                column.ComputedColumnSql.RemoveUnnecessaryBrackets(), CompareAttributes.ComputedColumnSql);
            error |= logger.CheckDifferent(property.ValueGenerated.ToString(), column.ValueGenerated.ConvertNullableValueGenerated(column.DefaultValueSql), CompareAttributes.ValueGenerated);
            return error;
        }

    }
}