// Copyright (c) 2017 Jon P Smith, GitHub: JonPSmith, web: http://www.thereformedprogrammer.net/
// Licensed under MIT licence. See License.txt in the project root for license information.

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DataLayer.BookApp.Configurations
{
    public class BookAuthorConfig : IEntityTypeConfiguration<BookAuthor>
    {
        public void Configure
            (EntityTypeBuilder<BookAuthor> entity)
        {
            entity.HasKey(p => 
                new { p.BookId, p.AuthorId });

            //-----------------------------
            //Relationships

            entity.HasOne(pt => pt.Book)        
                .WithMany(p => p.AuthorsLink)   
                .HasForeignKey(pt => pt.BookId);

            entity.HasOne(pt => pt.Author)        
                .WithMany(t => t.BooksLink)       
                .HasForeignKey(pt => pt.AuthorId);
        }
    }
}