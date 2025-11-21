using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using HotelReservation.Core.Enums;

namespace HotelReservation.Core.Models;

public class Room
{
    public int Id { get; set; }
    
    [Required]
    [StringLength(50)]
    public string RoomNumber { get; set; } = string.Empty;
    
    [Required]
    public RoomType Type { get; set; }
    
    [Range(1, 10)]
    public int Capacity { get; set; }
    
    [Column(TypeName = "decimal(18,2)")]
    [Range(0, 100000)]
    public decimal PricePerNight { get; set; }
    
    public string? Description { get; set; }
    
    public int FloorNumber { get; set; }
    
    public bool IsAvailable { get; set; } = true;
    
    // Foreign key
    public int HotelId { get; set; }
    
    // Navigation properties
    public virtual Hotel Hotel { get; set; } = null!;
    public virtual ICollection<Reservation> Reservations { get; set; } = new List<Reservation>();
    public virtual ICollection<RoomAmenity> RoomAmenities { get; set; } = new List<RoomAmenity>();
}
