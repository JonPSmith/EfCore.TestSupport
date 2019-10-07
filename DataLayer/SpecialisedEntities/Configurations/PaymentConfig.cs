// Copyright (c) 2017 Jon P Smith, GitHub: JonPSmith, web: http://www.thereformedprogrammer.net/
// Licensed under MIT licence. See License.txt in the project root for license information.

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DataLayer.SpecialisedEntities.Configurations
{
    public class PaymentConfig : IEntityTypeConfiguration<Payment>
    {
        public void Configure
            (EntityTypeBuilder<Payment> entity)
        {
            entity.HasDiscriminator(b => b.PType) //#A
                .HasValue<PaymentCash>(PTypes.Cash) //#B
                .HasValue<PaymentCard>(PTypes.Card); //#C
        }
    }
}