using AutoMapper;
using HotelReservation.Core.DTOs;
using HotelReservation.Core.Models;
using HotelReservation.Data.Context;
using HotelReservation.Data.Repositories.Interfaces;
using HotelReservation.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace HotelReservation.Services;

public class HotelService : IHotelService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;
    private readonly ApplicationDbContext _context;

    public HotelService(IUnitOfWork unitOfWork, IMapper mapper, ApplicationDbContext context)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
        _context = context;
    }

    public async Task<IEnumerable<HotelListDto>> GetAllHotelsAsync()
    {
        var hotels = await _unitOfWork.Hotels.GetHotelsWithRoomsAsync();
        return _mapper.Map<IEnumerable<HotelListDto>>(hotels);
    }

    public async Task<IEnumerable<HotelListDto>> GetActiveHotelsAsync()
    {
        var hotels = await _unitOfWork.Hotels.GetActiveHotelsAsync();
        return _mapper.Map<IEnumerable<HotelListDto>>(hotels);
    }

    public async Task<HotelDetailsDto?> GetHotelByIdAsync(int id)
    {
        var hotel = await _unitOfWork.Hotels.GetHotelWithRoomsAsync(id);
        if (hotel == null)
            return null;

        return _mapper.Map<HotelDetailsDto>(hotel);
    }

    public async Task<int> CreateHotelAsync(HotelCreateDto dto)
    {
        var hotel = _mapper.Map<Hotel>(dto);
        await _unitOfWork.Hotels.AddAsync(hotel);
        await _unitOfWork.SaveChangesAsync();
        return hotel.Id;
    }

    public async Task<bool> UpdateHotelAsync(HotelUpdateDto dto)
    {
        var hotel = await _unitOfWork.Hotels.GetByIdAsync(dto.Id);
        if (hotel == null)
            return false;

        _mapper.Map(dto, hotel);
        _unitOfWork.Hotels.Update(hotel);
        await _unitOfWork.SaveChangesAsync();
        return true;
    }

    public async Task<bool> DeleteHotelAsync(int id)
    {
        var hotel = await _context.Hotels
            .Include(h => h.Images)
            .Include(h => h.Rooms)
                .ThenInclude(r => r.Reservations)
            .FirstOrDefaultAsync(h => h.Id == id);
            
        if (hotel == null)
            return false;

        // Delete reservations for each room first
        foreach (var room in hotel.Rooms)
        {
            if (room.Reservations != null && room.Reservations.Any())
            {
                _context.Reservations.RemoveRange(room.Reservations);
            }
        }

        // Delete related images
        if (hotel.Images != null && hotel.Images.Any())
        {
            _context.HotelImages.RemoveRange(hotel.Images);
        }

        // Delete related rooms
        if (hotel.Rooms != null && hotel.Rooms.Any())
        {
            _context.Rooms.RemoveRange(hotel.Rooms);
        }

        // Delete the hotel
        _context.Hotels.Remove(hotel);
        await _context.SaveChangesAsync();
        return true;
    }
}

