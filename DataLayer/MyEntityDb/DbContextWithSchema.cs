// Copyright (c) 2020 Jon P Smith, GitHub: JonPSmith, web: http://www.thereformedprogrammer.net/
// Licensed under MIT license. See License.txt in the project root for license information.

using DataLayer.MyEntityDb.ModelBuilders;
using Microsoft.EntityFrameworkCore;

namespace DataLayer.MyEntityDb
{
    public class DbContextWithSchema : DbContext
    {
        public DbContextWithSchema(
            DbContextOptions<DbContextWithSchema> options)      
            : base(options) { }

        public DbSet<MyEntity> MyEntities { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            SetSchema.Build(modelBuilder);
        }
    }
}