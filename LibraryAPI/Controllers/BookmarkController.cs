using LibraryAPI.Data;
using LibraryAPI.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace LibraryAPI.Controllers
{
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
            var memberId = int.Parse(User.FindFirst("memberId")?.Value ?? "0");

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

        public class BookmarkModel
        {
            public int BookId { get; set; }
        }
    }
}
