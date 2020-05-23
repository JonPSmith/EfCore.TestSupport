// Copyright (c) 2020 Jon P Smith, GitHub: JonPSmith, web: http://www.thereformedprogrammer.net/
// Licensed under MIT license. See License.txt in the project root for license information.

using Microsoft.EntityFrameworkCore;

namespace DataLayer.Issue32
{
    public class Issue32Context : DbContext
    {
        public DbSet<MyClass> MyClasses { get; set; }

        public Issue32Context(DbContextOptions<Issue32Context> options)
            : base(options) { }

        protected override void
            OnModelCreating(ModelBuilder modelBuilder)
        {
        }
    }
}