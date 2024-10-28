using Microsoft.EntityFrameworkCore;
using BookRentalService.Models;

namespace BookRentalService.DbContextHelper
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        public DbSet<Book> Books { get; set; }
        public DbSet<User> Users { get; set; }
        public DbSet<Rental> Rentals { get; set; }
        public DbSet<Notification> Notifications { get; set; }
        public DbSet<WaitingList> WaitingLists { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<Book>()
                .HasKey(b => b.BookId);

            modelBuilder.Entity<User>()
                .HasKey(u => u.UserId);

            modelBuilder.Entity<Rental>()
                .HasKey(r => r.RentalId);

            modelBuilder.Entity<Notification>()
                .HasKey(n => n.NotificationId);

            modelBuilder.Entity<WaitingList>()
                .HasKey(w => w.WaitingListId);

            modelBuilder.Entity<Rental>()
                .HasOne(r => r.User)
                .WithMany(u => u.Rentals)
                .HasForeignKey(r => r.UserId);

            modelBuilder.Entity<Rental>()
                .HasOne(r => r.Book)
                .WithMany(b => b.Rentals)
                .HasForeignKey(r => r.BookId);

            modelBuilder.Entity<Notification>()
                .HasOne(n => n.Rental)
                .WithOne()
                .HasForeignKey<Notification>(n => n.RentalId);

            modelBuilder.Entity<WaitingList>()
                .HasOne(w => w.User)
                .WithMany()
                .HasForeignKey(w => w.UserId);

            modelBuilder.Entity<WaitingList>()
                .HasOne(w => w.Book)
                .WithMany()
                .HasForeignKey(w => w.BookId);
        }
    }

}
