// Copyright (c) 2017 Jon P Smith, GitHub: JonPSmith, web: http://www.thereformedprogrammer.net/
// Licensed under MIT licence. See License.txt in the project root for license information.

using Microsoft.EntityFrameworkCore;

namespace DataLayer.Issue2
{
    public class Issue2DbContext : DbContext
    {
        public DbSet<NormativeReference> NormativeReferences { get; set; }
        public DbSet<PrimaryKeyGuid> PrimaryKeyGuids { get; set; }
        public DbSet<PrincipalEntity> PrincipalEntities { get; set; }

        public Issue2DbContext(
            DbContextOptions<Issue2DbContext> options)      
            : base(options) { }
    }
}