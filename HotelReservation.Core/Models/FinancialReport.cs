using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace HotelReservation.Core.Models;

public class FinancialReport
{
    public int Id { get; set; }
    
    public ReportType Type { get; set; }
    
    public int Month { get; set; }
    
    public int Year { get; set; }
    
    [Column(TypeName = "decimal(18,2)")]
    public decimal TotalIncome { get; set; }
    
    [Column(TypeName = "decimal(18,2)")]
    public decimal TotalExpenses { get; set; }
    
    [Column(TypeName = "decimal(18,2)")]
    public decimal TotalSalaries { get; set; }
    
    [Column(TypeName = "decimal(18,2)")]
    public decimal NetProfit { get; set; }
    
    public int TotalReservations { get; set; }
    
    public int TotalCheckIns { get; set; }
    
    public int TotalCheckOuts { get; set; }
    
    [Column(TypeName = "decimal(5,2)")]
    public decimal OccupancyRate { get; set; }
    
    [StringLength(500)]
    public string? Notes { get; set; }
    
    [StringLength(256)]
    public string GeneratedBy { get; set; } = string.Empty;
    
    public DateTime GeneratedAt { get; set; } = DateTime.UtcNow;
}

public enum ReportType
{
    Daily,
    Weekly,
    Monthly,
    Quarterly,
    Annual
}
