using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace HotelReservation.Core.Models;

public class InventoryItem
{
    public int Id { get; set; }
    
    [Required]
    [StringLength(100)]
    public string Name { get; set; } = string.Empty;
    
    [StringLength(50)]
    public string? SKU { get; set; }
    
    [StringLength(500)]
    public string? Description { get; set; }
    
    public int CategoryId { get; set; }
    
    public int Quantity { get; set; }
    
    public int MinimumQuantity { get; set; } = 10;
    
    public int ReorderLevel { get; set; } = 10;
    
    [StringLength(50)]
    public string Unit { get; set; } = "pcs";
    
    [StringLength(50)]
    public string? UnitOfMeasure { get; set; }
    
    [Column(TypeName = "decimal(18,2)")]
    public decimal UnitPrice { get; set; }
    
    [StringLength(100)]
    public string? Location { get; set; }
    
    [StringLength(100)]
    public string? Supplier { get; set; }
    
    public bool IsActive { get; set; } = true;
    
    public DateTime? LastRestocked { get; set; }
    
    [StringLength(256)]
    public string? CreatedBy { get; set; }
    
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    
    public DateTime? UpdatedAt { get; set; }
    
    // Computed
    [NotMapped]
    public bool IsLowStock => Quantity <= ReorderLevel;
    
    [NotMapped]
    public decimal TotalValue => Quantity * UnitPrice;
    
    // Navigation
    public virtual InventoryCategory Category { get; set; } = null!;
    public virtual ICollection<InventoryTransaction> Transactions { get; set; } = new List<InventoryTransaction>();
}
