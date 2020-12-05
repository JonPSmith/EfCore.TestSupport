// Copyright (c) 2020 Jon P Smith, GitHub: JonPSmith, web: http://www.thereformedprogrammer.net/
// Licensed under MIT license. See License.txt in the project root for license information.

namespace DataLayer.SpecialisedEntities
{
    public class Address
    {
        public string NumberAndStreet { get; set; }
        public string City { get; set; }
        public string ZipPostCode { get; set; }
        public string CountryCodeIso2 { get; set; }
    }
}