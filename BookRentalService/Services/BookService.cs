using BookRentalService.Models;
using BookRentalService.Repository;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BookRentalService.Services
{
    public class BookService : IBookService
    {
        private readonly IBookRepository _bookRepository;
        private readonly IRepository<Rental> _rentalRepository;
        private readonly IRepository<WaitingList> _waitingListRepository;
        private readonly IRentalRepository _rentRepository;
        private readonly IEmailService _emailService;
        private readonly ILogger<BookService> _logger;
        private const int MaxExtensions = 2; // Maximum number of allowed extensions

        public BookService(IEmailService emailService, IRentalRepository rentRepository, IBookRepository bookRepository, IRepository<Rental> rentalRepository, IRepository<WaitingList> waitingListRepository, ILogger<BookService> logger)
        {
            _bookRepository = bookRepository;
            _rentalRepository = rentalRepository;
            _waitingListRepository = waitingListRepository;
            _rentRepository = rentRepository;
            _emailService = emailService;
            _logger = logger;
        }

        public async Task<IEnumerable<Book>> SearchBooksAsync(string title, string genre)
        {
            _logger.LogInformation("Searching for books with Title: '{Title}' and Genre: '{Genre}'", title, genre);
            var books = await _bookRepository.SearchBooksAsync(title, genre);
            _logger.LogInformation("Search for books completed. {Count} books found.", books.Count());
            return books;
        }

        public async Task<string> RentBookAsync(int userId, int bookId)
        {
            _logger.LogInformation("User with UserId: {UserId} is attempting to rent a book with BookId: {BookId}", userId, bookId);

            var book = await _bookRepository.GetByIdAsync(bookId);
            if (book == null)
            {
                _logger.LogWarning("Book with BookId: {BookId} not found", bookId);
                throw new Exception("Book not found.");
            }

            if (book.IsRented)
            {
                _logger.LogInformation("Book with BookId: {BookId} is already rented. Adding UserId: {UserId} to the waiting list.", bookId, userId);
                await AddToWaitingListAsync(userId, bookId);
                return "Book is currently unavailable and has been added to the waiting list.";
            }

            book.IsRented = true;
            await _bookRepository.UpdateAsync(book);
            _logger.LogInformation("Book with BookId: {BookId} has been marked as rented.", bookId);

            var rental = new Rental
            {
                UserId = userId,
                BookId = bookId,
                RentedOn = DateTime.UtcNow,
                DueDate = DateTime.UtcNow.AddDays(14) // 2 weeks by default
            };

            await _rentalRepository.AddAsync(rental);
            await DeleteWaitingListAsync(userId, bookId);
            _logger.LogInformation("Book with BookId: {BookId} successfully rented by UserId: {UserId}.", bookId, userId);

            return "Book rented successfully.";
        }

        public async Task ReturnBookAsync(int rentalId)
        {
            _logger.LogInformation("Attempting to return book for RentalId: {RentalId}", rentalId);

            var rental = await _rentalRepository.GetByIdAsync(rentalId);
            if (rental == null || rental.ReturnedOn.HasValue)
            {
                _logger.LogWarning("Invalid rental record or the book is already returned for RentalId: {RentalId}", rentalId);
                throw new Exception("Invalid rental record.");
            }

            rental.ReturnedOn = DateTime.UtcNow;
            await _rentalRepository.UpdateAsync(rental);
            _logger.LogInformation("Rental record with RentalId: {RentalId} updated with return date.", rentalId);

            var book = await _bookRepository.GetByIdAsync(rental.BookId);
            book.IsRented = false;
            await _bookRepository.UpdateAsync(book);
            _logger.LogInformation("Book with BookId: {BookId} has been marked as available.", book.BookId);

            // Notify the next user in the waiting list, if any
            await NotifyNextInLineAsync(book.BookId);
        }

        public async Task ExtendDueDateAsync(int rentalId, int days)
        {
            _logger.LogInformation("Attempting to extend due date for RentalId: {RentalId} by {Days} days", rentalId, days);

            var rental = await _rentalRepository.GetByIdAsync(rentalId);
            if (rental == null)
            {
                _logger.LogWarning("Rental with RentalId: {RentalId} not found", rentalId);
                throw new Exception("Rental not found.");
            }

            if (rental.ExtensionCount >= MaxExtensions)
            {
                _logger.LogWarning("Maximum extension limit reached for RentalId: {RentalId}", rentalId);
                throw new Exception("Maximum extension limit reached.");
            }

            rental.DueDate = rental.DueDate.AddDays(days);
            rental.ExtensionCount++;
            await _rentalRepository.UpdateAsync(rental);
            _logger.LogInformation("Due date extended by {Days} days for RentalId: {RentalId}", days, rentalId);
        }

        public async Task DeleteWaitingListAsync(int userId, int bookId)
        {
            _logger.LogInformation("Attempting to delete waiting list entry for UserId: {UserId} and BookId: {BookId}", userId, bookId);

            var waitingListEntries = await _waitingListRepository.GetAllAsync();
            var userWaitingList = waitingListEntries.Where(w => w.BookId == bookId && w.UserId == userId)
                                                   .OrderBy(w => w.RequestedOn)
                                                   .FirstOrDefault();

            if (userWaitingList != null)
            {
                await _waitingListRepository.DeleteAsync(userWaitingList.WaitingListId);
                _logger.LogInformation("Waiting list entry deleted for UserId: {UserId} and BookId: {BookId}", userId, bookId);
            }
            else
            {
                _logger.LogInformation("No waiting list entry found for UserId: {UserId} and BookId: {BookId}", userId, bookId);
            }
        }

        public async Task AddToWaitingListAsync(int userId, int bookId)
        {
            _logger.LogInformation("Adding UserId: {UserId} to the waiting list for BookId: {BookId}", userId, bookId);

            var waitingListEntry = new WaitingList
            {
                UserId = userId,
                BookId = bookId,
                RequestedOn = DateTime.UtcNow
            };

            await _waitingListRepository.AddAsync(waitingListEntry);
            _logger.LogInformation("UserId: {UserId} added to the waiting list for BookId: {BookId}", userId, bookId);
        }

        public async Task NotifyNextInLineAsync(int bookId)
        {
            _logger.LogInformation("Notifying the next user in the waiting list for BookId: {BookId}", bookId);

            // Await the async call to get the waiting list entries
            var waitingListEntries = await _waitingListRepository.GetAllAsync();

            if (waitingListEntries == null || !waitingListEntries.Any())
            {
                _logger.LogInformation("No entries found in the waiting list for BookId: {BookId}", bookId);
                return;
            }

            // Now, filter the entries by the specified bookId
            var nextInLine = waitingListEntries.Where(w => w.BookId == bookId)
                                               .OrderBy(w => w.RequestedOn)
                                               .FirstOrDefault();

            if (nextInLine != null && nextInLine.Book != null && nextInLine.User != null)
            {
                await _emailService.SendReservationNotificationAsync(nextInLine.User.Name, nextInLine.Book.Title);
                await _waitingListRepository.DeleteAsync(nextInLine.WaitingListId);
                _logger.LogInformation("Notification sent to UserId: {UserId} for available BookId: {BookId}", nextInLine.User.UserId, bookId);
            }
        }

        public async Task<IEnumerable<Rental>> GetUserRentalHistoryAsync(int userId)
        {
            _logger.LogInformation("Fetching rental history for UserId: {UserId}", userId);

            var rentals = await _rentalRepository.GetAllAsync();
            var userRentals = rentals.Where(r => r.UserId == userId).ToList();

            _logger.LogInformation("Rental history retrieved for UserId: {UserId}. {Count} rentals found.", userId, userRentals.Count);
            return userRentals;
        }

        public async Task<IEnumerable<Rental>> GetOverdueRentalsAsync()
        {
            _logger.LogInformation("Fetching all overdue rentals.");

            var overdueRentals = await _rentRepository.GetOverdueRentalsAsync();
            _logger.LogInformation("{Count} overdue rentals found.", overdueRentals.Count());

            return overdueRentals;
        }
    }
}
