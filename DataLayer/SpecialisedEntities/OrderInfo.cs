// Copyright (c) 2017 Jon P Smith, GitHub: JonPSmith, web: http://www.thereformedprogrammer.net/
// Licensed under MIT licence. See License.txt in the project root for license information.

namespace DataLayer.SpecialisedEntities
{
    public class OrderInfo 
    {
        public int OrderInfoId { get; set; }
        public string OrderNumber { get; set; }

        public Address BillingAddress { get; set; } //#B
        public Address DeliveryAddress { get; set; } //#B
    }
}