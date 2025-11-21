using HotelReservation.Core.Models;
using HotelReservation.Data.Context;
using HotelReservation.Data.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace HotelReservation.Data.Repositories;

public class AmenityRepository : GenericRepository<Amenity>, IAmenityRepository
{
    public AmenityRepository(ApplicationDbContext context) : base(context)
    {
    }

    public async Task<IEnumerable<Amenity>> GetActiveAmenitiesAsync()
    {
        return await _dbSet
            .Where(a => a.IsActive)
            .ToListAsync();
    }

    public async Task<IEnumerable<Amenity>> GetAmenitiesByRoomIdAsync(int roomId)
    {
        return await _context.RoomAmenities
            .Where(ra => ra.RoomId == roomId)
            .Select(ra => ra.Amenity)
            .ToListAsync();
    }
}
