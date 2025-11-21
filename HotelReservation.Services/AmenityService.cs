using AutoMapper;
using HotelReservation.Core.DTOs;
using HotelReservation.Core.Models;
using HotelReservation.Data.Repositories.Interfaces;
using HotelReservation.Services.Interfaces;

namespace HotelReservation.Services;

public class AmenityService : IAmenityService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public AmenityService(IUnitOfWork unitOfWork, IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    public async Task<IEnumerable<AmenityListDto>> GetAllAmenitiesAsync()
    {
        var amenities = await _unitOfWork.Amenities.GetAllAsync();
        return _mapper.Map<IEnumerable<AmenityListDto>>(amenities);
    }

    public async Task<IEnumerable<AmenityListDto>> GetActiveAmenitiesAsync()
    {
        var amenities = await _unitOfWork.Amenities.GetActiveAmenitiesAsync();
        return _mapper.Map<IEnumerable<AmenityListDto>>(amenities);
    }

    public async Task<AmenityListDto?> GetAmenityByIdAsync(int id)
    {
        var amenity = await _unitOfWork.Amenities.GetByIdAsync(id);
        if (amenity == null)
            return null;

        return _mapper.Map<AmenityListDto>(amenity);
    }

    public async Task<int> CreateAmenityAsync(AmenityCreateDto dto)
    {
        var amenity = _mapper.Map<Amenity>(dto);
        await _unitOfWork.Amenities.AddAsync(amenity);
        await _unitOfWork.SaveChangesAsync();
        return amenity.Id;
    }

    public async Task<bool> UpdateAmenityAsync(AmenityUpdateDto dto)
    {
        var amenity = await _unitOfWork.Amenities.GetByIdAsync(dto.Id);
        if (amenity == null)
            return false;

        _mapper.Map(dto, amenity);
        _unitOfWork.Amenities.Update(amenity);
        await _unitOfWork.SaveChangesAsync();
        return true;
    }

    public async Task<bool> DeleteAmenityAsync(int id)
    {
        var amenity = await _unitOfWork.Amenities.GetByIdAsync(id);
        if (amenity == null)
            return false;

        _unitOfWork.Amenities.Remove(amenity);
        await _unitOfWork.SaveChangesAsync();
        return true;
    }
}
