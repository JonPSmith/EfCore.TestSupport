// Copyright (c) 2020 Jon P Smith, GitHub: JonPSmith, web: http://www.thereformedprogrammer.net/
// Licensed under MIT license. See License.txt in the project root for license information.

using Microsoft.EntityFrameworkCore;

namespace DataLayer.Database2
{
    public class DbContext2 : DbContext
    {
        public DbSet<TopClass2> TopClasses { get; set; }
        public DbSet<Dependent2> Dependents { get; set; }

        public DbContext2(DbContextOptions<DbContext2> options)
            : base(options) { }
    }
}