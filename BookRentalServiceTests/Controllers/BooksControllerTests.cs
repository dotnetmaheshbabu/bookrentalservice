using Xunit;
using Moq;
using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using BookRentalService.Controllers;
using BookRentalService.Services;
using BookRentalService.Models;
using System.Collections.Generic;

namespace BookRentalService.Tests.Controllers
{
    public class BooksControllerTests
    {
        private readonly Mock<IBookService> _bookServiceMock;
        private readonly Mock<ILogger<BooksController>> _loggerMock;
        private readonly BooksController _booksController;

        public BooksControllerTests()
        {
            // Setup Mocks
            _bookServiceMock = new Mock<IBookService>();
            _loggerMock = new Mock<ILogger<BooksController>>();

            // Initialize the controller with mocked dependencies
            _booksController = new BooksController(_bookServiceMock.Object, _loggerMock.Object);
        }

        [Fact]
        public async Task SearchBooks_ShouldReturnOkResult_WhenBooksAreFound()
        {
            // Arrange
            var title = "Sample Title";
            var genre = "Sample Genre";
            var sampleBooks = new List<Book>
            {
                new Book { BookId = 1, Title = "Sample Book 1", Genre = genre },
                new Book { BookId = 2, Title = "Sample Book 2", Genre = genre }
            };

            _bookServiceMock.Setup(service => service.SearchBooksAsync(title, genre))
                            .ReturnsAsync(sampleBooks);

            // Act
            var result = await _booksController.SearchBooks(title, genre);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var returnedBooks = Assert.IsType<List<Book>>(okResult.Value);
            Assert.Equal(2, returnedBooks.Count);
            _loggerMock.Verify(log => log.LogInformation(It.IsAny<string>(), title, genre), Times.Once);
        }

        [Fact]
        public async Task SearchBooks_ShouldReturnServerError_WhenExceptionIsThrown()
        {
            // Arrange
            var title = "Sample Title";
            var genre = "Sample Genre";

            _bookServiceMock.Setup(service => service.SearchBooksAsync(title, genre))
                            .ThrowsAsync(new Exception("Error occurred"));

            // Act
            var result = await _booksController.SearchBooks(title, genre);

            // Assert
            var serverErrorResult = Assert.IsType<ObjectResult>(result);
            Assert.Equal(StatusCodes.Status500InternalServerError, serverErrorResult.StatusCode);
            _loggerMock.Verify(log => log.LogError(It.IsAny<Exception>(), It.IsAny<string>(), title, genre), Times.Once);
        }

        [Fact]
        public async Task RentBook_ShouldReturnOkResult_WhenBookIsRentedSuccessfully()
        {
            // Arrange
            var request = new RentRequest { UserId = 1, BookId = 1 };
            var expectedMessage = "Book rented successfully.";

            _bookServiceMock.Setup(service => service.RentBookAsync(request.UserId, request.BookId))
                            .ReturnsAsync(expectedMessage);

            // Act
            var result = await _booksController.RentBook(request);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var responseMessage = Assert.IsType<dynamic>(okResult.Value);
            Assert.Equal(expectedMessage, responseMessage.Message);
            _loggerMock.Verify(log => log.LogInformation(It.IsAny<string>(), request.UserId, request.BookId), Times.Once);
        }

        [Fact]
        public async Task RentBook_ShouldReturnBadRequest_WhenExceptionIsThrown()
        {
            // Arrange
            var request = new RentRequest { UserId = 1, BookId = 1 };
            var errorMessage = "Book not available.";

            _bookServiceMock.Setup(service => service.RentBookAsync(request.UserId, request.BookId))
                            .ThrowsAsync(new Exception(errorMessage));

            // Act
            var result = await _booksController.RentBook(request);

            // Assert
            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
            var responseMessage = Assert.IsType<dynamic>(badRequestResult.Value);
            Assert.Equal(errorMessage, responseMessage.Message);
            _loggerMock.Verify(log => log.LogError(It.IsAny<Exception>(), It.IsAny<string>(), request.UserId, request.BookId), Times.Once);
        }

        [Fact]
        public async Task ReturnBook_ShouldReturnOkResult_WhenBookIsReturnedSuccessfully()
        {
            // Arrange
            var rentalId = 1;

            _bookServiceMock.Setup(service => service.ReturnBookAsync(rentalId))
                            .Returns(Task.CompletedTask);

            // Act
            var result = await _booksController.ReturnBook(rentalId);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var responseMessage = Assert.IsType<dynamic>(okResult.Value);
            Assert.Equal("Book returned successfully.", responseMessage.Message);
            _loggerMock.Verify(log => log.LogInformation(It.IsAny<string>(), rentalId), Times.Once);
        }

        [Fact]
        public async Task ExtendDueDate_ShouldReturnOkResult_WhenDueDateIsExtendedSuccessfully()
        {
            // Arrange
            var rentalId = 1;
            var days = 7;

            _bookServiceMock.Setup(service => service.ExtendDueDateAsync(rentalId, days))
                            .Returns(Task.CompletedTask);

            // Act
            var result = await _booksController.ExtendDueDate(rentalId, days);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var responseMessage = Assert.IsType<dynamic>(okResult.Value);
            Assert.Equal("Due date extended successfully.", responseMessage.Message);
            _loggerMock.Verify(log => log.LogInformation(It.IsAny<string>(), rentalId, days), Times.Once);
        }

        [Fact]
        public async Task AddToWaitingList_ShouldReturnOkResult_WhenAddedSuccessfully()
        {
            // Arrange
            var request = new WaitingListRequest { UserId = 1, BookId = 1 };

            _bookServiceMock.Setup(service => service.AddToWaitingListAsync(request.UserId, request.BookId))
                            .Returns(Task.CompletedTask);

            // Act
            var result = await _booksController.AddToWaitingList(request);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var responseMessage = Assert.IsType<dynamic>(okResult.Value);
            Assert.Equal("Added to waiting list successfully.", responseMessage.Message);
            _loggerMock.Verify(log => log.LogInformation(It.IsAny<string>(), request.UserId, request.BookId), Times.Once);
        }
    }
}
