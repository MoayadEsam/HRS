using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace HotelReservation.Core.Models;

public class Salary
{
    public int Id { get; set; }
    
    public int EmployeeId { get; set; }
    
    [Required]
    public int Month { get; set; } // 1-12
    
    [Required]
    public int Year { get; set; }
    
    [Column(TypeName = "decimal(18,2)")]
    public decimal BaseSalary { get; set; }
    
    [Column(TypeName = "decimal(18,2)")]
    public decimal Bonus { get; set; } = 0;
    
    [Column(TypeName = "decimal(18,2)")]
    public decimal Deductions { get; set; } = 0;
    
    [Column(TypeName = "decimal(18,2)")]
    public decimal Overtime { get; set; } = 0;
    
    [Column(TypeName = "decimal(18,2)")]
    public decimal NetSalary { get; set; }
    
    public SalaryStatus Status { get; set; } = SalaryStatus.Pending;
    
    public DateTime? PaidDate { get; set; }
    
    [StringLength(500)]
    public string? Notes { get; set; }
    
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    
    // Navigation
    public virtual Employee Employee { get; set; } = null!;
}

public enum SalaryStatus
{
    Pending,
    Approved,
    Paid,
    Cancelled
}
