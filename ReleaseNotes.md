# Release notes

## 6.0.0-priview001

- Updated to Net6-rc.2 preview
- Added PostgreSQL database helpers
- Added the Seed Database feature back in at request of users

## 5.0.0

- A serious cleanup to the this library with BREAKING CHANGES
- BREAKING CHANGES
   - Altered SqliteInMemory methods to return a IDisposable options (disposes connection at the end of the test). THIS WILL BREAK YOUR CODE.
  - Cosmos DB methods renames and extended
- REMOVED FEATURES
   - Removed EfSchemaCompare - its in its own library, [EfCore.SchemaCompare](https://github.com/JonPSmith/EfCore.SchemaCompare).
   - Removed SeedDatabase - use old 3.2.0 EfCore.TestSupport version
   - Removed InMemory Database helper - use EF Core's In Memory database
- New features
   - Add `EnsureClean`. This clears the schema and data and set up new model (only for SQL Server)
   - SqliteInMemory now has `CreateOptionsWithLogTo` extension method
   - SqlServer now has `CreateUniqueClassOptionsWithLogTo` and `CreateUniqueMethodOptionsWithLogTo` extension methods
- Marked as obsolete
   - `CreateOptionsWithLogging<T>` - use ...ToLog version
   - `CreateUniqueClassOptionsWithLogging<T>` - use ...ToLog version
   - `CreateUniqueMethodOptionsWithLogging<T>` - use ...ToLog version
   - `CreateEmptyViaWipe` - recommend `EnsureClean` or respawn

*NOTE:It is no longer possible to detect the EF Core version via the netstandard so now it is done via the first number in the library's version. For instance EfCore.TestSupport, version 5.?.? works with EF Core 5.?.?.*

---


# Older versions of EfCore.TestSupport

These support EF Core 2.1, 3.0, and 3.1.

## 3.2.0

- New Feature: (only EF Core 3): The extension methods fro creating options for In-memory sqlite options and SQL Server options now have an optional parameter that allows you to add extra options to the DbContextOptionsBuilder<T>.
- New Feature: TimeThings has version that returns the result of the timing.
- Issue 32: Missing primary key in database causes a exception.

## 3.1.1

- Issue #14, part 2. Fix code in EfSchemaCompare that checks a connection string that came from the DbContext in case its a named connection string (which is silly).
- Issue #30. Fixed CompareEfSql on Linux. Changed `CompareHelpers.ComparerToComparison` Dictionary to if statements.

## 3.1.0

- Feature: GetCosmosDbToEmulatorOption sets up Cosmos DB options linked to to the Azure Cosmos DB Emulator.

## 3.0.0

- Support both EF Core >=2.1 and EF Core >=3.0 by supporting NetStandard2.0 and NetStandard2.1. 
- Bug fix: GetAllErrors() in CompareEfSql should use Environment.NewLine. See issue #20.
- Obsolete: Remove LogSetupHelper as obsolete, use `CreateOptionsWithLogging` for Sqlite and SQL Server. 

## 2.0.1

- Bug fix: Seed from Production's WriteJsonToJsonFile and ReadSeedDataFromJsonFile didn't get the calling assembly - see issue #21 (thanks to @Selmirrrrr for spotting this)


## 2.0.0

- BREAKING CHANGE: By default EfSchemaCompare to only scan the tables that the entity classes map to - see issue #18.
- Improvement: EfSchemaCompare now has case insensitive table, schema, columns, etc. matching feature - see issues #9 and #19.
- BREAKING CHANGE: In EfSchemaCompare missing indexes are now referred to by "index constraint name" instead of "constraint name" - this was done as part of case insensitivity 
- Bug fix: EfSchemaCompare default SQL value handled improved, plus now sets the correct ValueGenerated - see issue #15
- Bug fix: Fixed problem with serializing/deserializing DDD-styled entity classes.

## 1.9.0

- Added seed test data from production database - see [Seed from Production feature](https://github.com/JonPSmith/EfCore.TestSupport/wiki/Seed-from-Production-feature) in Wiki docs.

## 1.8.0
- Improvement: CompareEfSql now recoginises unique indexes provided by constraints (rather than via the normal SQL Index statement)
- Improvement: CompareEfSqlConfig.TablesToIgnoreCommaDelimited should trim tables names are ignore null entries
- Feature: Added WipeCreateDatabase extension method - useful for creating empty database prior to using SQL to add tables etc.

## 1.7.0
- Feature: Added DecodeMessage feature to LogOutput. Tries to recreate the actual SQL to allow cut/paste 
- Bug fix: Minor format error in ToString of LogOutput - adds an unwanted comma after log type if used for non-EF Core logs.

## 1.6.1 
- Bug fix: Added LogLevel to ...WithLogging versions of Sqlite/SQL server option builders.
- 
## 1.6.0
- Feature. Added new way to capture EF Core logging output, which is superior to the existing SetupLogging method. See EfCore.TestSupport Wiki page "Capture EF Core logging".
- Bug fix: SetupLogging is now marked as obsolete, but not removed so that existing unit tests don't break. 

## 1.5.2
- Bug fix: CompareEfSql no longer fails when there there isn't a primary key in a DbQuery - issue #8
- Feature: CompareEfSql outputs a single warning if any DbQuery types are found, as it cannot check DbQuery types - issue #11

## 1.5.1
- Bug fix: Fixed bug in CompareEfSql where two tables have the same name, but different schemas, caused an exception

## 1.5.0
- New Feature: Added GetConfiguration which takes a relative directory string. This allows you to access configurations in other projects or subdirectories.

## 1.4.0
- Package: Updated to .NET Core 2.1 and EF Core 2.1

## 1.3.1
- Package: EntityFrameworkCore libraies updated to version 2.0.2, and specific versions defined to match with SqlClient
- Bug fix: DeleteAllUnitTestDatabases directly referenced LocalDB, which meant it couldn't be used with other databases. This is fixed.
- Feature: TimeThings now allows you to input the number of runs being timed and gives you average time per run

## 1.3.0
- Bug fix: Fixed problem with using TestData methods on non-windows systems (Path.DirectorySeparatorChar) - Thanks to Henrik Blick for pointing this out
- Bug fix: TestData didn't find the correct directory
- Feature: Allows any 2.* version of EF Core
- Feature: Only loads the required Nuget xUnit packages

## 1.2.0

- BREAKING CHANGE: The SqlServerHelpers, SqliteInMemory, and EfInMemory option setting extensions now default to throwing an exception if a Microsoft.EntityFrameworkCore.Query.QueryClientEvaluationWarning is logged

## 1.1.5
- Bug Fix: Gives useful error message if TestData.GetCallingAssemblyTopLevelDir does not find /bin/ file

## 1.1.4
- Bug Fix: The TestData.GetCallingAssemblyTopLevelDir was not given the caling assembly in some cases
- Change: Made the ValueGenerated test assume that the database will create an integer key

## 1.1.3
- Bug Fix: Fixed issue #2 - primary key may not be set in database
- Bug Fix: Fixed issue #3 - wrong error on sql default value
- Change: The appsetting.json file is now not required - you get an error about a connection string not being in the appsetting.json file if you use any of the commands that need a appsetting.json file.

## 1.1.2
- Added feature: Now the EfSchemaCompare feature will work with any database provider
- Change: I made the CompareLog 'ignore errors' feature more open - now you can match any type (see wiki)
- Bug fix: With the help of the EF Core team I fixed the table splitting error I had (see EF Core issue #10345)

## 1.1.1
- Fixed bug in EfSchemaCompare - wasn't handling table spitting properly in stage2

## 1.1.0
- New feature: EfSchemaCompare to compare EF Core's view of the database with an actual database.

## 1.0.2
- Improved WipeDbViaSql to work with databases that don't use brackets around table names
- Changed required suffix to test database name to "Test" to allow for databases that don't support hyphen in names

## 1.0.1
- Feature - Improved names on TestData methods

## 1.0.0
- First Release







