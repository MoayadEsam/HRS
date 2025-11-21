using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using HotelReservation.Core.Enums;

namespace HotelReservation.Core.Models;

public class Reservation
{
    public int Id { get; set; }
    
    [Required]
    public DateTime CheckInDate { get; set; }
    
    [Required]
    public DateTime CheckOutDate { get; set; }
    
    [Range(1, 10)]
    public int NumberOfGuests { get; set; }
    
    [Column(TypeName = "decimal(18,2)")]
    public decimal TotalPrice { get; set; }
    
    public ReservationStatus Status { get; set; } = ReservationStatus.Pending;
    
    public string? SpecialRequests { get; set; }
    
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    
    public DateTime? UpdatedAt { get; set; }
    
    // Foreign keys
    [Required]
    public string UserId { get; set; } = string.Empty;
    
    public int RoomId { get; set; }
    
    // Navigation properties
    public virtual ApplicationUser User { get; set; } = null!;
    public virtual Room Room { get; set; } = null!;
}
