using AutoMapper;
using HotelReservation.Core.DTOs;
using HotelReservation.Core.Enums;
using HotelReservation.Core.Models;
using HotelReservation.Data.Repositories.Interfaces;
using HotelReservation.Services.Interfaces;

namespace HotelReservation.Services;

public class ReservationService : IReservationService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public ReservationService(IUnitOfWork unitOfWork, IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    public async Task<IEnumerable<ReservationListDto>> GetAllReservationsAsync()
    {
        var reservations = await _unitOfWork.Reservations.GetUpcomingReservationsAsync();
        return _mapper.Map<IEnumerable<ReservationListDto>>(reservations);
    }

    public async Task<IEnumerable<ReservationListDto>> GetReservationsByUserAsync(string userId)
    {
        var reservations = await _unitOfWork.Reservations.GetReservationsByUserAsync(userId);
        return _mapper.Map<IEnumerable<ReservationListDto>>(reservations);
    }

    public async Task<ReservationDetailsDto?> GetReservationByIdAsync(int id)
    {
        var reservation = await _unitOfWork.Reservations.GetReservationWithDetailsAsync(id);
        if (reservation == null)
            return null;

        return _mapper.Map<ReservationDetailsDto>(reservation);
    }

    public async Task<int> CreateReservationAsync(ReservationCreateDto dto, string userId)
    {
        // Validate dates
        if (dto.CheckInDate >= dto.CheckOutDate)
        {
            throw new ArgumentException("Check-out date must be after check-in date");
        }

        if (dto.CheckInDate < DateTime.Today)
        {
            throw new ArgumentException("Check-in date cannot be in the past");
        }

        // Check room availability
        var isAvailable = await _unitOfWork.Rooms.IsRoomAvailableAsync(
            dto.RoomId, dto.CheckInDate, dto.CheckOutDate);

        if (!isAvailable)
        {
            throw new InvalidOperationException("Room is not available for the selected dates");
        }

        // Get room to calculate price
        var room = await _unitOfWork.Rooms.GetByIdAsync(dto.RoomId);
        if (room == null)
        {
            throw new ArgumentException("Room not found");
        }

        // Validate guest capacity
        if (dto.NumberOfGuests > room.Capacity)
        {
            throw new ArgumentException($"Room capacity is {room.Capacity} guests");
        }

        // Calculate total price
        var totalPrice = await CalculateTotalPriceAsync(dto.RoomId, dto.CheckInDate, dto.CheckOutDate);

        // Create reservation
        var reservation = _mapper.Map<Reservation>(dto);
        reservation.UserId = userId;
        reservation.TotalPrice = totalPrice;
        reservation.Status = ReservationStatus.Confirmed;
        reservation.CreatedAt = DateTime.UtcNow;

        await _unitOfWork.Reservations.AddAsync(reservation);
        await _unitOfWork.SaveChangesAsync();

        return reservation.Id;
    }

    public async Task<bool> UpdateReservationAsync(ReservationUpdateDto dto)
    {
        var reservation = await _unitOfWork.Reservations.GetByIdAsync(dto.Id);
        if (reservation == null)
            return false;

        // Validate dates
        if (dto.CheckInDate >= dto.CheckOutDate)
        {
            throw new ArgumentException("Check-out date must be after check-in date");
        }

        // Check availability if dates changed
        if (reservation.CheckInDate != dto.CheckInDate || reservation.CheckOutDate != dto.CheckOutDate)
        {
            var isAvailable = await _unitOfWork.Rooms.IsRoomAvailableAsync(
                reservation.RoomId, dto.CheckInDate, dto.CheckOutDate, dto.Id);

            if (!isAvailable)
            {
                throw new InvalidOperationException("Room is not available for the selected dates");
            }

            // Recalculate price if dates changed
            reservation.TotalPrice = await CalculateTotalPriceAsync(
                reservation.RoomId, dto.CheckInDate, dto.CheckOutDate);
        }

        _mapper.Map(dto, reservation);
        reservation.UpdatedAt = DateTime.UtcNow;
        _unitOfWork.Reservations.Update(reservation);
        await _unitOfWork.SaveChangesAsync();

        return true;
    }

    public async Task<bool> CancelReservationAsync(int id)
    {
        var reservation = await _unitOfWork.Reservations.GetByIdAsync(id);
        if (reservation == null)
            return false;

        reservation.Status = ReservationStatus.Cancelled;
        reservation.UpdatedAt = DateTime.UtcNow;
        _unitOfWork.Reservations.Update(reservation);
        await _unitOfWork.SaveChangesAsync();

        return true;
    }

    public async Task<bool> DeleteReservationAsync(int id)
    {
        var reservation = await _unitOfWork.Reservations.GetByIdAsync(id);
        if (reservation == null)
            return false;

        _unitOfWork.Reservations.Remove(reservation);
        await _unitOfWork.SaveChangesAsync();
        return true;
    }

    public async Task<decimal> CalculateTotalPriceAsync(int roomId, DateTime checkIn, DateTime checkOut)
    {
        var room = await _unitOfWork.Rooms.GetByIdAsync(roomId);
        if (room == null)
        {
            throw new ArgumentException("Room not found");
        }

        var numberOfNights = (checkOut - checkIn).Days;
        if (numberOfNights <= 0)
        {
            throw new ArgumentException("Invalid date range");
        }

        return room.PricePerNight * numberOfNights;
    }
}
