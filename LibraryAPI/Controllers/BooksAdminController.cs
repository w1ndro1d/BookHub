using LibraryAPI.Data;
using LibraryAPI.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace LibraryAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(Roles = "Admin")]
    public class BooksAdminController : ControllerBase
    {
        private readonly LibraryDbContext _dbContext;

        public BooksAdminController(LibraryDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        // GET: api/admin/books
        [HttpGet]
        public async Task<IActionResult> GetAllBooks()
        {
            var books = await _dbContext.Books.ToListAsync();
            return Ok(books);
        }

        // POST: api/admin/books
        [HttpPost]
        public async Task<IActionResult> CreateBook([FromBody] Book model)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            _dbContext.Books.Add(model);
            await _dbContext.SaveChangesAsync();

            return CreatedAtAction(nameof(GetAllBooks), new { id = model.Id }, model);
        }

        // PUT: api/admin/books/{id}
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateBook(int id, [FromBody] Book updatedBook)
        {
            var book = await _dbContext.Books.FindAsync(id);
            if (book == null)
                return NotFound();

            // Update fields
            book.Title = updatedBook.Title;
            book.ISBN = updatedBook.ISBN;
            book.Author = updatedBook.Author;
            book.Description = updatedBook.Description;
            book.Genre = updatedBook.Genre;
            book.Price = updatedBook.Price;
            book.InStock = updatedBook.InStock;
            book.Rating = updatedBook.Rating;
            book.Format = updatedBook.Format;
            book.Language = updatedBook.Language;
            book.PublicationDate = updatedBook.PublicationDate;
            book.Discount = updatedBook.Discount;
            book.HasAwards = updatedBook.HasAwards;
            book.IsBestseller = updatedBook.IsBestseller;

            await _dbContext.SaveChangesAsync();
            return Ok(book);
        }

        // DELETE: api/admin/books/{id}
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteBook(int id)
        {
            var book = await _dbContext.Books.FindAsync(id);
            if (book == null)
                return NotFound();

            _dbContext.Books.Remove(book);
            await _dbContext.SaveChangesAsync();

            return NoContent();
        }
    }
}
