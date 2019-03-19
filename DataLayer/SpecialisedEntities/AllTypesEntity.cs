// Copyright (c) 2019 Jon P Smith, GitHub: JonPSmith, web: http://www.thereformedprogrammer.net/
// Licensed under MIT license. See License.txt in the project root for license information.

using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DataLayer.SpecialisedEntities
{
    public class AllTypesEntity
    {
        public int Id { get; private set; }
        public bool MyBool { get; private set; } = true;
        public bool? MyBoolNullable { get; private set; } = null;
        public int MyInt { get; private set; } = 1234;
        public int? MyIntNullable { get; private set; } = null;
        public double MyDouble { get; private set; } = 5678.9012;
        public Decimal MyDecimal { get; private set; } = 3456.789m;
        public Guid MyGuid { get;  set; }
        public Guid? MyGuidNullable { get; private set; } = null;

        public string MyString { get; private set; } = "string with ' in it";
        public string MyStringNull { get; private set; } = null;
        public string MyStringEmptyString { get; private set; } = string.Empty;

        [Required]
        [Column(TypeName = "varchar(123)")]
        public string MyAnsiNonNullString { get; private set; } = "ascii only";

        public DateTime MyDateTime { get; set; }
        public DateTime? MyDateTimeNullable { get; private set; } = null;
        public TimeSpan MyTimeSpan { get;  set; }
        public DateTimeOffset MyDateTimeOffset { get;  set; }
        public byte[] MyByteArray { get;  set; }

    }
}