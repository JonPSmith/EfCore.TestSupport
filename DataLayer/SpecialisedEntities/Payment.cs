// Copyright (c) 2017 Jon P Smith, GitHub: JonPSmith, web: http://www.thereformedprogrammer.net/
// Licensed under MIT licence. See License.txt in the project root for license information.
namespace DataLayer.SpecialisedEntities
{
    public enum PTypes : byte {  Cash = 1, Card = 2}
    public abstract class Payment
    {
        public int PaymentId { get; set; }

        public PTypes PType { get; set; }

        public decimal Amount { get; set; }
    }
}