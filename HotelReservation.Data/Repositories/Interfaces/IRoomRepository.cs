using HotelReservation.Core.DTOs;
using HotelReservation.Core.Models;

namespace HotelReservation.Data.Repositories.Interfaces;

public interface IRoomRepository : IGenericRepository<Room>
{
    Task<Room?> GetRoomWithDetailsAsync(int id);
    Task<IEnumerable<Room>> GetRoomsByHotelAsync(int hotelId);
    Task<IEnumerable<Room>> GetAvailableRoomsAsync(DateTime checkIn, DateTime checkOut);
    Task<IEnumerable<Room>> SearchRoomsAsync(RoomSearchDto searchDto);
    Task<bool> IsRoomAvailableAsync(int roomId, DateTime checkIn, DateTime checkOut, int? excludeReservationId = null);
}
