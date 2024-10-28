namespace BookRentalService.Models
{
    public class Notification
    {
        public int NotificationId { get; set; }
        public int RentalId { get; set; }
        public string? Message { get; set; }
        public DateTime SentOn { get; set; }
        public Rental? Rental { get; set; }
    }
}
