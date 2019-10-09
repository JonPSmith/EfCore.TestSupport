// Copyright (c) 2017 Jon P Smith, GitHub: JonPSmith, web: http://www.thereformedprogrammer.net/
// Licensed under MIT licence. See License.txt in the project root for license information.

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DataLayer.SpecialisedEntities.Configurations
{
    public class UserConfig : IEntityTypeConfiguration<User>
    {
        public void Configure
            (EntityTypeBuilder<User> entity)
        {
            entity.HasAlternateKey(p => p.Email);

            entity
                .OwnsOne(e => e.HomeAddress)
                .ToTable("Addresses");

#if NETSTANDARD2_0
            entity.OwnsOne(e => e.HomeAddress).ToTable("Addresses");
#elif NETSTANDARD2_1
            entity.OwnsOne(e => e.HomeAddress); //Cannot map ownedtype to separate table
#endif
        }
}
}