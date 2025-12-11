using System.ComponentModel.DataAnnotations;
using HotelReservation.Core.Models;

namespace HotelReservation.Core.DTOs;

// Inventory Category DTOs
public class InventoryCategoryListDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public bool IsActive { get; set; }
    public int ItemCount { get; set; }
    public decimal TotalValue { get; set; }
}

// Alias for view compatibility
public class InventoryCategoryDto : InventoryCategoryListDto
{
}

public class InventoryCategoryCreateDto
{
    [Required(ErrorMessage = "Category name is required")]
    [StringLength(100)]
    public string Name { get; set; } = string.Empty;
    
    [StringLength(500)]
    public string? Description { get; set; }
}

public class InventoryCategoryUpdateDto : InventoryCategoryCreateDto
{
    public int Id { get; set; }
    public bool IsActive { get; set; } = true;
}

// Inventory Item DTOs
public class InventoryItemListDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? SKU { get; set; }
    public string CategoryName { get; set; } = string.Empty;
    public int Quantity { get; set; }
    public int MinimumQuantity { get; set; }
    public int ReorderLevel { get; set; }
    public string Unit { get; set; } = string.Empty;
    public string? UnitOfMeasure { get; set; }
    public decimal UnitPrice { get; set; }
    public decimal TotalValue { get; set; }
    public bool IsLowStock { get; set; }
    public bool IsActive { get; set; }
}

public class InventoryItemDetailsDto : InventoryItemListDto
{
    public string? Description { get; set; }
    public int CategoryId { get; set; }
    public string? Location { get; set; }
    public string? Supplier { get; set; }
    public DateTime? LastRestocked { get; set; }
    public DateTime LastUpdated { get; set; }
    public DateTime CreatedAt { get; set; }
    public List<InventoryTransactionDto> RecentTransactions { get; set; } = new();
}

public class InventoryItemCreateDto
{
    [Required(ErrorMessage = "Item name is required")]
    [StringLength(100)]
    public string Name { get; set; } = string.Empty;
    
    [StringLength(50)]
    public string? SKU { get; set; }
    
    [StringLength(500)]
    public string? Description { get; set; }
    
    [Required(ErrorMessage = "Category is required")]
    public int CategoryId { get; set; }
    
    [Required(ErrorMessage = "Quantity is required")]
    [Range(0, int.MaxValue)]
    public int Quantity { get; set; }
    
    [Range(0, int.MaxValue)]
    public int MinimumQuantity { get; set; } = 10;
    
    [Range(0, int.MaxValue)]
    public int ReorderLevel { get; set; } = 10;
    
    [StringLength(50)]
    public string Unit { get; set; } = "pcs";
    
    [StringLength(50)]
    public string? UnitOfMeasure { get; set; }
    
    [Required(ErrorMessage = "Unit price is required")]
    [Range(0, double.MaxValue)]
    public decimal UnitPrice { get; set; }
    
    [StringLength(100)]
    public string? Location { get; set; }
    
    [StringLength(100)]
    public string? Supplier { get; set; }
    
    [StringLength(500)]
    public string? Notes { get; set; }
}

public class InventoryItemUpdateDto : InventoryItemCreateDto
{
    public int Id { get; set; }
    public bool IsActive { get; set; } = true;
}

// Inventory Transaction DTOs
public class InventoryTransactionListDto
{
    public int Id { get; set; }
    public int ItemId { get; set; }
    public string ItemName { get; set; } = string.Empty;
    public string CategoryName { get; set; } = string.Empty;
    public string TypeName { get; set; } = string.Empty;
    public string TransactionType => TypeName;
    public TransactionType Type { get; set; }
    public int Quantity { get; set; }
    public int PreviousQuantity { get; set; }
    public int NewQuantity { get; set; }
    public decimal UnitPrice { get; set; }
    public decimal TotalPrice => UnitPrice * Quantity;
    public DateTime TransactionDate { get; set; }
    public DateTime CreatedAt { get; set; }
    public string? CreatedByName { get; set; }
    public string CreatedBy { get; set; } = string.Empty;
    public string? Notes { get; set; }
}

public class InventoryTransactionDto
{
    public int Id { get; set; }
    public int ItemId { get; set; }
    public string ItemName { get; set; } = string.Empty;
    public TransactionType Type { get; set; }
    public string TypeName { get; set; } = string.Empty;
    public int Quantity { get; set; }
    public int PreviousQuantity { get; set; }
    public int NewQuantity { get; set; }
    public decimal UnitPrice { get; set; }
    public decimal TotalPrice { get; set; }
    public string? Reference { get; set; }
    public string? Notes { get; set; }
    public DateTime TransactionDate { get; set; }
    public string CreatedBy { get; set; } = string.Empty;
}

public class InventoryTransactionCreateDto
{
    [Required]
    public int ItemId { get; set; }
    
    [Required]
    public TransactionType Type { get; set; }
    
    [Required]
    [Range(1, int.MaxValue, ErrorMessage = "Quantity must be at least 1")]
    public int Quantity { get; set; }
    
    [Range(0, double.MaxValue)]
    public decimal? UnitPrice { get; set; }
    
    [StringLength(500)]
    public string? Notes { get; set; }
    
    [StringLength(100)]
    public string? Reference { get; set; }
}

// Inventory Summary DTOs
public class InventorySummaryDto
{
    public int TotalItems { get; set; }
    public int TotalCategories { get; set; }
    public int LowStockItems { get; set; }
    public int LowStockCount { get; set; }
    public int OutOfStockItems { get; set; }
    public int OutOfStockCount { get; set; }
    public decimal TotalValue { get; set; }
    public int TodayTransactions { get; set; }
    public List<InventoryItemListDto> LowStockAlerts { get; set; } = new();
}
