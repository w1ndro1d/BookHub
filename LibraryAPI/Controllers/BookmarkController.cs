using LibraryAPI.Data;
using LibraryAPI.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace LibraryAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class BookmarkController : ControllerBase
    {
        private readonly LibraryDbContext _dbContext;

        public BookmarkController(LibraryDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        [HttpPost]
        [Authorize]
        public async Task<IActionResult> Bookmark([FromBody] BookmarkModel model)
        {
            var memberId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");

            if (await _dbContext.Bookmarks.AnyAsync(b => b.MemberId == memberId && b.BookId == model.BookId))
                return BadRequest("Already bookmarked.");

            var bookmark = new Bookmark
            {
                MemberId = memberId,
                BookId = model.BookId,
                CreatedAt = DateTime.UtcNow
            };

            _dbContext.Bookmarks.Add(bookmark);
            await _dbContext.SaveChangesAsync();

            return Ok();
        }

        [HttpGet]
        [Authorize]
        public async Task<IActionResult> GetBookmarks()
        {
            var memberId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");

            var bookmarks = await _dbContext.Bookmarks
                .Include(b => b.Book)
                .Where(b => b.MemberId == memberId)
                .Select(b => new
                {
                    b.Book.Id,
                    b.Book.Title,
                    b.Book.Author,
                    b.Book.Genre,
                    b.Book.Price,
                    b.Book.InStock
                })
                .ToListAsync();

            return Ok(bookmarks);
        }

        [HttpDelete("{bookId}")]
        [Authorize]
        public async Task<IActionResult> RemoveBookmark(int bookId)
        {
            var memberId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");

            var bookmark = await _dbContext.Bookmarks
                .FirstOrDefaultAsync(b => b.MemberId == memberId && b.BookId == bookId);

            if (bookmark == null)
                return NotFound();

            _dbContext.Bookmarks.Remove(bookmark);
            await _dbContext.SaveChangesAsync();

            return Ok();
        }

        public class BookmarkModel
        {
            public int BookId { get; set; }
        }
    }
}
