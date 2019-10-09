// Copyright (c) 2017 Jon P Smith, GitHub: JonPSmith, web: http://www.thereformedprogrammer.net/
// Licensed under MIT licence. See License.txt in the project root for license information.

using System;
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
        private readonly IReadOnlyList<CompareLog> _ignoreList;
        private readonly StringComparer _caseComparer;
        private readonly StringComparison _caseComparison;
        private bool _hasErrors;

        private readonly List<CompareLog> _logs;
        public IReadOnlyList<CompareLog> Logs => _logs.ToImmutableList();

        public Stage1Comparer(IModel model, string dbContextName, CompareEfSqlConfig config = null, List<CompareLog> logs = null)
        {
            _model = model;
            _dbContextName = dbContextName;
            _logs = logs ?? new List<CompareLog>();
            _ignoreList = config?.LogsToIgnore ?? new List<CompareLog>();
            _caseComparer = config?.CaseComparer ?? StringComparer.CurrentCulture;
            _caseComparison = _caseComparer.GetStringComparison();
        }

#if NETSTANDARD2_0
        public bool CompareModelToDatabase(DatabaseModel databaseModel)
        {
            var dbLogger = new CompareLogger(CompareType.DbContext, _dbContextName, _logs, _ignoreList, () => _hasErrors = true);

            //Check things about the database, such as sequences
            dbLogger.MarkAsOk(_dbContextName);
            CheckDatabaseOk(_logs.Last(), _model.Relational(), databaseModel);

            var tableDict = databaseModel.Tables.ToDictionary(x => x.FormSchemaTable(databaseModel.DefaultSchema), _caseComparer);
            var dbQueries = _model.GetEntityTypes().Where(x => x.IsQueryType).ToList();
            if (dbQueries.Any())
                dbLogger.Warning("EfSchemaCompare does not check DbQuery types", null, string.Join(", ", dbQueries.Select(x => x.ClrType.Name)));
            foreach (var entityType in _model.GetEntityTypes().Where(x => !x.IsQueryType))
            {
                var eRel = entityType.Relational();
                var logger = new CompareLogger(CompareType.Entity, entityType.ClrType.Name, _logs.Last().SubLogs, _ignoreList, () => _hasErrors = true);
                if (tableDict.ContainsKey(eRel.FormSchemaTable()))
                {
                    var databaseTable = tableDict[eRel.FormSchemaTable()];
                    //Checks for table matching
                    var log = logger.MarkAsOk(eRel.FormSchemaTable());
                    logger.CheckDifferent(entityType.FindPrimaryKey().Relational().Name, databaseTable.PrimaryKey.Name,
                        CompareAttributes.ConstraintName, _caseComparison);
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
            var fKeyDict = table.ForeignKeys.ToDictionary(x => x.Name, _caseComparer);

            foreach (var entityFKey in entityType.GetForeignKeys())
            {
                var entityFKeyprops = entityFKey.Properties;
                var constraintName = entityFKey.Relational().Name;
                var logger = new CompareLogger(CompareType.ForeignKey, constraintName, log.SubLogs, _ignoreList, () => _hasErrors = true);
                if (IgnoreForeignKeyIfInSameTable(entityType, entityFKey, table))
                    continue;
                if (fKeyDict.ContainsKey(constraintName))
                {       
                    //Now check every foreign key
                    var error = false;
                    var thisKeyCols = fKeyDict[constraintName].Columns.ToDictionary(x => x.Name, _caseComparer);
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
                        fKeyDict[constraintName].OnDelete.ConvertReferentialActionToDeleteBehavior(entityFKey.DeleteBehavior), 
                            CompareAttributes.DeleteBehaviour, _caseComparison);
                    if (!error)
                        logger.MarkAsOk(constraintName);
                }
                else
                {
                    logger.NotInDatabase(constraintName, CompareAttributes.ConstraintName);
                }
            }
        }

        private bool IgnoreForeignKeyIfInSameTable(IEntityType entityType, IForeignKey entityFKey, DatabaseTable table)
        {
            if (entityType.DefiningEntityType != null &&
                string.Equals(entityType.DefiningEntityType.Relational().TableName, table.Name, _caseComparison))
                //if a owned table, and the owned entity's table matches this table then ignore
                return true;

            //see https://github.com/aspnet/EntityFrameworkCore/issues/10345#issuecomment-345841191
            if (entityFKey.Properties.All(x => string.Equals( x.DeclaringEntityType.Relational().TableName,  table.Name, _caseComparison))
                 && entityFKey.Properties.Select(p => p.Relational().ColumnName)
                    .SequenceEqual(entityFKey.PrincipalKey.Properties.Select(p => p.Relational().ColumnName)))
                //If all the declaring entity type of the foreign key are all in this table, then we ignore this (table splitting case)
                return true;

            //Otherwise we should not ignore it
            return false;
        }

        private void CompareIndexes(CompareLog log, IEntityType entityType, DatabaseTable table)
        {
            var indexDict = DatabaseIndexData.GetIndexesAndUniqueConstraints(table). ToDictionary(x => x.Name, _caseComparer);
            foreach (var entityIdx in entityType.GetIndexes())
            {
                var entityIdxprops = entityIdx.Properties;
                var logger = new CompareLogger(CompareType.Index, entityIdxprops.CombinedColNames(), log.SubLogs, _ignoreList, () => _hasErrors = true);
                var constraintName = entityIdx.Relational().Name;
                if (indexDict.ContainsKey(constraintName))
                {
                    //Now check every column in an index
                    var error = false;
                    var thisIdxCols = indexDict[constraintName].Columns.ToDictionary(x => x.Name, _caseComparer);
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
                        indexDict[constraintName].IsUnique.ToString(), CompareAttributes.Unique, _caseComparison);
                    if (!error)
                        logger.MarkAsOk(constraintName);
                }
                else
                {
                    logger.NotInDatabase(constraintName, CompareAttributes.IndexConstraintName);
                }
            }
        }

        private void CompareColumns(CompareLog log, IEntityType entityType, DatabaseTable table)
        {
            var columnDict = table.Columns.ToDictionary(x => x.Name, _caseComparer);
            var primaryKeyDict = table.PrimaryKey.Columns.ToDictionary(x => x.Name, _caseComparer);

            var efPKeyConstraintName = entityType.FindPrimaryKey().Relational().Name;
            bool pKeyError = false;
            var pKeyLogger = new CompareLogger(CompareType.PrimaryKey, efPKeyConstraintName, log.SubLogs, _ignoreList,
                () =>
                {
                    pKeyError = true;  //extra set of pKeyError
                    _hasErrors = true;
                });
            pKeyLogger.CheckDifferent(efPKeyConstraintName, table.PrimaryKey.Name, CompareAttributes.ConstraintName, _caseComparison);
            foreach (var property in entityType.GetProperties())
            {
                var pRel = property.Relational();
                var colLogger = new CompareLogger(CompareType.Property, property.Name, log.SubLogs, _ignoreList, () => _hasErrors = true);

                if (columnDict.ContainsKey(pRel.ColumnName))
                {
                    if (!IgnorePrimaryKeyFoundInOwnedTypes(entityType.DefiningEntityType, table, property, entityType.FindPrimaryKey()))
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
                }
                else
                {
                    colLogger.NotInDatabase(pRel.ColumnName, CompareAttributes.ColumnName);
                }
            }
            if (!pKeyError)
                pKeyLogger.MarkAsOk(efPKeyConstraintName);
        }

        private bool IgnorePrimaryKeyFoundInOwnedTypes(IEntityType entityTypeDefiningEntityType, DatabaseTable table, 
            IProperty property, IKey primaryKey)
        {
            if (entityTypeDefiningEntityType == null ||
                !string.Equals(entityTypeDefiningEntityType.Relational().TableName,table.Name, _caseComparison))
                //if not a owned table, or the owned tabl has its own table then carry on
                return false;

            //Now we know that its an owned table, and it has a primary key which matches the table
            if (!primaryKey.Properties.Contains(property))
                return false;

            //It is a primary key so don't consider it as that is checked in the rest of the code
            return true;
        }

        private bool ComparePropertyToColumn(CompareLogger logger, IProperty property, DatabaseColumn column)
        {
            var pRel = property.Relational();;
            var error = logger.CheckDifferent(pRel.ColumnType, column.StoreType, CompareAttributes.ColumnType, _caseComparison);
            error |= logger.CheckDifferent(property.IsNullable.NullableAsString(), column.IsNullable.NullableAsString(), CompareAttributes.Nullability, _caseComparison);
            error |= logger.CheckDifferent(pRel.ComputedColumnSql.RemoveUnnecessaryBrackets(), 
                column.ComputedColumnSql.RemoveUnnecessaryBrackets(), CompareAttributes.ComputedColumnSql, _caseComparison);
            var defaultValue = pRel.DefaultValueSql ?? pRel.DefaultValue?.ToString();
            error |= logger.CheckDifferent(defaultValue.RemoveUnnecessaryBrackets(),
                    column.DefaultValueSql.RemoveUnnecessaryBrackets(), CompareAttributes.DefaultValueSql, _caseComparison);
            error |= CheckValueGenerated(logger, property, column);
            return error;
        }

        //thanks to https://stackoverflow.com/questions/1749966/c-sharp-how-to-determine-whether-a-type-is-a-number
        private static HashSet<Type> IntegerTypes = new HashSet<Type>
        {
            typeof(byte), typeof(sbyte), typeof(short), typeof(ushort), typeof(int), typeof(uint), typeof(long), typeof(ulong)
        };

        private bool CheckValueGenerated(CompareLogger logger, IProperty property, DatabaseColumn column)
        {
            var colValGen = column.ValueGenerated.ConvertNullableValueGenerated(column.ComputedColumnSql, column.DefaultValueSql);            
            if (colValGen == ValueGenerated.Never.ToString()
                //There is a case where the property is part of the primary key and the key is not set in the database
                && property.ValueGenerated == ValueGenerated.OnAdd
                && property.IsKey()
                //We assume that a integer of some form should be provided by the database
                && !IntegerTypes.Contains(property.ClrType))
                return false;
            return logger.CheckDifferent(property.ValueGenerated.ToString(),
                colValGen, CompareAttributes.ValueGenerated, _caseComparison);
        }
