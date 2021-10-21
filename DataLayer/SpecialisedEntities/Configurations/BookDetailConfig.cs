// Copyright (c) 2017 Jon P Smith, GitHub: JonPSmith, web: http://www.thereformedprogrammer.net/
// Licensed under MIT licence. See License.txt in the project root for license information.

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DataLayer.SpecialisedEntities.Configurations
{
    public class BookDetailConfig : IEntityTypeConfiguration<BookDetail>
    {
        public void Configure
            (EntityTypeBuilder<BookDetail> entity)
        {
            entity.ToTable("Books");
        }
    }
}