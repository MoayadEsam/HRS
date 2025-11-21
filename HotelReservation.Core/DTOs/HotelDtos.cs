using System.ComponentModel.DataAnnotations;

namespace HotelReservation.Core.DTOs;

public class HotelCreateDto
{
    [Required(ErrorMessage = "Hotel name is required")]
    [StringLength(200, ErrorMessage = "Name cannot exceed 200 characters")]
    public string Name { get; set; } = string.Empty;
    
    [Required(ErrorMessage = "Address is required")]
    [StringLength(500)]
    public string Address { get; set; } = string.Empty;
    
    [Required(ErrorMessage = "City is required")]
    [StringLength(100)]
    public string City { get; set; } = string.Empty;
    
    [Required(ErrorMessage = "Country is required")]
    [StringLength(100)]
    public string Country { get; set; } = string.Empty;
    
    [StringLength(20)]
    public string? PostalCode { get; set; }
    
    [Phone(ErrorMessage = "Invalid phone number format")]
    public string? PhoneNumber { get; set; }
    
    [EmailAddress(ErrorMessage = "Invalid email format")]
    public string? Email { get; set; }
    
    [Range(1, 5, ErrorMessage = "Star rating must be between 1 and 5")]
    public int StarRating { get; set; }
    
    public string? Description { get; set; }
    
    [StringLength(1000, ErrorMessage = "Image URL cannot exceed 1000 characters")]
    [Url(ErrorMessage = "Please enter a valid URL")]
    public string? ImageUrl { get; set; }
}

public class HotelUpdateDto : HotelCreateDto
{
    public int Id { get; set; }
    public bool IsActive { get; set; }
}

public class HotelListDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string City { get; set; } = string.Empty;
    public string Country { get; set; } = string.Empty;
    public int StarRating { get; set; }
    public bool IsActive { get; set; }
    public int RoomCount { get; set; }
    public string? ImageUrl { get; set; }
}

public class HotelDetailsDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Address { get; set; } = string.Empty;
    public string City { get; set; } = string.Empty;
    public string Country { get; set; } = string.Empty;
    public string? PhoneNumber { get; set; }
    public string? Email { get; set; }
    public int StarRating { get; set; }
    public string? Description { get; set; }
    public string? ImageUrl { get; set; }
    public bool IsActive { get; set; }
    public DateTime CreatedAt { get; set; }
    public List<RoomListDto> Rooms { get; set; } = new();
}