#elif NETSTANDARD2_1
        public bool CompareModelToDatabase(DatabaseModel databaseModel)
        {
            var dbLogger = new CompareLogger(CompareType.DbContext, _dbContextName, _logs, _ignoreList, () => _hasErrors = true);

            //Check things about the database, such as sequences
            dbLogger.MarkAsOk(_dbContextName);
            CheckDatabaseOk(_logs.Last(), _model, databaseModel);

            var tableDict = databaseModel.Tables.ToDictionary(x => x.FormSchemaTable(databaseModel.DefaultSchema), _caseComparer);
            var dbQueries = _model.GetEntityTypes().Where(x => x.FindPrimaryKey() == null).ToList();
            if (dbQueries.Any())
                dbLogger.Warning("EfSchemaCompare does not check read-only types", null, string.Join(", ", dbQueries.Select(x => x.ClrType.Name)));
            foreach (var entityType in _model.GetEntityTypes().Where(x => x.FindPrimaryKey() != null))
            {
                var logger = new CompareLogger(CompareType.Entity, entityType.ClrType.Name, _logs.Last().SubLogs, _ignoreList, () => _hasErrors = true);
                if (tableDict.ContainsKey(entityType.FormSchemaTable()))
                {
                    var databaseTable = tableDict[entityType.FormSchemaTable()];
                    //Checks for table matching
                    var log = logger.MarkAsOk(entityType.FormSchemaTable());
                    logger.CheckDifferent(entityType.FindPrimaryKey()?.GetName(), databaseTable.PrimaryKey.Name,
                        CompareAttributes.ConstraintName, _caseComparison);
                    CompareColumns(log, entityType, databaseTable);
                    CompareForeignKeys(log, entityType, databaseTable);
                    CompareIndexes(log, entityType, databaseTable);
                }
                else
                {
                    logger.NotInDatabase(entityType.FormSchemaTable(), CompareAttributes.TableName);
                }
            }
            return _hasErrors;
        }

        private void CheckDatabaseOk(CompareLog log, IModel modelRel, DatabaseModel databaseModel)
        {
            //Check sequences
            //var logger = new CompareLogger(CompareType.Sequence, <sequence name>, _logs);
        }


        private void CompareForeignKeys(CompareLog log, IEntityType entityType, DatabaseTable table)
        {
            var fKeyDict = table.ForeignKeys.ToDictionary(x => x.Name, _caseComparer);

            foreach (var entityFKey in entityType.GetForeignKeys())
            {
                var entityFKeyprops = entityFKey.Properties;
                var constraintName = entityFKey.GetConstraintName();
                var logger = new CompareLogger(CompareType.ForeignKey, constraintName, log.SubLogs, _ignoreList, () => _hasErrors = true);
                if (IgnoreForeignKeyIfInSameTable(entityType, entityFKey, table))
                    continue;
                if (fKeyDict.ContainsKey(constraintName))
                {
                    //Now check every foreign key
                    var error = false;
                    var thisKeyCols = fKeyDict[constraintName].Columns.ToDictionary(x => x.Name, _caseComparer);
                    foreach (var fKeyProp in entityFKeyprops)
                    {
                        var columnName = fKeyProp.GetColumnName();
                        if (!thisKeyCols.ContainsKey(columnName))
                        {
                            logger.NotInDatabase(columnName);
                            error = true;
                        }
                    }
                    error |= logger.CheckDifferent(entityFKey.DeleteBehavior.ToString(),
                        fKeyDict[constraintName].OnDelete.ConvertReferentialActionToDeleteBehavior(entityFKey.DeleteBehavior),
                            CompareAttributes.DeleteBehaviour, _caseComparison);
                    if (!error)
                        logger.MarkAsOk(constraintName);
                }
                else
                {
                    logger.NotInDatabase(constraintName, CompareAttributes.ConstraintName);
                }
            }
        }

        private bool IgnoreForeignKeyIfInSameTable(IEntityType entityType, IForeignKey entityFKey, DatabaseTable table)
        {
            if (entityType.DefiningEntityType != null &&
                string.Equals(entityType.DefiningEntityType.GetTableName(), table.Name, _caseComparison))
                //if a owned table, and the owned entity's table matches this table then ignore
                return true;

            //see https://github.com/aspnet/EntityFrameworkCore/issues/10345#issuecomment-345841191
            if (entityFKey.Properties.All(x => string.Equals(x.DeclaringEntityType.GetTableName(), table.Name, _caseComparison))
                 && entityFKey.Properties.Select(p => p.GetColumnName())
                    .SequenceEqual(entityFKey.PrincipalKey.Properties.Select(p => p.GetColumnName())))
                //If all the declaring entity type of the foreign key are all in this table, then we ignore this (table splitting case)
                return true;

            //Otherwise we should not ignore it
            return false;
        }

        private void CompareIndexes(CompareLog log, IEntityType entityType, DatabaseTable table)
        {
            var indexDict = DatabaseIndexData.GetIndexesAndUniqueConstraints(table).ToDictionary(x => x.Name, _caseComparer);
            foreach (var entityIdx in entityType.GetIndexes())
            {
                var entityIdxprops = entityIdx.Properties;
                var logger = new CompareLogger(CompareType.Index, entityIdxprops.CombinedColNames(), log.SubLogs, _ignoreList, () => _hasErrors = true);
                var constraintName = entityIdx.GetName();
                if (indexDict.ContainsKey(constraintName))
                {
                    //Now check every column in an index
                    var error = false;
                    var thisIdxCols = indexDict[constraintName].Columns.ToDictionary(x => x.Name, _caseComparer);
                    foreach (var idxProp in entityIdxprops)
                    {
                        var columnName = idxProp.GetColumnName();
                        if (!thisIdxCols.ContainsKey(columnName))
                        {
                            logger.NotInDatabase(columnName);
                            error = true;
                        }
                    }
                    error |= logger.CheckDifferent(entityIdx.IsUnique.ToString(),
                        indexDict[constraintName].IsUnique.ToString(), CompareAttributes.Unique, _caseComparison);
                    if (!error)
                        logger.MarkAsOk(constraintName);
                }
                else
                {
                    logger.NotInDatabase(constraintName, CompareAttributes.IndexConstraintName);
                }
            }
        }

        private void CompareColumns(CompareLog log, IEntityType entityType, DatabaseTable table)
        {
            var columnDict = table.Columns.ToDictionary(x => x.Name, _caseComparer);
            var primaryKeyDict = table.PrimaryKey.Columns.ToDictionary(x => x.Name, _caseComparer);

            var efPKeyConstraintName = entityType.FindPrimaryKey().GetName();
            bool pKeyError = false;
            var pKeyLogger = new CompareLogger(CompareType.PrimaryKey, efPKeyConstraintName, log.SubLogs, _ignoreList,
                () =>
                {
                    pKeyError = true;  //extra set of pKeyError
                    _hasErrors = true;
                });
            pKeyLogger.CheckDifferent(efPKeyConstraintName, table.PrimaryKey.Name, CompareAttributes.ConstraintName, _caseComparison);
            foreach (var property in entityType.GetProperties())
            {
                var colLogger = new CompareLogger(CompareType.Property, property.Name, log.SubLogs, _ignoreList, () => _hasErrors = true);

                if (columnDict.ContainsKey(property.GetColumnName()))
                {
                    if (!IgnorePrimaryKeyFoundInOwnedTypes(entityType.DefiningEntityType, table, property, entityType.FindPrimaryKey()))
                    {
                        var error = ComparePropertyToColumn(colLogger, property, columnDict[property.GetColumnName()]);
                        //check for primary key
                        if (property.IsPrimaryKey() != primaryKeyDict.ContainsKey(property.GetColumnName()))
                        {
                            if (!primaryKeyDict.ContainsKey(property.GetColumnName()))
                            {
                                pKeyLogger.NotInDatabase(property.GetColumnName(), CompareAttributes.ColumnName);
                                error = true;
                            }
                            else
                            {
                                pKeyLogger.ExtraInDatabase(property.GetColumnName(), CompareAttributes.ColumnName,
                                    table.PrimaryKey.Name);
                            }
                        }

                        if (!error)
                        {
                            //There were no errors noted, so we mark it as OK
                            colLogger.MarkAsOk(property.GetColumnName());
                        }
                    }
                }
                else
                {
                    colLogger.NotInDatabase(property.GetColumnName(), CompareAttributes.ColumnName);
                }
            }
            if (!pKeyError)
                pKeyLogger.MarkAsOk(efPKeyConstraintName);
        }

        private bool IgnorePrimaryKeyFoundInOwnedTypes(IEntityType entityTypeDefiningEntityType, DatabaseTable table,
            IProperty property, IKey primaryKey)
        {
            if (entityTypeDefiningEntityType == null ||
                !string.Equals(entityTypeDefiningEntityType.GetTableName(), table.Name, _caseComparison))
                //if not a owned table, or the owned tabl has its own table then carry on
                return false;

            //Now we know that its an owned table, and it has a primary key which matches the table
            if (!primaryKey.Properties.Contains(property))
                return false;

            //It is a primary key so don't consider it as that is checked in the rest of the code
            return true;
        }

        private bool ComparePropertyToColumn(CompareLogger logger, IProperty property, DatabaseColumn column)
        {
            var error = logger.CheckDifferent(property.GetColumnType(), column.StoreType, CompareAttributes.ColumnType, _caseComparison);
            error |= logger.CheckDifferent(property.IsNullable.NullableAsString(), column.IsNullable.NullableAsString(), CompareAttributes.Nullability, _caseComparison);
            error |= logger.CheckDifferent(property.GetComputedColumnSql().RemoveUnnecessaryBrackets(),
                column.ComputedColumnSql.RemoveUnnecessaryBrackets(), CompareAttributes.ComputedColumnSql, _caseComparison);
            var defaultValue = property.GetDefaultValueSql() ?? property.GetDefaultValue()?.ToString();
            error |= logger.CheckDifferent(defaultValue.RemoveUnnecessaryBrackets(),
                    column.DefaultValueSql.RemoveUnnecessaryBrackets(), CompareAttributes.DefaultValueSql, _caseComparison);
            error |= CheckValueGenerated(logger, property, column);
            return error;
        }

        //thanks to https://stackoverflow.com/questions/1749966/c-sharp-how-to-determine-whether-a-type-is-a-number
        private static HashSet<Type> IntegerTypes = new HashSet<Type>
        {
            typeof(byte), typeof(sbyte), typeof(short), typeof(ushort), typeof(int), typeof(uint), typeof(long), typeof(ulong)
        };

        private bool CheckValueGenerated(CompareLogger logger, IProperty property, DatabaseColumn column)
        {
            var colValGen = column.ValueGenerated.ConvertNullableValueGenerated(column.ComputedColumnSql, column.DefaultValueSql);
            if (colValGen == ValueGenerated.Never.ToString()
                //There is a case where the property is part of the primary key and the key is not set in the database
                && property.ValueGenerated == ValueGenerated.OnAdd
                && property.IsKey()
                //We assume that a integer of some form should be provided by the database
                && !IntegerTypes.Contains(property.ClrType))
                return false;
            return logger.CheckDifferent(property.ValueGenerated.ToString(),
                colValGen, CompareAttributes.ValueGenerated, _caseComparison);
        }

#endif
    }
}