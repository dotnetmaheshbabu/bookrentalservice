using Microsoft.Extensions.Options;
using SendGrid;
using SendGrid.Helpers.Mail;
using System.Threading.Tasks;
using BookRentalService.Models;
using BookRentalService.Repository;
using Microsoft.Extensions.Logging;

namespace BookRentalService.Services
{
    public class EmailService : IEmailService
    {
        private readonly SendGridClient _client;
        private readonly ILogger<EmailService> _logger;
        private readonly IRentalRepository _rentalRepository;

        public EmailService(IOptions<SendGridOptions> options, ILogger<EmailService> logger, IRentalRepository rentalRepository)
        {
            _client = new SendGridClient(options.Value.ApiKey);
            _logger = logger;
            _rentalRepository = rentalRepository;
        }

        public async Task SendOverdueNotificationAsync(string toEmail, string bookTitle)
        {
            _logger.LogInformation("Preparing to send overdue notification to {Email} for book '{BookTitle}'", toEmail, bookTitle);
            try
            {
                var msg = new SendGridMessage()
                {
                    From = new EmailAddress("noreply@bookrental.com", "Book Rental Service"),
                    Subject = "Overdue Book Notification",
                    PlainTextContent = $"Your book '{bookTitle}' is overdue. Please return it as soon as possible."
                };
                msg.AddTo(new EmailAddress(toEmail));

                // Log before sending the email
                _logger.LogInformation("Sending overdue notification to {Email}", toEmail);

                // Uncomment the line below to actually send the email
                // await _client.SendEmailAsync(msg);

                _logger.LogInformation("Overdue notification sent to {Email} for book '{BookTitle}'", toEmail, bookTitle);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to send overdue notification to {Email} for book '{BookTitle}'", toEmail, bookTitle);
            }
        }

        public async Task SendReservationNotificationAsync(string toEmail, string bookTitle)
        {
            _logger.LogInformation("Preparing to send reservation notification to {Email} for book '{BookTitle}'", toEmail, bookTitle);
            try
            {
                var msg = new SendGridMessage()
                {
                    From = new EmailAddress("noreply@bookrental.com", "Book Rental Service"),
                    Subject = "Book Available for Reservation",
                    PlainTextContent = $"The book '{bookTitle}' is now available for you to rent."
                };
                msg.AddTo(new EmailAddress(toEmail));

                // Log before sending the email
                _logger.LogInformation("Sending reservation notification to {Email}", toEmail);

                // Uncomment the line below to actually send the email
                // await _client.SendEmailAsync(msg);

                _logger.LogInformation("Reservation notification sent to {Email} for book '{BookTitle}'", toEmail, bookTitle);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to send reservation notification to {Email} for book '{BookTitle}'", toEmail, bookTitle);
            }
        }

        public async Task SendOverdueNotificationsAsync()
        {
            _logger.LogInformation("Starting the process of sending overdue notifications.");

            try
            {
                // Fetch all overdue rentals
                var overdueRentals = await _rentalRepository.GetOverdueRentalsAsync();

                foreach (var rental in overdueRentals)
                {
                    var userEmail = rental.User.Email;
                    var bookTitle = rental.Book.Title;

                    // Log before sending each notification
                    _logger.LogInformation("Sending overdue notification to {Email} for book '{BookTitle}'", userEmail, bookTitle);

                    // Uncomment to send the email
                    // await SendOverdueNotificationAsync(userEmail, bookTitle);

                    _logger.LogInformation("Sent overdue notification to {Email} for book '{BookTitle}'", userEmail, bookTitle);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while sending overdue notifications.");
            }
        }
    }
}
