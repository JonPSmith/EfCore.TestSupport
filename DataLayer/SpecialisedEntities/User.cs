// Copyright (c) 2020 Jon P Smith, GitHub: JonPSmith, web: http://www.thereformedprogrammer.net/
// Licensed under MIT license. See License.txt in the project root for license information.

namespace DataLayer.SpecialisedEntities
{
    public class User
    {
        public int UserId { get; set; }

        public string Name { get; set; }

        public string Email { get; set; }

        public Address HomeAddress { get; set; }
    }
}