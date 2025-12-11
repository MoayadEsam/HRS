using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace HotelReservation.Core.Models;

public class InventoryTransaction
{
    public int Id { get; set; }
    
    public int ItemId { get; set; }
    
    public TransactionType Type { get; set; }
    
    public int Quantity { get; set; }
    
    public int PreviousQuantity { get; set; }
    
    public int NewQuantity { get; set; }
    
    [Column(TypeName = "decimal(18,2)")]
    public decimal UnitPrice { get; set; }
    
    [Column(TypeName = "decimal(18,2)")]
    public decimal TotalPrice { get; set; }
    
    [StringLength(500)]
    public string? Notes { get; set; }
    
    [StringLength(100)]
    public string? Reference { get; set; }
    
    [StringLength(256)]
    public string CreatedBy { get; set; } = string.Empty;
    
    public DateTime TransactionDate { get; set; } = DateTime.UtcNow;
    
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    
    // Navigation
    public virtual InventoryItem Item { get; set; } = null!;
}

public enum TransactionType
{
    In,
    Out,
    Adjustment,
    Transfer,
    Damaged,
    Returned
}
