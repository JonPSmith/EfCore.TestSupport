// Copyright (c) 2016 Jon P Smith, GitHub: JonPSmith, web: http://www.thereformedprogrammer.net/
// Licensed under MIT licence. See License.txt in the project root for license information.

namespace DataLayer.EfClasses
{
    public class BookAuthor               
    {
        public int BookId { get; set; }  //#A
        public int AuthorId { get; set; }//#A
        public byte Order { get; set; }   

        //-----------------------------
        //Relationships

        public Book Book { get; set; }      
        public Author Author { get; set; }  
    }
    /************************************************************
    A# The primary key is make up of the two foreign keys
     * ********************************************************/

}