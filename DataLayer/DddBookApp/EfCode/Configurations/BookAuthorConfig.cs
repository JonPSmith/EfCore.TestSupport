// Copyright (c) 2018 Jon P Smith, GitHub: JonPSmith, web: http://www.thereformedprogrammer.net/
// Licensed under MIT licence. See License.txt in the project root for license information.

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DataLayer.DddBookApp.EfCode.Configurations
{
    public class BookAuthorConfig : IEntityTypeConfiguration<DddBookAuthor>
    {
        public void Configure
            (EntityTypeBuilder<DddBookAuthor> entity)
        {
            entity.HasKey(p => new { p.BookId, p.AuthorId });

            //-----------------------------
            //Relationships

            //entity.HasOne(pt => pt.DddBook)  
            //    .WithMany(p => p.AuthorsLink)
            //    .HasForeignKey(pt => pt.BookId);

            //entity.HasOne(pt => pt.DddAuthor)
            //    .WithMany(t => t.BooksLink)  
            //    .HasForeignKey(pt => pt.AuthorId);
        }
    }
}