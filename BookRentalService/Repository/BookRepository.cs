using BookRentalService.DbContextHelper;
using BookRentalService.Models;
using Microsoft.EntityFrameworkCore;

namespace BookRentalService.Repository
{
    public class BookRepository : Repository<Book>, IBookRepository
    {
        public BookRepository(AppDbContext context) : base(context) { }

        public async Task<IEnumerable<Book>> SearchBooksAsync(string title, string genre)
        {
            return await _dbSet
                .Where(b => (string.IsNullOrEmpty(title) || b.Title.Contains(title)) &&
                            (string.IsNullOrEmpty(genre) || b.Genre.Equals(genre)))
                .ToListAsync();
        }
    }
}
