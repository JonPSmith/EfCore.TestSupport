﻿// Copyright (c) 2018 Jon P Smith, GitHub: JonPSmith, web: http://www.thereformedprogrammer.net/
// Licensed under MIT licence. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using StatusGeneric;

namespace DataLayer.DddBookApp
{
    [DebuggerDisplay("Title = {Title}")]
    public class DddBook
    {
        public const int PromotionalTextLength = 200;
        //[JsonProperty]
        private HashSet<DddBookAuthor> _authorsLink;

        //-----------------------------------------------
        //relationships

        //Use uninitialised backing fields - this means we can detect if the collection was loaded
        //[JsonProperty]
        private HashSet<DddReview> _reviews;

        //-----------------------------------------------
        //ctors

        private DddBook() { }

        [JsonConstructor]
        private DddBook(string title, string description, DateTime publishedOn, string publisher, decimal orgPrice,
            decimal actualPrice, string promotionalText, string imageUrl, IEnumerable<DddBookAuthor> authorsLink, IEnumerable<DddReview> reviews)
        {
            if (string.IsNullOrWhiteSpace(title))
                throw new ArgumentNullException(nameof(title));

            Title = title;
            Description = description;
            PublishedOn = publishedOn;
            Publisher = publisher;
            OrgPrice = orgPrice;
            ActualPrice = actualPrice;
            PromotionalText = promotionalText;
            ImageUrl = imageUrl;

            _authorsLink = new HashSet<DddBookAuthor>(authorsLink);
            _reviews = reviews == null ? null : new HashSet<DddReview>(reviews);
        }

        public static IStatusGeneric<DddBook> CreateBook(string title, string description, DateTime publishedOn,
            string publisher, decimal price, string imageUrl, ICollection<DddAuthor> authors)
        {
            var status = new StatusGenericHandler<DddBook>();
            if (string.IsNullOrWhiteSpace(title))
                status.AddError("The book title cannot be empty.");

            var book = new DddBook
            {
                Title = title,
                Description = description,
                PublishedOn = publishedOn,
                Publisher = publisher,
                ActualPrice = price,
                OrgPrice = price,
                ImageUrl = imageUrl,
                _reviews = new HashSet<DddReview>()       //We add an empty list on create. I allows reviews to be added when building test data
            };
            if (authors == null)
                throw new ArgumentNullException(nameof(authors));

            byte order = 0;
            book._authorsLink = new HashSet<DddBookAuthor>(authors.Select(a => new DddBookAuthor(book, a, order++)));
            if (!book._authorsLink.Any())
                status.AddError("You must have at least one Author for a book.");

            return status.SetResult(book);
        }

        [Key]
        public int BookId { get; private set; }
        [Required(AllowEmptyStrings = false)]
        public string Title { get; private set; }
        public string Description { get; private set; }
        public DateTime PublishedOn { get; set; }
        public string Publisher { get; private set; }
        public decimal OrgPrice { get; private set; }
        public decimal ActualPrice { get; private set; }

        [MaxLength(PromotionalTextLength)]
        public string PromotionalText { get; private set; }

        public string ImageUrl { get; private set; }

        //[JsonIgnore]
        public IEnumerable<DddReview> Reviews => _reviews?.ToList();
        //[JsonIgnore]
        public IEnumerable<DddBookAuthor> AuthorsLink => _authorsLink?.ToList();

        public void UpdatePublishedOn(DateTime publishedOn)
        {
            PublishedOn = publishedOn;
        }

        public void AddReview(int numStars, string comment, string voterName, 
            DbContext context = null) 
        {
            if (_reviews != null)    
            {
                _reviews.Add(new DddReview(numStars, comment, voterName));   
            }
            else if (context == null)
            {
                throw new ArgumentNullException(nameof(context), 
                    "You must provide a context if the Reviews collection isn't valid.");
            }
            else if (context.Entry(this).IsKeySet)  
            {
                context.Add(new DddReview(numStars, comment, voterName, BookId));
            }
            else                                     
            {                                        
                throw new InvalidOperationException("Could not add a new review.");  
            }
        }

        public void RemoveReview(DddReview dddReview, DbContext context = null)                          
        {
            if (_reviews != null)
            {
                //This is there to handle the add/remove of reviews when first created (or someone uses an .Include(p => p.Reviews)
                _reviews.Remove(dddReview); 
            }
            else if (context == null)
            {
                throw new ArgumentNullException(nameof(context),
                    "You must provide a context if the Reviews collection isn't valid.");
            }
            else if (dddReview.BookId != BookId || dddReview.ReviewId <= 0)
            {
                // This ensures that the review is a) linked to the book you defined, and b) the review has a valid primary key
                throw new InvalidOperationException("The review either hasn't got a valid primary key or was not linked to the Book.");
            }
            else
            {
                //NOTE: EF Core can delete a entity even if it isn't loaded - it just has to have a valid primary key.
                context.Remove(dddReview);
            }
        }

        public IStatusGeneric AddPromotion(decimal actualPrice, string promotionalText)                  
        {
            var status = new StatusGenericHandler();
            if (string.IsNullOrWhiteSpace(promotionalText))
            {
                status.AddError("You must provide some text to go with the promotion.", nameof(PromotionalText));
                return status;
            }

            ActualPrice = actualPrice;  
            PromotionalText = promotionalText;

            status.Message = $"The book's new price is ${actualPrice:F}.";

            return status; 
        }

        public void RemovePromotion() 
        {
            ActualPrice = OrgPrice; 
            PromotionalText = null; 
        }
    }

}