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

        #region ADMIN CRUD
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
        #endregion ADMIN CRUD


        #region Admin Announcements
        //for announcements
        [HttpGet("announcements")]
        public IActionResult GetActiveAnnouncements()
        {
            //var now = DateTime.UtcNow;
            //var active = _dbContext.Announcements
            //    .Where(a =>
            //        (a.StartDate == null || a.StartDate <= now) &&
            //        (a.EndDate == null || a.EndDate >= now))
            //    .OrderByDescending(a => a.IsPinned)
            //    .ThenByDescending(a => a.CreatedAt)
            //    .ToList();

            // Fetch all announcements, without checking dates
            var allAnnouncements = _dbContext.Announcements
                .OrderByDescending(a => a.IsPinned)
                .ThenByDescending(a => a.CreatedAt)
                .ToList();

            return Ok(allAnnouncements);
        }

        [HttpPost("announcements")]
        public IActionResult CreateAnnouncement([FromBody] Announcement announcement)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);
            announcement.CreatedAt = DateTime.Now;

            ////if another announcement is already pinned, make sure only the latest addition gets pinned
            //if (announcement.IsPinned)
            //{
            //    var alreadyPinned = _dbContext.Announcements
            //        .FirstOrDefault(a => a.IsPinned && a.Id != announcement.Id);

            //    if (alreadyPinned != null)
            //    {
            //        alreadyPinned.IsPinned = false;
            //        _dbContext.Announcements.Update(alreadyPinned);
            //    }
            //}

            _dbContext.Announcements.Add(announcement);
            _dbContext.SaveChanges();
            return Ok(announcement);
        }

        [HttpPut("announcements/{id}")]
        public IActionResult UpdateAnnouncement(int id, [FromBody] Announcement updated)
        {
            var existing = _dbContext.Announcements.Find(id);
            if (existing == null) return NotFound();

            existing.Title = updated.Title;
            existing.Message = updated.Message;
            existing.StartDate = updated.StartDate;
            existing.EndDate = updated.EndDate;
            existing.IsPinned = updated.IsPinned;

            _dbContext.SaveChanges();
            return Ok();
        }

        [HttpDelete("announcements/{id}")]
        public IActionResult DeleteAnnouncement(int id)
        {
            var a = _dbContext.Announcements.Find(id);
            if (a == null) return NotFound();

            _dbContext.Announcements.Remove(a);
            _dbContext.SaveChanges();
            return Ok();
        }



        //public api route for non-admins, to display announcements on homepage
        [AllowAnonymous]
        [HttpGet("announcements/public")]
        public IActionResult GetPublicAnnouncements()
        {
            var today = DateTime.Today;

            var publicAnnouncements = _dbContext.Announcements
                .Where(a => a.IsPinned == true &&
                            a.StartDate.HasValue && a.EndDate.HasValue &&
                            a.StartDate.Value.Date <= today &&
                            a.EndDate.Value.Date >= today)
                .ToList();

            return Ok(publicAnnouncements);
        }

        #endregion Admin Announcements
    }
}
