# Release notes




## TODO


## 1.9.0

- Feature: Add EfCoreResetter to reset the Primary and Foreign keys for save/restore of data in a database.
- Feature: Add EfCoreResetter to anonymise names, addesses etc.
- Bug fix: fix issue #15, EfSchemaCompare: Error with default values on string properties.

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







