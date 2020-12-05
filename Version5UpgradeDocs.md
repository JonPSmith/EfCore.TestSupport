# Upgrade documentation notes

This document provides information when converting to Version 5 of the EfCore.TestSupport library.

## Breaking change in SqliteInMemory options

The new `SqliteInMemory.CreateOptions<MyDbContext>()` method now returns a class that implements `DbContextOptions<MyDbContext>` neede by the database, but now also implements `IDisposable`. This is done to dispose Sqlite's connection (which contains the in-memeory data) when the context is disposed. (I didn't do that in the the previous versions, and I should have.)

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
        var books = context.Books.ToList(); //WILL FAIL!!!!!

        //VERIFY
        books.Last().Reviews.ShouldBeNull();
    }
}
```

You have three options (best first):
1. Have one instance of the application DbContext and use `ChangeTracker.Clear()`.
2. Have two instances of the application DbContext and call `options.StopNextDispose()`
3. Turn off Dispose of the connection using `options.TurnOffDispose()` (and turn on again at the end)

### 1. One instance and use `ChangeTracker.Clear()`

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

### 2. Use `options.StopNextDispose()`

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

### 3. Turn off Dispose of the connection using `options.TurnOffDispose()`

If you have mutiple instances of the application DbContext, then you can use the `options.TurnOffDispose()` and call the `options.ManualDispose()` methods at the end of the unit test.

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