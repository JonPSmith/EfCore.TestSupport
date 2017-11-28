// Copyright (c) 2017 Jon P Smith, GitHub: JonPSmith, web: http://www.thereformedprogrammer.net/
// Licensed under MIT licence. See License.txt in the project root for license information.

using System.ComponentModel.DataAnnotations.Schema;

namespace DataLayer.Issue3
{
    public enum ValueAggregationTypeEnum : byte { Invariable = 1, Minimum = 2, Maximum = 3, Average = 4 }
    public class Parameter
    {     
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int ParameterId { get; set; }
        public ValueAggregationTypeEnum ValueAggregationTypeId { get; set; }
        public decimal? NumericValue { get; set; }
    }
}
