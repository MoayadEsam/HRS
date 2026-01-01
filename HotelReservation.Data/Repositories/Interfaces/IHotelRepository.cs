using HotelReservation.Core.Models;

namespace HotelReservation.Data.Repositories.Interfaces;

public interface IHotelRepository : IGenericRepository<Hotel>
{
    Task<IEnumerable<Hotel>> GetHotelsWithRoomsAsync();
    Task<Hotel?> GetHotelWithRoomsAsync(int id);
    Task<IEnumerable<Hotel>> GetActiveHotelsAsync();
    void RemoveImage(HotelImage image);
}
