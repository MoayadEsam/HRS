using HotelReservation.Core.Models;
using HotelReservation.Data.Context;
using HotelReservation.Data.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace HotelReservation.Data.Repositories;

public class HotelRepository : GenericRepository<Hotel>, IHotelRepository
{
    public HotelRepository(ApplicationDbContext context) : base(context)
    {
    }

    public async Task<IEnumerable<Hotel>> GetHotelsWithRoomsAsync()
    {
        return await _dbSet
            .Include(h => h.Rooms)
            .ToListAsync();
    }

    public async Task<Hotel?> GetHotelWithRoomsAsync(int id)
    {
        return await _dbSet
            .Include(h => h.Rooms)
            .FirstOrDefaultAsync(h => h.Id == id);
    }

    public async Task<IEnumerable<Hotel>> GetActiveHotelsAsync()
    {
        return await _dbSet
            .Where(h => h.IsActive)
            .ToListAsync();
    }
}
