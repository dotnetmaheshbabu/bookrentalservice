namespace BookRentalService.Services
{
    public interface IEmailService
    {
        Task SendOverdueNotificationAsync(string toEmail, string bookTitle);
        Task SendReservationNotificationAsync(string toEmail, string bookTitle);
        Task SendOverdueNotificationsAsync();
    }
}
