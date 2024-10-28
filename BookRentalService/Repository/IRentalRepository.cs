using BookRentalService.Models;

namespace BookRentalService.Repository
{
    public interface IRentalRepository : IRepository<Rental>
    {
        Task<IEnumerable<Rental>> GetOverdueRentalsAsync();
    }
}
