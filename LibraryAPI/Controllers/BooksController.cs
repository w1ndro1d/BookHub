using LibraryAPI.Data;
using LibraryAPI.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;

namespace LibraryAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class BooksController : ControllerBase
    {
        private readonly LibraryDbContext _dbContext;

        public BooksController(LibraryDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        // This method returns all books without any filters
        [HttpGet("all")]
        public async Task<ActionResult<IEnumerable<Book>>> GetAllBooks()
        {
            var books = await _dbContext.Books.ToListAsync();
            return Ok(books);
        }
        // This method applies filters and sorting
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Book>>> GetBooks(
            [FromQuery] string searchQuery = null,
            [FromQuery] string filterAuthor = null,
            [FromQuery] string filterGenre = null,
            [FromQuery] decimal? filterPriceMin = null,
            [FromQuery] decimal? filterPriceMax = null,
            [FromQuery] string filterLanguage = null,
            [FromQuery] string filterFormat = null,
            [FromQuery] string filterISBN = null,
            [FromQuery] string sortOrder = "Title",
            [FromQuery] int? filterAvailability = null,
            [FromQuery] string category = null)
        {

            var query = _dbContext.Books.AsQueryable();

            if (!string.IsNullOrEmpty(searchQuery))
            {
                var pattern = $"%{searchQuery}%";
                query = query.Where(b =>
                    EF.Functions.Like(b.Title, pattern) ||
                    EF.Functions.Like(b.Description, pattern) ||
                    EF.Functions.Like(b.ISBN, pattern));
            }

            if (!string.IsNullOrEmpty(filterAuthor))
            {
                query = query.Where(b => EF.Functions.Like(b.Author, $"%{filterAuthor}%"));
            }

            if (!string.IsNullOrEmpty(filterGenre))
            {
                query = query.Where(b => EF.Functions.Like(b.Genre, $"%{filterGenre}%"));
            }

            if (!string.IsNullOrEmpty(filterLanguage))
            {
                query = query.Where(b => EF.Functions.Like(b.Language, $"%{filterLanguage}%"));
            }

            if (!string.IsNullOrEmpty(filterFormat))
            {
                query = query.Where(b => EF.Functions.Like(b.Format, $"%{filterFormat}%"));
            }

            if (!string.IsNullOrEmpty(filterISBN))
            {
                query = query.Where(b => EF.Functions.Like(b.ISBN, $"%{filterISBN}%"));
            }

            if (filterPriceMin.HasValue)
            {
                query = query.Where(b => b.Price >= filterPriceMin.Value);
            }

            if (filterPriceMax.HasValue)
            {
                query = query.Where(b => b.Price <= filterPriceMax.Value);
            }

            if (filterAvailability.HasValue)
            {
                query = query.Where(b => b.InStock == filterAvailability);
            }

            // Apply category filters
            if (!string.IsNullOrEmpty(category))
            {
                var normalized = category.Trim().ToLower();
                query = normalized switch
                {
                    "bestsellers" => query.Where(b => b.IsBestseller),
                    "awardwinners" => query.Where(b => b.HasAwards),
                    "newreleases" => query.Where(b => b.PublicationDate >= DateTime.Now.AddMonths(-3)), //published in the last 3 months
                    "newarrivals" => query.Where(b => b.ListedDate >= DateTime.Now.AddMonths(-1)),  //listed date in the past month
                    "comingsoon" => query.Where(b => b.PublicationDate > DateTime.Now), //will be published at a later date
                    "deals" => query.Where(b => b.Discount > 0),
                    _ => query
                };
            }

            // Apply sorting
            switch (sortOrder)
            {
                case "price":
                    query = query.OrderBy(b => b.Price);
                    break;
                case "publicationDate":
                    query = query.OrderByDescending(b => b.PublicationDate);
                    break;
                case "popularity":
                    query = query.OrderByDescending(b => b.Rating);
                    break;
                default:
                    query = query.OrderBy(b => b.Title);
                    break;
            }

            // Fetch the filtered and sorted data
            var books = await query.ToListAsync();
            return Ok(books);
        }
    }
}
