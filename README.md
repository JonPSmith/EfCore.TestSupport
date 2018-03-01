# EfCore.TestSupport

This git repo contains the source of the
[EfCore.TestSupport](https://www.nuget.org/packages/EfCore.TestSupport/), 
and various tests to check the EfCore.TestSupport 
[NuGet package](https://www.nuget.org/packages/EfCore.TestSupport/).

This project is open-source (MIT licence).

See [Release Notes](https://github.com/JonPSmith/EfCore.TestSupport/blob/master/ReelaseNotes.md)
for information on changes.

# Documentation

The NuGet package [EfCore.TestSupport](https://www.nuget.org/packages/EfCore.TestSupport/)
is a netstandard2.0 library containing methods to help you unit test applications that use
[Entity Framework Core](https://docs.microsoft.com/en-us/ef/core/index)
for database access. This readme defines the various groups, with links to the documentation in the 
[EfCore.TestSupport wiki](https://github.com/JonPSmith/EfCore.TestSupport/wiki).

*NOTE: The techniques are explained in much more details in chapter 15 of the book
[Entity Framework in Action](http://bit.ly/2m8KRAZ).*

Here is an image covering just a few of the methods, in **bold** available in this library.  
 
![Examples of libray methods in use](https://github.com/JonPSmith/EfCore.TestSupport/blob/master/Example-of-various-test-features.png)

The various groups of tools are:

1. Helpers to create an in-memory Sqlite database for unit testing.  
See [Sqlite in memory test database](https://github.com/JonPSmith/EfCore.TestSupport/wiki/1.-Sqlite-in-memory-test-database).
2. Helpers to create an InMemory database for unit testing.  
See [EF InMemory test database](https://github.com/JonPSmith/EfCore.TestSupport/wiki/2.-EF-InMemory-test-database).
3. Helpers to create connection strings with a unique database name.  
See [Creating connection strings](https://github.com/JonPSmith/EfCore.TestSupport/wiki/3.-Creating-connection-strings).
4. Helpers for creating unique SQL Server databases for unit testing.  
See [Create SQL Server databases](https://github.com/JonPSmith/EfCore.TestSupport/wiki/4.-Create-SQL-Server-databases).
5. Helpers for creating an empty database, and deleting SQL unit test databases.  
See [Quickly create empty databases](https://github.com/JonPSmith/EfCore.TestSupport/wiki/5.-Quickly-create-empty-database).
6. Various tools for getting test data, or file paths to test data.  
See [Test Data tools](https://github.com/JonPSmith/EfCore.TestSupport/wiki/6.-Test-Data-tools).
7. A tool for applying a SQL script file to a EF Core database.  
See [Run SQL Script](https://github.com/JonPSmith/EfCore.TestSupport/wiki/7.-Run-SQL-Script).
8. Tools for capturing EF Core logging.  
See [Capture EF Core logging](https://github.com/JonPSmith/EfCore.TestSupport/wiki/8.-Capture-EF-Core-logging).
9. Tool to compare EF Core's view of the database with an actual database.  
See [EfSchemaCompare](https://github.com/JonPSmith/EfCore.TestSupport/wiki/9.-EfSchemaCompare).  



