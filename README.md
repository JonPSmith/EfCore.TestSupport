# EfCore.TestSupport

This NuGet package containing methods to help you unit test applications that use [Entity Framework Core](https://docs.microsoft.com/en-us/ef/core/index) for database access. This readme provides links to the documentation in the [EfCore.TestSupport wiki](https://github.com/JonPSmith/EfCore.TestSupport/wiki). Also see [Release Notes](https://github.com/JonPSmith/EfCore.TestSupport/blob/master/ReleaseNotes.md) for information on changes.

**NOTE:** *The first number in the version number of this library defines what version of EF Core it works for. e.g. EfCore.TestSupport version 5 is works with to EF Core 5.*

**WARNING: If you are updating from the original EfCore.TestSupport (2.1 or 3), then you should read the [Version5UpgradeDocs.md](#) file as there are some breaking changes in this version.**

This project is open-source (MIT licence). 

Please don't add a pull request without first contacting me via an issue containing the reason/idea first. 

The EfCore.TestSupport library is available on [NuGet as EfCore.TestSupport](https://www.nuget.org/packages/EfCore.TestSupport/) and is an open-source library under the MIT licence. See [ReleaseNotes](https://github.com/JonPSmith/EfCore.TestSupport/blob/master/ReleaseNotes.md) for details of changes and information on versions before EF Core 5.


## Documentation

The NuGet package [EfCore.TestSupport](https://www.nuget.org/packages/EfCore.TestSupport/) containing methods to help you unit test applications that use [Entity Framework Core](https://docs.microsoft.com/en-us/ef/core/index) for database access. This readme defines the various groups, with links to the documentation in the [EfCore.TestSupport wiki](https://github.com/JonPSmith/EfCore.TestSupport/wiki).

*NOTE: The techniques are explained in much more details in chapter 17 of the book [Entity Framework in Action, second edition](https://bit.ly/EfCoreBookEd2).*

Here is an image covering just a few of the methods available in this library.  
 
![Examples of libray methods in use](https://github.com/JonPSmith/EfCore.TestSupport/blob/master/ExampleOfTestSupport.png)

The various groups of tools are:

1. Helpers to create an in-memory Sqlite database for unit testing.  
See [Sqlite in memory test database](https://github.com/JonPSmith/EfCore.TestSupport/wiki/1.-Sqlite-in-memory-test-database).
2. Helpers to create connection strings with a unique database name.  
See [Creating connection strings](https://github.com/JonPSmith/EfCore.TestSupport/wiki/3.-Creating-connection-strings).
3. Helpers for creating unique SQL Server databases for unit testing.  
See [Create SQL Server databases](https://github.com/JonPSmith/EfCore.TestSupport/wiki/4.-Create-SQL-Server-databases).
4. Helpers to create Cosmos DB databases linked to Azure Cosmos DB Emulator.  
See [Create Cosmos DB options](https://github.com/JonPSmith/EfCore.TestSupport/wiki/Create-Cosmos-DB-options).
6. Helper for wiping all data and resetting the schema a SQL Server database.  
See [Quickly wipe and reset schema on SQL Server](#).
7. Various tools for getting test data, or file paths to test data.   
See [Test Data tools](https://github.com/JonPSmith/EfCore.TestSupport/wiki/6.-Test-Data-tools).
8. A tool for applying a SQL script file to a EF Core database.  
See [Run SQL Script](https://github.com/JonPSmith/EfCore.TestSupport/wiki/7.-Run-SQL-Script).
9. Tools for capturing EF Core logging.  
See [Capture EF Core logging](https://github.com/JonPSmith/EfCore.TestSupport/wiki/8.-Capture-EF-Core-logging).




