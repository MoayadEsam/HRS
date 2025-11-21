using System.ComponentModel.DataAnnotations;
using HotelReservation.Core.Enums;

namespace HotelReservation.Core.DTOs;

public class RoomCreateDto
{
    [Required(ErrorMessage = "Room number is required")]
    [StringLength(50)]
    public string RoomNumber { get; set; } = string.Empty;
    
    [Required(ErrorMessage = "Room type is required")]
    public RoomType Type { get; set; }
    
    [Range(1, 10, ErrorMessage = "Capacity must be between 1 and 10")]
    public int Capacity { get; set; }
    
    [Required(ErrorMessage = "Price per night is required")]
    [Range(0.01, 100000, ErrorMessage = "Price must be between 0.01 and 100000")]
    public decimal PricePerNight { get; set; }
    
    public string? Description { get; set; }
    
    public int FloorNumber { get; set; }
    
    [Required(ErrorMessage = "Hotel is required")]
    public int HotelId { get; set; }
    
    public List<int> AmenityIds { get; set; } = new();
}

public class RoomUpdateDto : RoomCreateDto
{
    public int Id { get; set; }
    public bool IsAvailable { get; set; }
}

public class RoomListDto
{
    public int Id { get; set; }
    public string RoomNumber { get; set; } = string.Empty;
    public RoomType Type { get; set; }
    public string TypeName => Type.ToString();
    public int Capacity { get; set; }
    public decimal PricePerNight { get; set; }
    public bool IsAvailable { get; set; }
    public string HotelName { get; set; } = string.Empty;
    public int HotelId { get; set; }
    public int FloorNumber { get; set; }
}

public class RoomDetailsDto
{
    public int Id { get; set; }
    public string RoomNumber { get; set; } = string.Empty;
    public RoomType Type { get; set; }
    public string TypeName => Type.ToString();
    public int Capacity { get; set; }
    public decimal PricePerNight { get; set; }
    public string? Description { get; set; }
    public int FloorNumber { get; set; }
    public bool IsAvailable { get; set; }
    public int HotelId { get; set; }
    public string HotelName { get; set; } = string.Empty;
    public List<AmenityListDto> Amenities { get; set; } = new();
}

public class RoomSearchDto
{
    public int? HotelId { get; set; }
    public DateTime? CheckInDate { get; set; }
    public DateTime? CheckOutDate { get; set; }
    public decimal? MinPrice { get; set; }
    public decimal? MaxPrice { get; set; }
    public RoomType? Type { get; set; }
    public int? MinCapacity { get; set; }
    public List<int>? AmenityIds { get; set; }
}
