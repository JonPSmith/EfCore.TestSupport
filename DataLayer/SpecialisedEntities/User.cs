// Copyright (c) 2017 Jon P Smith, GitHub: JonPSmith, web: http://www.thereformedprogrammer.net/
// Licensed under MIT licence. See License.txt in the project root for license information.

namespace DataLayer.SpecialisedEntities
{
    public class User
    {
        public int UserId { get; set; }

        public string Name { get; set; }

        public Address HomeAddress { get; set; }
    }
}