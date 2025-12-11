using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace HotelReservation.Core.Models;

public class Expense
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
    
    public int CategoryId { get; set; }
    
    [Required]
    public DateTime ExpenseDate { get; set; }
    
    public PaymentMethod PaymentMethod { get; set; } = PaymentMethod.Cash;
    
    [StringLength(100)]
    public string? Reference { get; set; }
    
    [StringLength(256)]
    public string? ReceiptUrl { get; set; }
    
    [StringLength(100)]
    public string? Vendor { get; set; }
    
    public ExpenseStatus Status { get; set; } = ExpenseStatus.Pending;
    
    [StringLength(256)]
    public string? ApprovedBy { get; set; }
    
    public DateTime? ApprovedDate { get; set; }
    
    [StringLength(256)]
    public string CreatedBy { get; set; } = string.Empty;
    
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    
    public DateTime? UpdatedAt { get; set; }
    
    // Navigation
    public virtual ExpenseCategory Category { get; set; } = null!;
}

public enum ExpenseStatus
{
    Pending,
    Approved,
    Rejected,
    Paid
}

public enum PaymentMethod
{
    Cash,
    CreditCard,
    DebitCard,
    BankTransfer,
    Check,
    Online
}
