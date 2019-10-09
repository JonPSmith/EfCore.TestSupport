// Copyright (c) 2017 Jon P Smith, GitHub: JonPSmith, web: http://www.thereformedprogrammer.net/
// Licensed under MIT licence. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Scaffolding.Metadata;

namespace TestSupport.EfSchemeCompare.Internal
{
    internal static class CompareHelpers
    {
#if NETSTANDARD2_0
        public static string FormSchemaTable(this IRelationalEntityTypeAnnotations eRel)
        {
            return FormSchemaTable(eRel.Schema, eRel.TableName);
        }
#elif NETSTANDARD2_1
        public static string FormSchemaTable(this IEntityType entityType)
        {
            return FormSchemaTable(entityType.GetSchema(), entityType.GetTableName());
        }
#endif

        public static string FormSchemaTable(this DatabaseTable table, string defaultSchema)
        {
            //The DatabaseTable always provides a schema name, while the database Model provides null if default schema name.
            //This makes sure that name will match the EF Core Model format
            var schemaToUse = table.Schema == defaultSchema ? null : table.Schema;
            return FormSchemaTable(schemaToUse, table.Name);
        }

        public static string NullableAsString(this bool isNullable)
        {
            return isNullable ? "NULL" : "NOT NULL";
        }

        public static StringComparison GetStringComparison(this StringComparer caseComparer)
        {
            return ComparerToComparison[caseComparer];
        }

        private static readonly Dictionary<StringComparer, StringComparison> ComparerToComparison =
            new Dictionary<StringComparer, StringComparison>
            {
                {StringComparer.CurrentCulture, StringComparison.CurrentCulture},
                {StringComparer.CurrentCultureIgnoreCase, StringComparison.CurrentCultureIgnoreCase},
                {StringComparer.InvariantCulture, StringComparison.InvariantCulture},
                {StringComparer.InvariantCultureIgnoreCase, StringComparison.InvariantCultureIgnoreCase},
                {StringComparer.Ordinal, StringComparison.Ordinal},
                {StringComparer.OrdinalIgnoreCase, StringComparison.OrdinalIgnoreCase}
            };

#if NETSTANDARD2_0
        public static string CombinedColNames(this IEnumerable<IProperty> properties)
        {
            return string.Join(",", properties.Select(x => x.Relational().ColumnName));
        }
#elif NETSTANDARD2_1
        public static string CombinedColNames(this IEnumerable<IProperty> properties)
        {
            return string.Join(",", properties.Select(x => x.GetColumnName()));
        }
#endif

        //The scaffold does not set the correct ValueGenerated for a column that has a sql default value of a computed column
        //see https://github.com/aspnet/EntityFrameworkCore/issues/9323
        public static string ConvertNullableValueGenerated(this ValueGenerated? valGen, string computedColumnSql, string defaultValueSql)
        {
            if (valGen == null && defaultValueSql != null)
                return ValueGenerated.OnAdd.ToString();
            if (valGen == null && computedColumnSql != null)
                return ValueGenerated.OnAddOrUpdate.ToString();
            return valGen?.ToString() ?? ValueGenerated.Never.ToString();
        }

        public static string ConvertReferentialActionToDeleteBehavior(this ReferentialAction? refAct, DeleteBehavior entityBehavior)
        {
            if ((entityBehavior == DeleteBehavior.ClientSetNull || entityBehavior == DeleteBehavior.Restrict)
                && refAct == ReferentialAction.NoAction)
                //A no action constrait can map to either ClientSetNull or Restrict
                return entityBehavior.ToString();

            return refAct?.ToString() ?? ReferentialAction.NoAction.ToString();
        }

        public static string RemoveUnnecessaryBrackets(this string val)
        {
            if (val == null) return null;

            while (val.Length > 1 && val[0] == '(' && val[val.Length-1] == ')')
            {
                val = val.Substring(1, val.Length - 2);
            }

            return val;
        }

        //--------------------------------------------------------------
        //private 

        private static string FormSchemaTable(string schema, string table)
        {
            return string.IsNullOrEmpty(schema)
                ? table
                : $"{schema}.{table}";
        }
    }
}