using HotelReservation.Core.Enums;
using HotelReservation.Core.Models;
using HotelReservation.Data.Context;
using HotelReservation.Data.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace HotelReservation.Data.Repositories;

public class ReservationRepository : GenericRepository<Reservation>, IReservationRepository
{
    public ReservationRepository(ApplicationDbContext context) : base(context)
    {
    }

    public async Task<Reservation?> GetReservationWithDetailsAsync(int id)
    {
        return await _dbSet
            .Include(r => r.User)
            .Include(r => r.Room)
                .ThenInclude(rm => rm.Hotel)
            .FirstOrDefaultAsync(r => r.Id == id);
    }

    public async Task<IEnumerable<Reservation>> GetReservationsByUserAsync(string userId)
    {
        return await _dbSet
            .Include(r => r.Room)
                .ThenInclude(rm => rm.Hotel)
            .Where(r => r.UserId == userId)
            .OrderByDescending(r => r.CreatedAt)
            .ToListAsync();
    }

    public async Task<IEnumerable<Reservation>> GetReservationsByRoomAsync(int roomId)
    {
        return await _dbSet
            .Include(r => r.User)
            .Where(r => r.RoomId == roomId)
            .OrderByDescending(r => r.CheckInDate)
            .ToListAsync();
    }

    public async Task<IEnumerable<Reservation>> GetReservationsByDateRangeAsync(DateTime startDate, DateTime endDate)
    {
        return await _dbSet
            .Include(r => r.User)
            .Include(r => r.Room)
                .ThenInclude(rm => rm.Hotel)
            .Where(r => r.CheckInDate <= endDate && r.CheckOutDate >= startDate)
            .OrderBy(r => r.CheckInDate)
            .ToListAsync();
    }

    public async Task<IEnumerable<Reservation>> GetUpcomingReservationsAsync()
    {
        var today = DateTime.Today;
        return await _dbSet
            .Include(r => r.User)
            .Include(r => r.Room)
                .ThenInclude(rm => rm.Hotel)
            .Where(r => r.CheckInDate >= today && r.Status != ReservationStatus.Cancelled)
            .OrderBy(r => r.CheckInDate)
            .ToListAsync();
    }

    public async Task<bool> HasOverlappingReservationsAsync(int roomId, DateTime checkIn, DateTime checkOut, int? excludeReservationId = null)
    {
        var query = _dbSet
            .Where(r => r.RoomId == roomId &&
                       r.Status != ReservationStatus.Cancelled &&
                       !(r.CheckOutDate <= checkIn || r.CheckInDate >= checkOut));

        if (excludeReservationId.HasValue)
        {
            query = query.Where(r => r.Id != excludeReservationId.Value);
        }

        return await query.AnyAsync();
    }
}
