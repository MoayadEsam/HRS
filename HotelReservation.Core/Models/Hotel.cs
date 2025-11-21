using System.ComponentModel.DataAnnotations;

namespace HotelReservation.Core.Models;

public class Hotel
{
    public int Id { get; set; }
    
    [Required]
    [StringLength(200)]
    public string Name { get; set; } = string.Empty;
    
    [Required]
    [StringLength(500)]
    public string Address { get; set; } = string.Empty;
    
    [Required]
    [StringLength(100)]
    public string City { get; set; } = string.Empty;
    
    [Required]
    [StringLength(100)]
    public string Country { get; set; } = string.Empty;
    
    [Phone]
    public string? PhoneNumber { get; set; }
    
    [EmailAddress]
    public string? Email { get; set; }
    
    [Range(1, 5)]
    public int StarRating { get; set; }
    
    public string? Description { get; set; }
    
    [StringLength(1000)]
    public string? ImageUrl { get; set; }
    
    public bool IsActive { get; set; } = true;
    
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    
    // Navigation properties
    public virtual ICollection<Room> Rooms { get; set; } = new List<Room>();
}
