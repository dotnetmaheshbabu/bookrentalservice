using Xunit;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BookRentalService.Models;
using BookRentalService.Services;
using BookRentalService.Repository;
using Microsoft.Extensions.Logging;

namespace BookRentalService.Tests.Services
{
    public class BookServiceTests
    {
        private readonly Mock<IBookRepository> _bookRepositoryMock;
        private readonly Mock<IRepository<Rental>> _rentalRepositoryMock;
        private readonly Mock<IRepository<WaitingList>> _waitingListRepositoryMock;
        private readonly Mock<IRentalRepository> _rentRepositoryMock;
        private readonly Mock<IEmailService> _emailServiceMock;
        private readonly Mock<ILogger<BookService>> _loggerMock;
        private readonly BookService _bookService;

        public BookServiceTests()
        {
            // Setup Mocks
            _bookRepositoryMock = new Mock<IBookRepository>();
            _rentalRepositoryMock = new Mock<IRepository<Rental>>();
            _waitingListRepositoryMock = new Mock<IRepository<WaitingList>>();
            _rentRepositoryMock = new Mock<IRentalRepository>();
            _emailServiceMock = new Mock<IEmailService>();
            _loggerMock = new Mock<ILogger<BookService>>();

            // Initialize the BookService with mocked dependencies
            _bookService = new BookService(
                _emailServiceMock.Object,
                _rentRepositoryMock.Object,
                _bookRepositoryMock.Object,
                _rentalRepositoryMock.Object,
                _waitingListRepositoryMock.Object,
                _loggerMock.Object
            );
        }

        [Fact]
        public async Task SearchBooksAsync_ShouldReturnListOfBooks_WhenBooksExist()
        {
            // Arrange
            var title = "Sample Title";
            var genre = "Sample Genre";
            var sampleBooks = new List<Book>
            {
                new Book { BookId = 1, Title = "Sample Book 1", Genre = genre, IsRented = false },
                new Book { BookId = 2, Title = "Sample Book 2", Genre = genre, IsRented = true }
            };

            _bookRepositoryMock.Setup(repo => repo.SearchBooksAsync(title, genre))
                               .ReturnsAsync(sampleBooks);

            // Act
            var result = await _bookService.SearchBooksAsync(title, genre);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(2, result.Count());
            _loggerMock.Verify(log => log.LogInformation("Searching for books with Title: '{Title}' and Genre: '{Genre}'", title, genre), Times.Once);
        }

        [Fact]
        public async Task RentBookAsync_ShouldThrowException_WhenBookNotFound()
        {
            // Arrange
            var userId = 1;
            var bookId = 1;
            _bookRepositoryMock.Setup(repo => repo.GetByIdAsync(bookId)).ReturnsAsync((Book)null);

            // Act & Assert
            var exception = await Assert.ThrowsAsync<Exception>(() => _bookService.RentBookAsync(userId, bookId));
            Assert.Equal("Book not found.", exception.Message);
            _loggerMock.Verify(log => log.LogWarning("Book with BookId: {BookId} not found", bookId), Times.Once);
        }

        [Fact]
        public async Task RentBookAsync_ShouldAddToWaitingList_WhenBookIsRented()
        {
            // Arrange
            var userId = 1;
            var bookId = 1;
            var book = new Book { BookId = bookId, IsRented = true };
            _bookRepositoryMock.Setup(repo => repo.GetByIdAsync(bookId)).ReturnsAsync(book);

            // Act
            var result = await _bookService.RentBookAsync(userId, bookId);

            // Assert
            Assert.Equal("Book is currently unavailable and has been added to the waiting list.", result);
            _waitingListRepositoryMock.Verify(repo => repo.AddAsync(It.IsAny<WaitingList>()), Times.Once);
        }

        [Fact]
        public async Task RentBookAsync_ShouldMarkBookAsRented_WhenAvailable()
        {
            // Arrange
            var userId = 1;
            var bookId = 1;
            var book = new Book { BookId = bookId, IsRented = false };
            _bookRepositoryMock.Setup(repo => repo.GetByIdAsync(bookId)).ReturnsAsync(book);

            // Act
            var result = await _bookService.RentBookAsync(userId, bookId);

            // Assert
            Assert.Equal("Book rented successfully.", result);
            Assert.True(book.IsRented);
            _bookRepositoryMock.Verify(repo => repo.UpdateAsync(book), Times.Once);
        }

        [Fact]
        public async Task ReturnBookAsync_ShouldMarkBookAsAvailable_WhenReturnedSuccessfully()
        {
            // Arrange
            var rentalId = 1;
            var rental = new Rental { RentalId = rentalId, BookId = 1, ReturnedOn = null };
            var book = new Book { BookId = 1, IsRented = true };

            _rentalRepositoryMock.Setup(repo => repo.GetByIdAsync(rentalId)).ReturnsAsync(rental);
            _bookRepositoryMock.Setup(repo => repo.GetByIdAsync(rental.BookId)).ReturnsAsync(book);

            // Act
            await _bookService.ReturnBookAsync(rentalId);

            // Assert
            Assert.False(book.IsRented);
            _bookRepositoryMock.Verify(repo => repo.UpdateAsync(book), Times.Once);
            _rentalRepositoryMock.Verify(repo => repo.UpdateAsync(rental), Times.Once);
        }

        [Fact]
        public async Task ExtendDueDateAsync_ShouldExtendSuccessfully_WhenConditionsMet()
        {
            // Arrange
            var rentalId = 1;
            var rental = new Rental { RentalId = rentalId, DueDate = DateTime.UtcNow, ExtensionCount = 0 };
            var daysToExtend = 7;

            _rentalRepositoryMock.Setup(repo => repo.GetByIdAsync(rentalId)).ReturnsAsync(rental);

            // Act
            await _bookService.ExtendDueDateAsync(rentalId, daysToExtend);

            // Assert
            Assert.Equal(1, rental.ExtensionCount);
            Assert.Equal(rental.DueDate.AddDays(daysToExtend), rental.DueDate);
            _rentalRepositoryMock.Verify(repo => repo.UpdateAsync(rental), Times.Once);
        }
    }
}
