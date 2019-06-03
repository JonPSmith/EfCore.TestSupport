// Copyright (c) 2019 Jon P Smith, GitHub: JonPSmith, web: http://www.thereformedprogrammer.net/
// Licensed under MIT license. See License.txt in the project root for license information.

namespace DataLayer.Issue15
{
    public class Message
    {
        public int MessageId { get; set; }

        public string StringRequiredDefaultEmpty { get; set; }
        public string StringRequiredDefaultSomething { get; set; }
        public string StringRequiredComputedColumnSql { get; set; }
        public string StringRequiredNoDefault { get; set; }

        public int IntRequiredDefault0 { get; set; }
        public int IntRequiredDefault8 { get; set; }
        public int IntRequiredNoDefault { get; set; }

        public EnumType EnumRequiredDefaultZero { get; set; }
        public EnumType EnumRequiredDefaultOne { get; set; }
        public EnumType EnumRequiredNoDefault { get; set; }

        public bool BoolRequiredDefaultFalse { get; set; }
        public bool BoolRequiredDefaultTrue { get; set; }
        public bool BoolRequiredNoDefault { get; set; }

        public string XmlRequiredDefaultEmpty { get; set; }
        public string XmlRequiredDefaultSomething { get; set; }
        public string XmlRequiredNoDefault { get; set; }
    }

    public enum EnumType : int
    {
        Zero = 0,
        One = 1,
    }
}