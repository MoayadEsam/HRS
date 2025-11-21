using HotelReservation.Core.Models;

namespace HotelReservation.Data.Repositories.Interfaces;

public interface IReservationRepository : IGenericRepository<Reservation>
{
    Task<Reservation?> GetReservationWithDetailsAsync(int id);
    Task<IEnumerable<Reservation>> GetReservationsByUserAsync(string userId);
    Task<IEnumerable<Reservation>> GetReservationsByRoomAsync(int roomId);
    Task<IEnumerable<Reservation>> GetReservationsByDateRangeAsync(DateTime startDate, DateTime endDate);
    Task<IEnumerable<Reservation>> GetUpcomingReservationsAsync();
    Task<bool> HasOverlappingReservationsAsync(int roomId, DateTime checkIn, DateTime checkOut, int? excludeReservationId = null);
}
