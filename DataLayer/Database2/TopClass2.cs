// Copyright (c) 2020 Jon P Smith, GitHub: JonPSmith, web: http://www.thereformedprogrammer.net/
// Licensed under MIT license. See License.txt in the project root for license information.

using System.Collections.Generic;

namespace DataLayer.Database2
{
    public class TopClass2
    {
        public int Id { get; set; }

        public string MyString { get; set; }

        public ICollection<Dependent2> Dependents { get; set; }
    }
}