using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace HotelReservation.Core.Models;

public class Income
{
    public int Id { get; set; }
    
    [Required]
    [StringLength(200)]
    public string Title { get; set; } = string.Empty;
    
    [StringLength(1000)]
    public string? Description { get; set; }
    
    [Required]
    [Column(TypeName = "decimal(18,2)")]
    public decimal Amount { get; set; }
    
    public IncomeType Type { get; set; }
    
    [Required]
    public DateTime IncomeDate { get; set; }
    
    public PaymentMethod PaymentMethod { get; set; } = PaymentMethod.Cash;
    
    [StringLength(100)]
    public string? Reference { get; set; }
    
    // Link to reservation if income is from booking
    public int? ReservationId { get; set; }
    
    [StringLength(256)]
    public string CreatedBy { get; set; } = string.Empty;
    
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    
    public DateTime? UpdatedAt { get; set; }
    
    // Navigation
    public virtual Reservation? Reservation { get; set; }
}

public enum IncomeType
{
    RoomBooking,
    RoomService,
    Restaurant,
    Spa,
    Laundry,
    Parking,
    EventHall,
    MiniBar,
    Other
}
