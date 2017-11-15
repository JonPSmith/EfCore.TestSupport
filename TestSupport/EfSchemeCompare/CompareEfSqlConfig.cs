// Copyright (c) 2017 Jon P Smith, GitHub: JonPSmith, web: http://www.thereformedprogrammer.net/
// Licensed under MIT licence. See License.txt in the project root for license information.

namespace TestSupport.EfSchemeCompare
{
    /// <summary>
    /// This class holds the configuration information for the CompareEfSql class
    /// </summary>
    public class CompareEfSqlConfig
    {
        /// <summary>
        /// Place a table name (with schema name if required) of the tables you want the comparer to ignore.
        /// This allows you to ignore tables that your EF Core context doesn't use. 
        /// This will stop "Extra In Database" errors for these tables
        /// Typical format: "MyTable,MyOtherTable,dbo.MyTableWithSchema" (note: table match is case insensitive)
        /// </summary>
        public string TablesToIgnoreCommaDelimited { get; set; }

    }
}