﻿// <auto-generated />
using System;
using DataLayer.BookApp.EfCode;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

namespace DataLayer.BookApp.Migrations
{
    [DbContext(typeof(BookContext))]
    partial class BookContextModelSnapshot : ModelSnapshot
    {
        protected override void BuildModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("Relational:MaxIdentifierLength", 63)
                .HasAnnotation("ProductVersion", "5.0.10")
                .HasAnnotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn);

            modelBuilder.Entity("DataLayer.BookApp.Author", b =>
                {
                    b.Property<int>("AuthorId")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("integer")
                        .HasAnnotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn);

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasMaxLength(100)
                        .HasColumnType("character varying(100)");

                    b.HasKey("AuthorId");

                    b.ToTable("Authors");
                });

            modelBuilder.Entity("DataLayer.BookApp.Book", b =>
                {
                    b.Property<int>("BookId")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("integer")
                        .HasAnnotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn);

                    b.Property<string>("Description")
                        .HasColumnType("text");

                    b.Property<string>("ImageUrl")
                        .HasMaxLength(512)
                        .IsUnicode(false)
                        .HasColumnType("character varying(512)");

                    b.Property<decimal>("Price")
                        .HasColumnType("numeric(9,2)");

                    b.Property<DateTime>("PublishedOn")
                        .HasColumnType("date");

                    b.Property<string>("Publisher")
                        .HasMaxLength(64)
                        .HasColumnType("character varying(64)");

                    b.Property<bool>("SoftDeleted")
                        .HasColumnType("boolean");

                    b.Property<string>("Title")
                        .IsRequired()
                        .HasMaxLength(256)
                        .HasColumnType("character varying(256)");

                    b.HasKey("BookId");

                    b.HasIndex("PublishedOn");

                    b.ToTable("Books");
                });

            modelBuilder.Entity("DataLayer.BookApp.BookAuthor", b =>
                {
                    b.Property<int>("BookId")
                        .HasColumnType("integer");

                    b.Property<int>("AuthorId")
                        .HasColumnType("integer");

                    b.Property<byte>("Order")
                        .HasColumnType("smallint");

                    b.HasKey("BookId", "AuthorId");

                    b.HasIndex("AuthorId");

                    b.ToTable("BookAuthor");
                });

            modelBuilder.Entity("DataLayer.BookApp.PriceOffer", b =>
                {
                    b.Property<int>("PriceOfferId")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("integer")
                        .HasAnnotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn);

                    b.Property<int>("BookId")
                        .HasColumnType("integer");

                    b.Property<decimal>("NewPrice")
                        .HasColumnType("numeric(9,2)");

                    b.Property<string>("PromotionalText")
                        .IsRequired()
                        .HasMaxLength(200)
                        .HasColumnType("character varying(200)");

                    b.HasKey("PriceOfferId");

                    b.HasIndex("BookId")
                        .IsUnique();

                    b.ToTable("PriceOffers");
                });

            modelBuilder.Entity("DataLayer.BookApp.Review", b =>
                {
                    b.Property<int>("ReviewId")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("integer")
                        .HasAnnotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn);

                    b.Property<int>("BookId")
                        .HasColumnType("integer");

                    b.Property<string>("Comment")
                        .HasColumnType("text");

                    b.Property<int>("NumStars")
                        .HasColumnType("integer");

                    b.Property<string>("VoterName")
                        .HasMaxLength(100)
                        .HasColumnType("character varying(100)");

                    b.HasKey("ReviewId");

                    b.HasIndex("BookId");

                    b.ToTable("Review");
                });

            modelBuilder.Entity("DataLayer.BookApp.BookAuthor", b =>
                {
                    b.HasOne("DataLayer.BookApp.Author", "Author")
                        .WithMany("BooksLink")
                        .HasForeignKey("AuthorId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("DataLayer.BookApp.Book", "Book")
                        .WithMany("AuthorsLink")
                        .HasForeignKey("BookId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("Author");

                    b.Navigation("Book");
                });

            modelBuilder.Entity("DataLayer.BookApp.PriceOffer", b =>
                {
                    b.HasOne("DataLayer.BookApp.Book", null)
                        .WithOne("Promotion")
                        .HasForeignKey("DataLayer.BookApp.PriceOffer", "BookId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();
                });

            modelBuilder.Entity("DataLayer.BookApp.Review", b =>
                {
                    b.HasOne("DataLayer.BookApp.Book", null)
                        .WithMany("Reviews")
                        .HasForeignKey("BookId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();
                });

            modelBuilder.Entity("DataLayer.BookApp.Author", b =>
                {
                    b.Navigation("BooksLink");
                });

            modelBuilder.Entity("DataLayer.BookApp.Book", b =>
                {
                    b.Navigation("AuthorsLink");

                    b.Navigation("Promotion");

                    b.Navigation("Reviews");
                });
#pragma warning restore 612, 618
        }
    }
}
