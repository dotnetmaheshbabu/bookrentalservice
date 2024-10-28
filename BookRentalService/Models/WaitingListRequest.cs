namespace BookRentalService.Models
{
    public class WaitingListRequest
    {
        public int UserId { get; set; }
        public int BookId { get; set; }
    }
}
