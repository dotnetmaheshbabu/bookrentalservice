using BookRentalService.Models;

namespace BookRentalService.Repository
{
    public interface IBookRepository : IRepository<Book>
    {
        Task<IEnumerable<Book>> SearchBooksAsync(string title, string genre);
    }
}
