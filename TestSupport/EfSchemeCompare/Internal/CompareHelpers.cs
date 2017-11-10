// Copyright (c) 2017 Jon P Smith, GitHub: JonPSmith, web: http://www.thereformedprogrammer.net/
// Licensed under MIT licence. See License.txt in the project root for license information.

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
        public static string FormSchemaTable(this IRelationalEntityTypeAnnotations eRel)
        {
            return FormSchemaTable(eRel.Schema, eRel.TableName);
        }

        public static string FormSchemaTable(this DatabaseTable table)
        {
            return FormSchemaTable(table.Schema, table.Name);
        }

        public static string NullableAsString(this bool isNullable)
        {
            return isNullable ? "NULL" : "NOT NULL";
        }

        public static string CombinedColNames(this IEnumerable<IProperty> properties)
        {
            return string.Join(",", properties.Select(x => x.Relational().ColumnName));
        }

        public static string ConvertNullableValueGenerated(this ValueGenerated? valGen)
        {
            return valGen?.ToString() ?? ValueGenerated.Never.ToString();
        }

        public static string ConvertNullableReferentialAction(this ReferentialAction? refAct)
        {
            return refAct?.ToString() ?? ReferentialAction.NoAction.ToString();
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