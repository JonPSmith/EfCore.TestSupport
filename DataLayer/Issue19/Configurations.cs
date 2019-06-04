// Copyright (c) 2019 Jon P Smith, GitHub: JonPSmith, web: http://www.thereformedprogrammer.net/
// Licensed under MIT license. See License.txt in the project root for license information.

using Microsoft.EntityFrameworkCore;

namespace DataLayer.Issue19
{
    public static class Configurations
    {
        public static void SetupModel(this ModelBuilder modelBuilder, bool lowerCase)
        {
            string GetName(string name)
            {
                return lowerCase ? name.ToLowerInvariant() : name;
            }

            modelBuilder.Entity<PrincipalClass>().ToTable(GetName("Principals"), GetName("SchemaOne"));

            modelBuilder.Entity<PrincipalClass>().Property(x => x.PrincipalClassId)
                .HasColumnName(GetName(nameof(PrincipalClass.PrincipalClassId)));
            modelBuilder.Entity<PrincipalClass>().Property(x => x.IntWithIndex)
                .HasColumnName(GetName(nameof(PrincipalClass.IntWithIndex)));

            modelBuilder.Entity<PrincipalClass>().HasKey(x => x.PrincipalClassId)
                .HasName(GetName("PK_PrincipalClass"));
            modelBuilder.Entity<PrincipalClass>().HasIndex(x => x.IntWithIndex)
                .HasName(GetName("IX_PrincipalClass_IntWithIndex"));

            modelBuilder.Entity<DependentClass>().ToTable(GetName("Dependents"), GetName("SchemaTwo"));

            modelBuilder.Entity<DependentClass>().Property(x => x.DependentClassId)
                .HasColumnName(GetName(nameof(DependentClass.DependentClassId)));
            modelBuilder.Entity<DependentClass>().Property(x => x.PrincipalClassId)
                .HasColumnName(GetName(nameof(DependentClass.PrincipalClassId)));

            modelBuilder.Entity<DependentClass>().HasKey(x => x.DependentClassId)
                .HasName(GetName("PK_DependentClass"));

            modelBuilder.Entity<PrincipalClass>()
                .HasOne(x => x.Dependent)
                .WithOne()
                .HasForeignKey<DependentClass>(x => x.PrincipalClassId)
                .HasConstraintName(GetName("FK_Dependent_Principal_PrincipalClassId"));
        }
    }
}