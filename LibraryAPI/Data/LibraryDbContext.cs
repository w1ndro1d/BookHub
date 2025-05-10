using LibraryAPI.Models;
using Microsoft.EntityFrameworkCore;

namespace LibraryAPI.Data
{
    public class LibraryDbContext : DbContext
    {
        public LibraryDbContext(DbContextOptions<LibraryDbContext> options): base(options)
        {
            
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // One-to-one: Member has one Cart
            modelBuilder.Entity<Member>()
                .HasOne(m => m.Cart)
                .WithOne(c => c.Member)
                .HasForeignKey<Cart>(c => c.MemberId);

            // One-to-many: Cart has many CartItems
            modelBuilder.Entity<Cart>()
                .HasMany(c => c.Items)
                .WithOne(i => i.Cart)
                .HasForeignKey(i => i.CartId);

            // Many-to-one: CartItem -> Book
            modelBuilder.Entity<CartItem>()
                .HasOne(ci => ci.Book)
                .WithMany()
                .HasForeignKey(ci => ci.BookId);
        }

        public DbSet<Book> Books { get; set; }
        public DbSet<Member> Members { get; set; }
        public DbSet<Order> Orders { get; set; }
        public DbSet<OrderItem> OrderItems { get; set; }
        public DbSet<Review> Reviews { get; set; }
        public DbSet<Cart> Carts { get; set; }
        public DbSet<CartItem> CartItems { get; set; }
        public DbSet<Bookmark> Bookmarks { get; set; } 
        public DbSet<Announcement> Announcements { get; set; }
    }
}
