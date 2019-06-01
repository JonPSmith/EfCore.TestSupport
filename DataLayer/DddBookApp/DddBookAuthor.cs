// Copyright (c) 2018 Jon P Smith, GitHub: JonPSmith, web: http://www.thereformedprogrammer.net/
// Licensed under MIT licence. See License.txt in the project root for license information.

using Newtonsoft.Json;

namespace DataLayer.DddBookApp
{
    public class DddBookAuthor
    {
        private DddBookAuthor() { }

        internal DddBookAuthor(DddBook dddBook, DddAuthor dddAuthor, byte order)
        {
            DddBook = dddBook;
            DddAuthor = dddAuthor;
            Order = order;
        }

        public int BookId { get; private set; }
        public int AuthorId { get; private set; }
        public byte Order { get; private set; }

        //-----------------------------
        //Relationships

        [JsonProperty]
        public DddBook DddBook { get; private set; }
        [JsonProperty]
        public DddAuthor DddAuthor { get; private set; }
    }
}