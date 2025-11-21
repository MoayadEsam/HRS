using AutoMapper;
using HotelReservation.Core.DTOs;
using HotelReservation.Core.Models;
using HotelReservation.Data.Repositories.Interfaces;
using HotelReservation.Services.Interfaces;

namespace HotelReservation.Services;

public class HotelService : IHotelService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public HotelService(IUnitOfWork unitOfWork, IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
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
        var hotel = await _unitOfWork.Hotels.GetByIdAsync(id);
        if (hotel == null)
            return false;

        _unitOfWork.Hotels.Remove(hotel);
        await _unitOfWork.SaveChangesAsync();
        return true;
    }
}
