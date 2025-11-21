using AutoMapper;
using HotelReservation.Core.DTOs;
using HotelReservation.Core.Models;
using HotelReservation.Data.Context;
using HotelReservation.Data.Repositories.Interfaces;
using HotelReservation.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace HotelReservation.Services;

public class RoomService : IRoomService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;
    private readonly ApplicationDbContext _context;

    public RoomService(IUnitOfWork unitOfWork, IMapper mapper, ApplicationDbContext context)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
        _context = context;
    }

    public async Task<IEnumerable<RoomListDto>> GetAllRoomsAsync()
    {
        var rooms = await _context.Rooms.Include(r => r.Hotel).ToListAsync();
        return _mapper.Map<IEnumerable<RoomListDto>>(rooms);
    }

    public async Task<IEnumerable<RoomListDto>> GetRoomsByHotelAsync(int hotelId)
    {
        var rooms = await _unitOfWork.Rooms.GetRoomsByHotelAsync(hotelId);
        return _mapper.Map<IEnumerable<RoomListDto>>(rooms);
    }

    public async Task<RoomDetailsDto?> GetRoomByIdAsync(int id)
    {
        var room = await _unitOfWork.Rooms.GetRoomWithDetailsAsync(id);
        if (room == null)
            return null;

        return _mapper.Map<RoomDetailsDto>(room);
    }

    public async Task<IEnumerable<RoomListDto>> SearchRoomsAsync(RoomSearchDto searchDto)
    {
        var rooms = await _unitOfWork.Rooms.SearchRoomsAsync(searchDto);
        return _mapper.Map<IEnumerable<RoomListDto>>(rooms);
    }

    public async Task<int> CreateRoomAsync(RoomCreateDto dto)
    {
        var room = _mapper.Map<Room>(dto);
        await _unitOfWork.Rooms.AddAsync(room);
        await _unitOfWork.SaveChangesAsync();

        // Add amenities
        if (dto.AmenityIds.Any())
        {
            var roomAmenities = dto.AmenityIds.Select(amenityId => new RoomAmenity
            {
                RoomId = room.Id,
                AmenityId = amenityId
            });

            await _context.RoomAmenities.AddRangeAsync(roomAmenities);
            await _context.SaveChangesAsync();
        }

        return room.Id;
    }

    public async Task<bool> UpdateRoomAsync(RoomUpdateDto dto)
    {
        var room = await _unitOfWork.Rooms.GetByIdAsync(dto.Id);
        if (room == null)
            return false;

        _mapper.Map(dto, room);
        _unitOfWork.Rooms.Update(room);

        // Update amenities
        var existingAmenities = await _context.RoomAmenities
            .Where(ra => ra.RoomId == dto.Id)
            .ToListAsync();

        _context.RoomAmenities.RemoveRange(existingAmenities);

        var newAmenities = dto.AmenityIds.Select(amenityId => new RoomAmenity
        {
            RoomId = dto.Id,
            AmenityId = amenityId
        });

        await _context.RoomAmenities.AddRangeAsync(newAmenities);
        await _unitOfWork.SaveChangesAsync();
        
        return true;
    }

    public async Task<bool> DeleteRoomAsync(int id)
    {
        var room = await _unitOfWork.Rooms.GetByIdAsync(id);
        if (room == null)
            return false;

        _unitOfWork.Rooms.Remove(room);
        await _unitOfWork.SaveChangesAsync();
        return true;
    }

    public async Task<bool> IsRoomAvailableAsync(int roomId, DateTime checkIn, DateTime checkOut, int? excludeReservationId = null)
    {
        return await _unitOfWork.Rooms.IsRoomAvailableAsync(roomId, checkIn, checkOut, excludeReservationId);
    }
}
