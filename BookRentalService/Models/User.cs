namespace BookRentalService.Models
{
    public class User
    {
        public int UserId { get; set; }
        public string? Name { get; set; }
        public string? Email { get; set; }
        public ICollection<Rental>? Rentals { get; set; }
    }
}
