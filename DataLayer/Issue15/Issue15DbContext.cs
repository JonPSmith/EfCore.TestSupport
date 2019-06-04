// Copyright (c) 2019 Jon P Smith, GitHub: JonPSmith, web: http://www.thereformedprogrammer.net/
// Licensed under MIT license. See License.txt in the project root for license information.

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DataLayer.Issue15
{
    public class Issue15DbContext : DbContext
    {
        public DbSet<Message> Messages { get; set; }

        public Issue15DbContext(DbContextOptions<Issue15DbContext> options) : base(options)
        {
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            var b = modelBuilder.Entity<Message>();

            b.Property(p => p.StringRequiredDefaultEmpty).IsRequired().HasDefaultValue("");
            b.Property(p => p.StringRequiredDefaultSomething).IsRequired().HasDefaultValue("something");
            b.Property(p => p.StringRequiredComputedColumnSql).HasComputedColumnSql("left([StringRequiredDefaultSomething],(4))");
            b.Property(p => p.StringRequiredNoDefault).IsRequired();

            b.Property(p => p.IntRequiredDefault0).IsRequired().HasDefaultValue(0);
            b.Property(p => p.IntRequiredDefault8).IsRequired().HasDefaultValue(8);
            b.Property(p => p.IntRequiredNoDefault).IsRequired();

            b.Property(p => p.EnumRequiredDefaultZero).IsRequired().HasDefaultValue(EnumType.Zero);
            b.Property(p => p.EnumRequiredDefaultOne).IsRequired().HasDefaultValue(EnumType.One);
            b.Property(p => p.EnumRequiredNoDefault).IsRequired();

            b.Property(p => p.BoolRequiredDefaultFalse).IsRequired().HasDefaultValue(false);
            b.Property(p => p.BoolRequiredDefaultTrue).IsRequired().HasDefaultValue(true);
            b.Property(p => p.BoolRequiredNoDefault).IsRequired();

            b.Property(p => p.XmlRequiredDefaultEmpty).HasColumnType("xml").IsRequired().HasDefaultValue("");
            b.Property(p => p.XmlRequiredDefaultSomething).HasColumnType("xml").IsRequired().HasDefaultValue("<something />");
            b.Property(p => p.XmlRequiredNoDefault).HasColumnType("xml").IsRequired();
        }

    }
}