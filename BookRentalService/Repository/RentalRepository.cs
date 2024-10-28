using BookRentalService.DbContextHelper;
using BookRentalService.Models;
using Microsoft.EntityFrameworkCore;

namespace BookRentalService.Repository
{
    public class RentalRepository : Repository<Rental>, IRentalRepository
    {
        public RentalRepository(AppDbContext context) : base(context) { }

        public async Task<IEnumerable<Rental>> GetOverdueRentalsAsync()
        {
            // Retrieve overdue rentals
            return await _dbSet
                .Where(r => r.IsOverdue || (r.DueDate < DateTime.UtcNow && !r.ReturnedOn.HasValue))
                .ToListAsync();
        }
    }

}
