using HotelReservation.Core.DTOs;

namespace HotelReservation.Services.Interfaces;

public interface IHotelService
{
    Task<IEnumerable<HotelListDto>> GetAllHotelsAsync();
    Task<IEnumerable<HotelListDto>> GetActiveHotelsAsync();
    Task<HotelDetailsDto?> GetHotelByIdAsync(int id);
    Task<int> CreateHotelAsync(HotelCreateDto dto);
    Task<bool> UpdateHotelAsync(HotelUpdateDto dto);
    Task<bool> DeleteHotelAsync(int id);
}

public interface IRoomService
{
    Task<IEnumerable<RoomListDto>> GetAllRoomsAsync();
    Task<IEnumerable<RoomListDto>> GetRoomsByHotelAsync(int hotelId);
    Task<RoomDetailsDto?> GetRoomByIdAsync(int id);
    Task<IEnumerable<RoomListDto>> SearchRoomsAsync(RoomSearchDto searchDto);
    Task<int> CreateRoomAsync(RoomCreateDto dto);
    Task<bool> UpdateRoomAsync(RoomUpdateDto dto);
    Task<bool> DeleteRoomAsync(int id);
    Task<bool> IsRoomAvailableAsync(int roomId, DateTime checkIn, DateTime checkOut, int? excludeReservationId = null);
}

public interface IAmenityService
{
    Task<IEnumerable<AmenityListDto>> GetAllAmenitiesAsync();
    Task<IEnumerable<AmenityListDto>> GetActiveAmenitiesAsync();
    Task<AmenityListDto?> GetAmenityByIdAsync(int id);
    Task<int> CreateAmenityAsync(AmenityCreateDto dto);
    Task<bool> UpdateAmenityAsync(AmenityUpdateDto dto);
    Task<bool> DeleteAmenityAsync(int id);
}

public interface IReservationService
{
    Task<IEnumerable<ReservationListDto>> GetAllReservationsAsync();
    Task<IEnumerable<ReservationListDto>> GetReservationsByUserAsync(string userId);
    Task<ReservationDetailsDto?> GetReservationByIdAsync(int id);
    Task<int> CreateReservationAsync(ReservationCreateDto dto, string userId);
    Task<bool> UpdateReservationAsync(ReservationUpdateDto dto);
    Task<bool> CancelReservationAsync(int id);
    Task<bool> DeleteReservationAsync(int id);
    Task<decimal> CalculateTotalPriceAsync(int roomId, DateTime checkIn, DateTime checkOut);
}
