using LibraryAPI.Models;
using Microsoft.EntityFrameworkCore;

namespace LibraryAPI.Data
{
    public static class SeedData
    {
        public static void Initialize(LibraryDbContext dbContext)
        {
            //apply pending migrations(if any)
            //we want to be sure tables exist during seed value insertion
            dbContext.Database.Migrate();

            if(!dbContext.Books.Any())
            {
                dbContext.Books.AddRange(
                     new Book
                     {
                         Title = "The Great Gatsby",
                         Author = "F. Scott Fitzgerald",
                         Genre = "Classic",
                         Description = "A story of the Jazz Age.",
                         Price = 9.99m,
                         InStock = true,
                         Language = "English",
                         Format = "Paperback",
                         ISBN = "9780743273565",
                         PublicationDate = new DateTime(1925, 4, 10)
                     },
                    new Book
                    {
                        Title = "Atomic Habits",
                        Author = "James Clear",
                        Genre = "Self-Help",
                        Description = "Tiny changes, remarkable results.",
                        Price = 14.99m,
                        InStock = true,
                        Language = "English",
                        Format = "Hardcover",
                        ISBN = "9780735211292",
                        PublicationDate = new DateTime(2018, 10, 16)
                    },
                    new Book
                    {
                        Title = "1984",
                        Author = "George Orwell",
                        Genre = "Dystopian",
                        Description = "A novel about a totalitarian regime.",
                        Price = 12.99m,
                        InStock = true,
                        Language = "English",
                        Format = "Paperback",
                        ISBN = "9780451524935",
                        PublicationDate = new DateTime(1949, 6, 8)
                    },
                    new Book
                    {
                        Title = "Harry Potter and the Sorcerer's Stone",
                        Author = "J.K. Rowling",
                        Genre = "Fantasy",
                        Description = "The boy who lived.",
                        Price = 19.99m,
                        InStock = true,
                        Language = "English",
                        Format = "Hardcover",
                        ISBN = "9780590353427",
                        PublicationDate = new DateTime(1997, 6, 26)
                    },
                    new Book
                    {
                        Title = "Rich Dad Poor Dad",
                        Author = "Robert Kiyosaki",
                        Genre = "Finance",
                        Description = "What the rich teach their kids about money.",
                        Price = 15.99m,
                        InStock = true,
                        Language = "English",
                        Format = "Paperback",
                        ISBN = "9781612680194",
                        PublicationDate = new DateTime(1997, 4, 1)
                    }
                    );
                dbContext.SaveChanges();
            }
        }
    }
}
