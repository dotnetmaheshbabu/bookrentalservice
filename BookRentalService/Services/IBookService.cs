using BookRentalService.Models;

namespace BookRentalService.Services
{
    public interface IBookService
    {
        Task<IEnumerable<Book>> SearchBooksAsync(string title, string genre);
        Task<string> RentBookAsync(int userId, int bookId);
        Task ReturnBookAsync(int rentalId);
        Task<IEnumerable<Rental>> GetUserRentalHistoryAsync(int userId);
        Task ExtendDueDateAsync(int rentalId, int days);
        Task AddToWaitingListAsync(int userId, int bookId);
        Task DeleteWaitingListAsync(int userId, int bookId);
        Task NotifyNextInLineAsync(int bookId);
        Task<IEnumerable<Rental>> GetOverdueRentalsAsync();
    }
}
