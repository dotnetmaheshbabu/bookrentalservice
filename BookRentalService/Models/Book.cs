﻿namespace BookRentalService.Models
{
    public class Book
    {
        public int BookId { get; set; }
        public string? Title { get; set; }
        public string? Author { get; set; }
        public string? ISBN { get; set; }
        public string? Genre { get; set; }
        public bool IsRented { get; set; }
        public ICollection<Rental>? Rentals { get; set; }
    }
}
