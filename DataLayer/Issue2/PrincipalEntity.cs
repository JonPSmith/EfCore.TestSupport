// // Copyright (c) 2017 Jon P Smith, GitHub: JonPSmith, web: http://www.thereformedprogrammer.net/
// // Licensed under MIT licence. See License.txt in the project root for license information.

namespace DataLayer.Issue2
{
    public class PrincipalEntity
    {
        public int PrincipalEntityId { get; set; }

        public PrimaryKeyIsFKey Relationship { get; set; }
    }
}