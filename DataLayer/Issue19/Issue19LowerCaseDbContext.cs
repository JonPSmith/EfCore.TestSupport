// Copyright (c) 2017 Jon P Smith, GitHub: JonPSmith, web: http://www.thereformedprogrammer.net/
// Licensed under MIT licence. See License.txt in the project root for license information.

using DataLayer.BookApp;
using DataLayer.Issue3;
using Microsoft.EntityFrameworkCore;

namespace DataLayer.Issue19
{
    public class Issue19LowerCaseDbContext : DbContext
    {
        public DbSet<PrincipalClass> PrincipalClasses { get; set; }
        public DbSet<DependentClass> DependentClasses { get; set; }

        public Issue19LowerCaseDbContext(DbContextOptions<Issue19LowerCaseDbContext> options) : base(options) { }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.SetupModel(true);

        }
    }
}