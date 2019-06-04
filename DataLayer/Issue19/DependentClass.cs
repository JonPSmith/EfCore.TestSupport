// Copyright (c) 2019 Jon P Smith, GitHub: JonPSmith, web: http://www.thereformedprogrammer.net/
// Licensed under MIT license. See License.txt in the project root for license information.

namespace DataLayer.Issue19
{
    public class DependentClass
    {
        public int DependentClassId { get; set; }
        //Foreign Key
        public int PrincipalClassId { get; set; }
    }
}