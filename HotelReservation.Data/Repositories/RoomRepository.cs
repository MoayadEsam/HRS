using HotelReservation.Core.DTOs;
using HotelReservation.Core.Enums;
using HotelReservation.Core.Models;
using HotelReservation.Data.Context;
using HotelReservation.Data.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace HotelReservation.Data.Repositories;

public class RoomRepository : GenericRepository<Room>, IRoomRepository
{
    public RoomRepository(ApplicationDbContext context) : base(context)
    {
    }

    public async Task<Room?> GetRoomWithDetailsAsync(int id)
    {
        return await _dbSet
            .Include(r => r.Hotel)
            .Include(r => r.RoomAmenities)
                .ThenInclude(ra => ra.Amenity)
            .FirstOrDefaultAsync(r => r.Id == id);
    }

    public async Task<IEnumerable<Room>> GetRoomsByHotelAsync(int hotelId)
    {
        return await _dbSet
            .Include(r => r.Hotel)
            .Where(r => r.HotelId == hotelId)
            .ToListAsync();
    }

    public async Task<IEnumerable<Room>> GetAvailableRoomsAsync(DateTime checkIn, DateTime checkOut)
    {
        return await _dbSet
            .Include(r => r.Hotel)
            .Include(r => r.RoomAmenities)
                .ThenInclude(ra => ra.Amenity)
            .Where(r => r.IsAvailable && !r.Reservations.Any(res =>
                res.Status != ReservationStatus.Cancelled &&
                !(res.CheckOutDate <= checkIn || res.CheckInDate >= checkOut)))
            .ToListAsync();
    }

    public async Task<IEnumerable<Room>> SearchRoomsAsync(RoomSearchDto searchDto)
    {
        var query = _dbSet
            .Include(r => r.Hotel)
            .Include(r => r.RoomAmenities)
                .ThenInclude(ra => ra.Amenity)
            .AsQueryable();

        if (searchDto.HotelId.HasValue)
        {
            query = query.Where(r => r.HotelId == searchDto.HotelId.Value);
        }

        if (searchDto.Type.HasValue)
        {
            query = query.Where(r => r.Type == searchDto.Type.Value);
        }

        if (searchDto.MinPrice.HasValue)
        {
            query = query.Where(r => r.PricePerNight >= searchDto.MinPrice.Value);
        }

        if (searchDto.MaxPrice.HasValue)
        {
            query = query.Where(r => r.PricePerNight <= searchDto.MaxPrice.Value);
        }

        if (searchDto.MinCapacity.HasValue)
        {
            query = query.Where(r => r.Capacity >= searchDto.MinCapacity.Value);
        }

        if (searchDto.AmenityIds != null && searchDto.AmenityIds.Any())
        {
            query = query.Where(r => r.RoomAmenities.Any(ra => searchDto.AmenityIds.Contains(ra.AmenityId)));
        }

        // Check availability if dates are provided
        if (searchDto.CheckInDate.HasValue && searchDto.CheckOutDate.HasValue)
        {
            var checkIn = searchDto.CheckInDate.Value;
            var checkOut = searchDto.CheckOutDate.Value;

            query = query.Where(r => r.IsAvailable && !r.Reservations.Any(res =>
                res.Status != ReservationStatus.Cancelled &&
                !(res.CheckOutDate <= checkIn || res.CheckInDate >= checkOut)));
        }

        return await query.ToListAsync();
    }

    public async Task<bool> IsRoomAvailableAsync(int roomId, DateTime checkIn, DateTime checkOut, int? excludeReservationId = null)
    {
        var room = await _dbSet
            .Include(r => r.Reservations)
            .FirstOrDefaultAsync(r => r.Id == roomId);

        if (room == null || !room.IsAvailable)
            return false;

        var overlappingReservations = room.Reservations
            .Where(res => res.Status != ReservationStatus.Cancelled &&
                         !(res.CheckOutDate <= checkIn || res.CheckInDate >= checkOut));

        if (excludeReservationId.HasValue)
        {
            overlappingReservations = overlappingReservations.Where(res => res.Id != excludeReservationId.Value);
        }

        return !overlappingReservations.Any();
    }
}
