using System.ComponentModel.DataAnnotations;
using HotelReservation.Core.Enums;

namespace HotelReservation.Core.DTOs;

public class ReservationCreateDto
{
    [Required(ErrorMessage = "Check-in date is required")]
    [DataType(DataType.Date)]
    public DateTime CheckInDate { get; set; }
    
    [Required(ErrorMessage = "Check-out date is required")]
    [DataType(DataType.Date)]
    public DateTime CheckOutDate { get; set; }
    
    [Required(ErrorMessage = "Number of guests is required")]
    [Range(1, 10, ErrorMessage = "Number of guests must be between 1 and 10")]
    public int NumberOfGuests { get; set; }
    
    public string? SpecialRequests { get; set; }
    
    [Required(ErrorMessage = "Room is required")]
    public int RoomId { get; set; }
}

public class ReservationUpdateDto
{
    public int Id { get; set; }
    
    [Required]
    [DataType(DataType.Date)]
    public DateTime CheckInDate { get; set; }
    
    [Required]
    [DataType(DataType.Date)]
    public DateTime CheckOutDate { get; set; }
    
    [Range(1, 10)]
    public int NumberOfGuests { get; set; }
    
    public string? SpecialRequests { get; set; }
    
    public ReservationStatus Status { get; set; }
}

public class ReservationListDto
{
    public int Id { get; set; }
    public DateTime CheckInDate { get; set; }
    public DateTime CheckOutDate { get; set; }
    public int NumberOfGuests { get; set; }
    public decimal TotalPrice { get; set; }
    public ReservationStatus Status { get; set; }
    public string StatusName => Status.ToString();
    public string RoomNumber { get; set; } = string.Empty;
    public string HotelName { get; set; } = string.Empty;
    public string UserName { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
}

public class ReservationDetailsDto
{
    public int Id { get; set; }
    public DateTime CheckInDate { get; set; }
    public DateTime CheckOutDate { get; set; }
    public int NumberOfGuests { get; set; }
    public decimal TotalPrice { get; set; }
    public ReservationStatus Status { get; set; }
    public string StatusName => Status.ToString();
    public string? SpecialRequests { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public int NumberOfNights { get; set; }
    
    // Room details
    public int RoomId { get; set; }
    public string RoomNumber { get; set; } = string.Empty;
    public string RoomType { get; set; } = string.Empty;
    public decimal PricePerNight { get; set; }
    
    // Hotel details
    public int HotelId { get; set; }
    public string HotelName { get; set; } = string.Empty;
    public string HotelAddress { get; set; } = string.Empty;
    
    // User details
    public string UserId { get; set; } = string.Empty;
    public string UserName { get; set; } = string.Empty;
    public string UserEmail { get; set; } = string.Empty;
}
