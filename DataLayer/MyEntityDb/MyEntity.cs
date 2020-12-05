// Copyright (c) 2020 Jon P Smith, GitHub: JonPSmith, web: http://www.thereformedprogrammer.net/
// Licensed under MIT license. See License.txt in the project root for license information.

using System;

namespace DataLayer.MyEntityDb
{
    public class MyEntity
    {
        private int _backingField;

        public int MyEntityId { get; set; }

        public DateTime MyDateTime { get; set; }

        public int MyInt { get; set; }

        public string MyString { get; set; }
    }
}