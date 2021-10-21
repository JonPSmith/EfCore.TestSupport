// Copyright (c) 2017 Jon P Smith, GitHub: JonPSmith, web: http://www.thereformedprogrammer.net/
// Licensed under MIT licence. See License.txt in the project root for license information.

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DataLayer.SpecialisedEntities.Configurations
{
    public class BookSummaryConfig : IEntityTypeConfiguration<BookSummary>
    {
        public void Configure
            (EntityTypeBuilder<BookSummary> entity)
        {
            entity.HasKey(p => p.BookSummaryId);

            entity
                .HasOne(e => e.Details).WithOne()
                .HasForeignKey<BookDetail>(e => e.BookDetailId);
            entity.ToTable("Books");
        }
    }
}