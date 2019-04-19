// Copyright (c) 2018 Jon P Smith, GitHub: JonPSmith, web: http://www.thereformedprogrammer.net/
// Licensed under MIT licence. See License.txt in the project root for license information.

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DataLayer.DddBookApp.EfCode.Configurations
{
    public class BookConfig : IEntityTypeConfiguration<DddBook>
    {
        public void Configure
            (EntityTypeBuilder<DddBook> entity)
        {
            entity.Property(p => p.PublishedOn).HasColumnType("date");        

            entity.Property(p => p.ActualPrice)
                .HasColumnType("decimal(9,2)");

            entity.Property(x => x.ImageUrl)
                .IsUnicode(false);

            entity.HasIndex(x => x.PublishedOn);
            entity.HasIndex(x => x.ActualPrice);

            //----------------------------
            //relationships

            entity.HasMany(p => p.Reviews)  
                .WithOne()                     
                .HasForeignKey(p => p.BookId);

            entity.Metadata
                .FindNavigation(nameof(DddBook.Reviews))
                .SetPropertyAccessMode(PropertyAccessMode.Field);

            entity.Metadata
                .FindNavigation(nameof(DddBook.AuthorsLink))
                .SetPropertyAccessMode(PropertyAccessMode.Field);
        }
    }
}