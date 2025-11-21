using AutoMapper;
using HotelReservation.Core.DTOs;
using HotelReservation.Core.Models;

namespace HotelReservation.Services.Mappings;

public class MappingProfile : Profile
{
    public MappingProfile()
    {
        // Hotel mappings
        CreateMap<Hotel, HotelListDto>()
            .ForMember(dest => dest.RoomCount, opt => opt.MapFrom(src => src.Rooms.Count));
        CreateMap<Hotel, HotelDetailsDto>();
        CreateMap<HotelCreateDto, Hotel>();
        CreateMap<HotelUpdateDto, Hotel>();

        // Room mappings
        CreateMap<Room, RoomListDto>()
            .ForMember(dest => dest.HotelName, opt => opt.MapFrom(src => src.Hotel.Name));
        CreateMap<Room, RoomDetailsDto>()
            .ForMember(dest => dest.HotelName, opt => opt.MapFrom(src => src.Hotel.Name))
            .ForMember(dest => dest.Amenities, opt => opt.MapFrom(src => src.RoomAmenities.Select(ra => ra.Amenity)));
        CreateMap<RoomCreateDto, Room>();
        CreateMap<RoomUpdateDto, Room>();

        // Amenity mappings
        CreateMap<Amenity, AmenityListDto>();
        CreateMap<AmenityCreateDto, Amenity>();
        CreateMap<AmenityUpdateDto, Amenity>();

        // Reservation mappings
        CreateMap<Reservation, ReservationListDto>()
            .ForMember(dest => dest.RoomNumber, opt => opt.MapFrom(src => src.Room.RoomNumber))
            .ForMember(dest => dest.HotelName, opt => opt.MapFrom(src => src.Room.Hotel.Name))
            .ForMember(dest => dest.UserName, opt => opt.MapFrom(src => src.User.FirstName + " " + src.User.LastName));
        
        CreateMap<Reservation, ReservationDetailsDto>()
            .ForMember(dest => dest.NumberOfNights, opt => opt.MapFrom(src => (src.CheckOutDate - src.CheckInDate).Days))
            .ForMember(dest => dest.RoomNumber, opt => opt.MapFrom(src => src.Room.RoomNumber))
            .ForMember(dest => dest.RoomType, opt => opt.MapFrom(src => src.Room.Type.ToString()))
            .ForMember(dest => dest.PricePerNight, opt => opt.MapFrom(src => src.Room.PricePerNight))
            .ForMember(dest => dest.HotelId, opt => opt.MapFrom(src => src.Room.HotelId))
            .ForMember(dest => dest.HotelName, opt => opt.MapFrom(src => src.Room.Hotel.Name))
            .ForMember(dest => dest.HotelAddress, opt => opt.MapFrom(src => src.Room.Hotel.Address))
            .ForMember(dest => dest.UserName, opt => opt.MapFrom(src => src.User.FirstName + " " + src.User.LastName))
            .ForMember(dest => dest.UserEmail, opt => opt.MapFrom(src => src.User.Email));
        
        CreateMap<ReservationCreateDto, Reservation>();
        CreateMap<ReservationUpdateDto, Reservation>();

        // User mappings
        CreateMap<ApplicationUser, UserDto>()
            .ForMember(dest => dest.Roles, opt => opt.Ignore());
    }
}
