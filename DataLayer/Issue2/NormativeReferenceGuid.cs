// Copyright (c) 2017 Jon P Smith, GitHub: JonPSmith, web: http://www.thereformedprogrammer.net/
// Licensed under MIT licence. See License.txt in the project root for license information.

using System;
using System.ComponentModel.DataAnnotations;

namespace DataLayer.Issue2
{
    public class NormativeReferenceGuid
    {
        [Key, MaxLength(50)]
        public Guid NormativeReferenceGuidId { get; set; }
        public string Name { get; set; }
    }
}