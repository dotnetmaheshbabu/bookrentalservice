namespace BookRentalService.Models
{
    public class WaitingList
    {
        public int WaitingListId { get; set; }
        public int BookId { get; set; }
        public int UserId { get; set; }
        public DateTime RequestedOn { get; set; }
        public Book? Book { get; set; }
        public User? User { get; set; }
    }
}
