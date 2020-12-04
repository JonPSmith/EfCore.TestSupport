// Copyright (c) 2020 Jon P Smith, GitHub: JonPSmith, web: http://www.thereformedprogrammer.net/
// Licensed under MIT license. See License.txt in the project root for license information.

using Microsoft.EntityFrameworkCore;

namespace DataLayer.Database1
{
    public class DbContext1 : DbContext
    {
        public DbSet<TopClass1> TopClasses { get; set; }
        public DbSet<Dependent1> Dependents { get; set; }

        public DbContext1(DbContextOptions<DbContext1> options)
            : base(options) { }

    }
}