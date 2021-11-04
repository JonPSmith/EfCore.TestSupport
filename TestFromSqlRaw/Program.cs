using Microsoft.EntityFrameworkCore;
using TestSupport.EfHelpers;

namespace TestFromSqlRaw
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var options = SqliteInMemory.CreateOptions<MyDbContext>();
            var context = new MyDbContext(options);

            //This shows that making the Microsoft.EntityFrameworkCore.Cosmos NuGet package private
            //to the TestSupport project removes the compile-time error "The call is ambiguous..." - SEE BELOW
            //
            //Code	CS0121: The call is ambiguous between the following methods or properties:
            //'Microsoft.EntityFrameworkCore.RelationalQueryableExtensions.FromSqlRaw<TEntity>(Microsoft.EntityFrameworkCore.DbSet<TEntity>, string, params object[])' and
            //'Microsoft.EntityFrameworkCore.CosmosQueryableExtensions.FromSqlRaw<TEntity>(Microsoft.EntityFrameworkCore.DbSet<TEntity>, string, params object[])'

            context.MyEntities.FromSqlRaw("Select * FROM MyEntities");
        }
    }
}


