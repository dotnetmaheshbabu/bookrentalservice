using Xunit;
using Moq;
using System.Threading.Tasks;
using BookRentalService.Services;
using BookRentalService.Repository;
using BookRentalService.Models;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using SendGrid;
using SendGrid.Helpers.Mail;
using System.Collections.Generic;

namespace BookRentalService.Tests.Services
{
    public class EmailServiceTests
    {
        private readonly Mock<IRentalRepository> _rentalRepositoryMock;
        private readonly Mock<ILogger<EmailService>> _loggerMock;
        private readonly Mock<IOptions<SendGridOptions>> _sendGridOptionsMock;
        private readonly Mock<SendGridClient> _sendGridClientMock;
        private readonly EmailService _emailService;

        public EmailServiceTests()
        {
            // Setup Mocks
            _rentalRepositoryMock = new Mock<IRentalRepository>();
            _loggerMock = new Mock<ILogger<EmailService>>();
            _sendGridOptionsMock = new Mock<IOptions<SendGridOptions>>();
            _sendGridClientMock = new Mock<SendGridClient>("your-fake-api-key"); // Mocked instance with fake API key

            // Setup SendGridOptions
            _sendGridOptionsMock.Setup(x => x.Value)
                .Returns(new SendGridOptions { ApiKey = "your-fake-api-key" });

            // Initialize the EmailService with mocked dependencies
            _emailService = new EmailService(_sendGridOptionsMock.Object, _loggerMock.Object, _rentalRepositoryMock.Object);
        }

        [Fact]
        public async Task SendOverdueNotificationAsync_Should_Log_Info_When_Email_Sent()
        {
            // Arrange
            var toEmail = "test@example.com";
            var bookTitle = "Test Book";

            // Act
            await _emailService.SendOverdueNotificationAsync(toEmail, bookTitle);

            // Assert
            _loggerMock.Verify(
                x => x.Log(
                    LogLevel.Information,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((o, t) => o.ToString().Contains("Overdue notification sent")),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.Once);
        }

        [Fact]
        public async Task SendOverdueNotificationAsync_Should_Log_Error_When_Exception_Thrown()
        {
            // Arrange
            var toEmail = "test@example.com";
            var bookTitle = "Test Book";

            // Force an exception by setting up an invalid SendGridClient behavior
            _sendGridOptionsMock.Setup(x => x.Value.ApiKey).Throws(new System.Exception("SendGrid Error"));

            // Act
            await _emailService.SendOverdueNotificationAsync(toEmail, bookTitle);

            // Assert
            _loggerMock.Verify(
                x => x.Log(
                    LogLevel.Error,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((o, t) => o.ToString().Contains("Failed to send overdue notification")),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.Once);
        }

        [Fact]
        public async Task SendOverdueNotificationsAsync_Should_Send_Notifications_For_Overdue_Books()
        {
            // Arrange
            var overdueRentals = new List<Rental>
            {
                new Rental { User = new User { Email = "user1@example.com" }, Book = new Book { Title = "Book 1" } },
                new Rental { User = new User { Email = "user2@example.com" }, Book = new Book { Title = "Book 2" } }
            };

            // Mock the overdue rentals retrieval
            _rentalRepositoryMock.Setup(repo => repo.GetOverdueRentalsAsync())
                                 .ReturnsAsync(overdueRentals);

            // Act
            await _emailService.SendOverdueNotificationsAsync();

            // Assert
            _loggerMock.Verify(
                x => x.Log(
                    LogLevel.Information,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((o, t) => o.ToString().Contains("Sending overdue notification")),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.Exactly(overdueRentals.Count));

            _loggerMock.Verify(
                x => x.Log(
                    LogLevel.Information,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((o, t) => o.ToString().Contains("Sent overdue notification")),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.Exactly(overdueRentals.Count));
        }
    }
}
