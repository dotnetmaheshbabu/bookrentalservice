namespace BookRentalService.Models
{
    public class Rental
    {
        public int RentalId { get; set; }
        public int UserId { get; set; }
        public int BookId { get; set; }
        public DateTime RentedOn { get; set; }
        public DateTime? ReturnedOn { get; set; }
        public DateTime DueDate { get; set; }
        public bool IsOverdue { get; set; }
        public int ExtensionCount { get; set; } // Track extensions
        public User? User { get; set; }
        public Book? Book { get; set; }
    }
}
