// Copyright (c) 2020 Jon P Smith, GitHub: JonPSmith, web: http://www.thereformedprogrammer.net/
// Licensed under MIT license. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;

namespace DataLayer.Database1
{
    public class TopClass1
    {
        public int Id { get; set; }

        public Guid MyGuid { get; set; }

        public ICollection<Dependent1> Dependents { get; set; }

    }
}