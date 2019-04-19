// Copyright (c) 2019 Jon P Smith, GitHub: JonPSmith, web: http://www.thereformedprogrammer.net/
// Licensed under MIT license. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using Newtonsoft.Json;
using TestSupport.SeedDatabase;
using Xunit;
using Xunit.Abstractions;

namespace Test.UnitTests.TestDataResetter
{
    public class TestDuplicateObjectInJsonSerialize
    {
        private readonly ITestOutputHelper _output;

        public TestDuplicateObjectInJsonSerialize(ITestOutputHelper output)
        {
            _output = output;
        }

        [Fact]
        public void TestJsonSerialize()
        {
            //SETUP
            var many1 = new ManyToMany {ManyId = 1};
            var many2 = new ManyToMany {ManyId = 2};
            var l1a = new Level1(many1, 1);
            var l1b = new Level1(many2, 2);
            var l2 = new Level2 {L2Int = 3};
            many1.L1 = l1a;
            many1.L2 = l2;
            many2.L1 = l1b;
            many2.L2 = l2;
            l2.Many = new List<ManyToMany> { many1, many2 };

            var entities = new List<Level1> {l1a, l1b};

            //ATTEMPT
            var json = entities.DefaultSerializeToJson();

            //VERIFY
            _output.WriteLine(json);
        }

        private class Level1
        {
            //[JsonProperty]
            private readonly HashSet<ManyToMany> _many;

            public Level1(ManyToMany many, int l1Int)
            {
                _many = new HashSet<ManyToMany>{many};
                L1Int = l1Int;
            }

            public int L1Int { get; set; }
            public IEnumerable<ManyToMany> Many => _many.ToList();
        }

        private class Level2
        {
            public int L2Int { get; set; }
            public ICollection<ManyToMany> Many { get; set; }
        }

        private class ManyToMany
        {
            public int ManyId { get; set; }
            public Level1 L1 { get; set; }
            public Level2 L2 { get; set; }
        }


    
    }
}