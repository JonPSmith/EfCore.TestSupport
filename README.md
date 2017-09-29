# EfCore.TestSupport

This git repo contains the source of the NuGet package 
[EfCore.TestSupport](https://www.nuget.org/packages/EfCore.TestSupport/), 
and various tests to check that NuGet package.

# Documentation

This netstandard2.0 library contains tools to help anyone that is unit testing applications that use
[Entity Framework Core](https://docs.microsoft.com/en-us/ef/core/index)
for database access. The techniques are explained in chapter 15 of the book
[Entity Framework in Action](http://bit.ly/2m8KRAZ).
This readme defines the various groups and the signatures of the methods.

The various groups of tools are

1. Helpers to create an in-memory Sqlite database for unit testing
2. Helpers to create an InMemory database for unit testing
3. Helpers to create connection strings with a unique database name
4. Helpers for creating unique SQL Server databases for unit testing
5. Helpers for creating an empty database, and deleting SQL unit test databases
6. Various tools for getting test data, or file paths to test data
7. A tool for applying a SQL script file to a EF Core database
8.  Tools for capturing EF Core logging 


## 1. Helpers to create an in-memory Sqlite database

The `sqliteInMemory.CreateOptions<T>` method will create options that will
provide a sqlite, in-memory database for unit testing. The code below shows 
how is can be used.

```c#
[Fact]
public void TestSqliteOk()
{
    //SETUP
    var options = SqliteInMemory.CreateOptions<EfCoreContext>(); 
    using (var context = new EfCoreContext(options))
    {
        //... rest of unit test goes here
```

It has one, optional bool parameter of `throwOnClientServerWarning` which defaults to false.
If set to true it will configure EF Core to throw an expection if a `QueryClientEvaluationWarning` is logged.

## 2. Helpers to create an InMemory database

The `EfInMemory.CreateOptions<T>` method will create options that will
provide an EF Core InMemory database for unit testing. The code below shows 
how is can be used.

```c#
[Fact]
public void TestSqliteOk()
{
    //SETUP
    var options = EfInMemory.CreateOptions<EfCoreContext>(); 
    using (var context = new EfCoreContext(options))
    {
        //... rest of unit test goes here
```

It has one, optional bool parameter of `throwOnClientServerWarning` which defaults to false.
If set to true it will configure EF Core to throw an expection if a `QueryClientEvaluationWarning` is logged.

## 3. Helpers to create connection strings with a unique database name

The [xUnit](https://xunit.github.io/) unit test library will run unit test classes in parallel.
This means you need class-unique databases to allow the unit tests not to clash. 
I have a number of methods to help with this, but first you must add a `appsettings.json`

### Specifying the base database connection string

If you are going to use this library to help create SQL Server databases, 
then you need to place an `appsettings.json` file in the top-level directory 
of you test project. The file should contain:  
1. A connection string with the name `UnitTestConnection`
2. The name of the database in that connection string must end with `-Test`. That is a safety feature (see later)

[Click here](https://github.com/JonPSmith/EfCore.TestSupport/blob/master/Test/appsettings.json) 
for an example of the `appsettings.json` file.

### The `AppSettings.GetConfiguration()` method

The method `AppSettings.GetConfiguration()` will get the configuration file using the ASP.NET Core code.
You can place any setting for your unit tests

### The `GetUniqueDatabaseConnectionString()` extention method

The method `GetUniqueDatabaseConnectionString()` is an extention method on an object.
It uses that object's name to form a connection string based on the `UnitTestConnection` in 
you `appsettings.json` file, but where the database name from the `UnitTestConnection` 
connection string has the name of the object added as a suffix. See the test code below.

```c#
[Fact]
public void GetTestConnectionStringOk()
{
    //SETUP
    var config = AppSettings.GetConfiguration();
    var orgDbName = new SqlConnectionStringBuilder(config.GetConnectionString(AppSettings.UnitTestConnectionStringName)).InitialCatalog;

    //ATTEMPT
    var con = this.GetUniqueDatabaseConnectionString();

    //VERIFY
    var newDatabaseName = new SqlConnectionStringBuilder(con).InitialCatalog;
    newDatabaseName.ShouldEqual ($"{orgDbName}.{this.GetType().Name}");
}
```

The  `GetUniqueDatabaseConnectionString()` extention method takes one, optional 
parameter, which it will add onto the database name. This allows you to make 
method-level unique database names

## 4. Helpers for creating unique SQL Server databases

The library has two methods that will create options to 
provide an SQL Server database for unit testing. 
One provides a class-level unique database name, and one provides a method-unique database name.

Both take the same optional parameter as the Sqlite.CreateOptions<T> which defaults to false.
If set to true it will configure EF Core to throw an expection if a `QueryClientEvaluationWarning` is logged.

### The `CreateUniqueClassOptions()` extension method

This returns a SQL Server options with the connection string from the `appsettings.json` file
but the name of the database now has the type name of the object (which should be `this`) 
as a suffix. See test code below

```c#
[Fact]
public void TestSqlServerUniqueClassOk()
{
    //SETUP
    //ATTEMPT
    var options = this.CreateUniqueClassOptions<EfCoreContext>();
    using (var context = new EfCoreContext(options))
    {
        //VERIFY
        var builder = new SqlConnectionStringBuilder(context.Database.GetDbConnection().ConnectionString);
        builder.InitialCatalog.ShouldEndWith(this.GetType().Name);
    }
}
```

... sorry, more needs to be added, but run out of time at the moment :(
