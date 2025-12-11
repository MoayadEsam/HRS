using System.ComponentModel.DataAnnotations;

namespace HotelReservation.Core.Models;

public class InventoryCategory
{
    public int Id { get; set; }
    
    [Required]
    [StringLength(100)]
    public string Name { get; set; } = string.Empty;
    
    [StringLength(500)]
    public string? Description { get; set; }
    
    public bool IsActive { get; set; } = true;
    
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    
    // Navigation
    public virtual ICollection<InventoryItem> Items { get; set; } = new List<InventoryItem>();
}
