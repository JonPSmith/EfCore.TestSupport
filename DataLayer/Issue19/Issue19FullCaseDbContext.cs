// Copyright (c) 2017 Jon P Smith, GitHub: JonPSmith, web: http://www.thereformedprogrammer.net/
// Licensed under MIT licence. See License.txt in the project root for license information.


using Microsoft.EntityFrameworkCore;

namespace DataLayer.Issue19
{
    public class Issue19FullCaseDbContext : DbContext
    {
        public DbSet<PrincipalClass> PrincipalClasses { get; set; }
        public DbSet<DependentClass> DependentClasses { get; set; }

        public Issue19FullCaseDbContext(DbContextOptions<Issue19FullCaseDbContext> options) : base(options) { }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.SetupModel(false);
        }
    }
}