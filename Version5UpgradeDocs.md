# Upgrade documentation notes

This document provides information when converting to Version 5 of the `EfCore.TestSupport` library.

## Summary of the changes

1. BREAKING CHANGE: The `SqliteInMemory.CreateOptions` etc. has changed and some of your unit tests might break. See section below.
2. Nice new `EnsureClean` feature added for SQL Server. See docs on that.
2. The EfSchemaCompare feature has been removed. If you need this then keep using the V3 version of EfCore.TestSupport (I do plan to create a library for EfSchemaCompare, but I haven't done that yet)
3. Added SQLite/SQL Server options with logging using the new `LogTo` logging output and marked the `...WithLogging` versions as obsolete.
4. Cosmos DB methods renames and extended.
4. Removed InMemory Database helper as this provider isn't a good way to unit test. If you need it then use EF Core's In Memory database provider.
5. Removed SeedDatabase - use old 3.2.0 EfCore.TestSupport version (very few people used this)


## Breaking change in SqliteInMemory options

**If you use any of the `SqliteInMemory.CreateOptions...` from previous versions of EfCore.TestSupport, then its VERY LIKELY THAT IT WILL BREAK YOUR UNIT TESTS!!**

The new `SqliteInMemory.CreateOptions<MyDbContext>()` method now returns a class that implements `DbContextOptions<MyDbContext>` needed by the database, but now also implements `IDisposable`. This is done to dispose Sqlite's connection (which contains the in-memory data) when the context is disposed. (I didn't do that in the the previous versions, and I should have.)

This means when the application DbContext using that option is disposed the `Dispose` method in the options class is also called and the connection is disposed.

This means if you have a unit tests like the one below it will fail, because the second instance of the dbContext the connection will be empty.

```c#
public void TestSqliteTwoInstancesBAD()
{
    //SETUP
    var options = SqliteInMemory.CreateOptions<BookContext>();
    using (var context = new BookContext(options))
    {
        context.Database.EnsureCreated();
        context.SeedDatabaseFourBooks(); 
    }
    using (var context = new BookContext(options))
    {
        //ATTEMPT
        //THIS WILL FAIL!!!! THIS WILL FAIL!!!! THIS WILL FAIL!!!!
        var books = context.Books.ToList();

        //VERIFY
        books.Last().Reviews.ShouldBeNull();
    }
}
```

You have three options (best first):

1. **Quick and Easy**: Turn off Dispose using `options.TurnOffDispose()`
2. **Best approach**: Have one instance of the application DbContext and use `ChangeTracker.Clear()`.
3. **Keep your Using(var...**: Have two instances of the application DbContext and call `options.StopNextDispose()`
4. **Lots of DbContext instances**: Turn off Dispose and manually dispose at the end

### 1. **Quick and Easy**: Turn off Dispose using `options.TurnOffDispose()`

The quick version is to called `options.TurnOffDispose()`. This makes it work in the same way the previous versions of EfCore.TestSupport worked.

```c#
public void TestSqliteThreeInstancesOk()
{
    //SETUP
    var options = SqliteInMemory.CreateOptions<BookContext>();
    options.TurnOffDispose();
    
    //... rest of your code as it was
}
```

### 2. **Best approach**: One instance and use `ChangeTracker.Clear()`

In EF Core 5 there is new feature triggered by `context.ChangeTracker.Clear()`. This clears out all the tracked entities in the current instance of the application DbContext. This means you don't need two application DbContexts to check that your second part worked property. 

```c#
public void TestSqliteOneInstanceWithChangeTrackerClearOk()
{
    //SETUP
    var options = SqliteInMemory.CreateOptions<BookContext>();
    using var context = new BookContext(options);
    context.Database.EnsureCreated();
    context.SeedDatabaseFourBooks();

    context.ChangeTracker.Clear(); //NEW LINE ADDED

    //ATTEMPT
    var books = context.Books.ToList();

    //VERIFY
    books.Last().Reviews.ShouldBeNull();
}
```

*NOTE: It also allows you to use `using var context = ...` - see line 5. This makes the code simpler, and quicker to write. That's why I recommend this version*

### 3. **Keep your Using(var...**: Use `options.StopNextDispose()`

If you want to keep the two instances of the application DbContext, then you need to use the `options.StopNextDispose()` to stop the dispose on the first application DbContext instance. You can call the `StopNextDispose` method any time before the first application DbContext instance is disposed, but I tend to do it right under the creating of the option, as shown in line 5 of the code below

```c#
public void TestSqliteTwoInstancesGood()
{
    //SETUP
    var options = SqliteInMemory.CreateOptions<BookContext>();
    options.TurnOffDispose();
    using (var context = new BookContext(options))
    {
        context.Database.EnsureCreated();
        context.SeedDatabaseFourBooks(); 
    }
    using (var context = new BookContext(options))
    {
        //ATTEMPT
        var books = context.Books.ToList();

        //VERIFY
        books.Last().Reviews.ShouldBeNull();
    }
}
```

### 4. **Lots of DbContext instances**: Turn off Dispose and manually dispose at the end

If you have multiple instances of the application DbContext, then you can use the `options.TurnOffDispose()` and call the `options.ManualDispose()` methods at the end of the unit test.

```c#
public void TestSqliteThreeInstancesOk()
{
    //SETUP
    var options = SqliteInMemory.CreateOptions<BookContext>();
    options.TurnOffDispose();
    using (var context = new BookContext(options))
    {
        context.Database.EnsureCreated();
        context.SeedDatabaseFourBooks(); 
    }
    using (var context = new BookContext(options))
    {
        //ATTEMPT
        var books = context.Books.ToList();

        //VERIFY
        books.Last().Reviews.ShouldBeNull();
    } 
    using (var context = new BookContext(options))
    {
        //ATTEMPT
        var books = context.Books.ToList(); 

        //VERIFY
        books.Last().Reviews.ShouldBeNull();
    }
    options.ManualDispose();
}
```

*NOTE the call to `options.TurnOnDispose();` before the last application DbContext.*



## Removed features

- You can find the EfSchemaCompare feature in the GitHub repo [EfCore.SchemaCompare](https://github.com/JonPSmith/EfCore.SchemaCompare) and its associated [NuGet package](https://www.nuget.org/packages/EfCore.SchemaCompare/).
- The InMemory Database provider is not recommended by the EF Core team - see [this link](https://docs.microsoft.com/en-us/ef/core/testing/#approach-3-the-ef-core-in-memory-database) .
- Removed SeedDatabase - very few people used this, but you can get the code in the [Version3-2-0](https://github.com/JonPSmith/EfCore.TestSupport/tree/Version3-2-0) branch.