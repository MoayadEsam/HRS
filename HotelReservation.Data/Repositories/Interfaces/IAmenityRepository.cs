using HotelReservation.Core.Models;

namespace HotelReservation.Data.Repositories.Interfaces;

public interface IAmenityRepository : IGenericRepository<Amenity>
{
    Task<IEnumerable<Amenity>> GetActiveAmenitiesAsync();
    Task<IEnumerable<Amenity>> GetAmenitiesByRoomIdAsync(int roomId);
}
