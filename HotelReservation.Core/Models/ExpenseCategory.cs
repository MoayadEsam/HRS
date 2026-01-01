using System.ComponentModel.DataAnnotations;

namespace HotelReservation.Core.Models;

public class ExpenseCategory
{
    public int Id { get; set; }
    
    [Required]
    [StringLength(100)]
    public string Name { get; set; } = string.Empty;
    
    [StringLength(500)]
    public string? Description { get; set; }
    
    [StringLength(50)]
    public string? IconClass { get; set; }
    
    public decimal Budget { get; set; }
    
    public bool IsActive { get; set; } = true;
    
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    
    // Navigation
    public virtual ICollection<Expense> Expenses { get; set; } = new List<Expense>();
}
