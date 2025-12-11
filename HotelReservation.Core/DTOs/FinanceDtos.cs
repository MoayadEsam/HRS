using System.ComponentModel.DataAnnotations;
using HotelReservation.Core.Models;

namespace HotelReservation.Core.DTOs;

// Expense Category DTOs
public class ExpenseCategoryListDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string? IconClass { get; set; }
    public bool IsActive { get; set; }
    public int ExpenseCount { get; set; }
    public decimal TotalAmount { get; set; }
}

// Alias for view compatibility
public class ExpenseCategoryDto : ExpenseCategoryListDto
{
    public decimal Budget { get; set; }
    public decimal TotalSpent { get; set; }
}

public class ExpenseCategoryCreateDto
{
    [Required(ErrorMessage = "Category name is required")]
    [StringLength(100)]
    public string Name { get; set; } = string.Empty;
    
    [StringLength(500)]
    public string? Description { get; set; }
    
    [StringLength(50)]
    public string? IconClass { get; set; }
    
    [Range(0, double.MaxValue)]
    public decimal Budget { get; set; }
}

public class ExpenseCategoryUpdateDto : ExpenseCategoryCreateDto
{
    public int Id { get; set; }
    public bool IsActive { get; set; } = true;
}

// Expense DTOs
public class ExpenseListDto
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public string CategoryName { get; set; } = string.Empty;
    public DateTime ExpenseDate { get; set; }
    public string PaymentMethodName { get; set; } = string.Empty;
    public string StatusName { get; set; } = string.Empty;
    public string? Vendor { get; set; }
    public string? InvoiceNumber { get; set; }
    public bool IsApproved => StatusName == "Approved";
    public string? ApprovedByName { get; set; }
    public string? Notes { get; set; }
    public string CreatedBy { get; set; } = string.Empty;
}

public class ExpenseDetailsDto : ExpenseListDto
{
    public string? Description { get; set; }
    public int CategoryId { get; set; }
    public PaymentMethod PaymentMethod { get; set; }
    public ExpenseStatus Status { get; set; }
    public string? Reference { get; set; }
    public string? ReceiptUrl { get; set; }
    public string? ApprovedBy { get; set; }
    public DateTime? ApprovedDate { get; set; }
    public DateTime CreatedAt { get; set; }
}

public class ExpenseCreateDto
{
    [Required(ErrorMessage = "Title is required")]
    [StringLength(200)]
    public string Title { get; set; } = string.Empty;
    
    [Required(ErrorMessage = "Description is required")]
    [StringLength(500)]
    public string Description { get; set; } = string.Empty;
    
    [Required(ErrorMessage = "Amount is required")]
    [Range(0.01, double.MaxValue, ErrorMessage = "Amount must be greater than 0")]
    public decimal Amount { get; set; }
    
    [Required(ErrorMessage = "Category is required")]
    public int CategoryId { get; set; }
    
    [Required(ErrorMessage = "Expense date is required")]
    public DateTime ExpenseDate { get; set; }
    
    public PaymentMethod PaymentMethod { get; set; } = PaymentMethod.Cash;
    
    [StringLength(100)]
    public string? Reference { get; set; }
    
    [StringLength(256)]
    public string? ReceiptUrl { get; set; }
    
    [StringLength(200)]
    public string? Vendor { get; set; }
    
    [StringLength(50)]
    public string? InvoiceNumber { get; set; }
    
    [StringLength(500)]
    public string? Notes { get; set; }
}

public class ExpenseUpdateDto : ExpenseCreateDto
{
    public int Id { get; set; }
}

// Income DTOs
public class IncomeListDto
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public string Source => TypeName;
    public string TypeName { get; set; } = string.Empty;
    public DateTime IncomeDate { get; set; }
    public string? PaymentMethod => PaymentMethodName;
    public string PaymentMethodName { get; set; } = string.Empty;
    public string? ReferenceNumber => Reference;
    public string? Reference { get; set; }
    public int? ReservationId { get; set; }
    public string? Notes { get; set; }
    public string CreatedBy { get; set; } = string.Empty;
}

public class IncomeDetailsDto : IncomeListDto
{
    public string? Description { get; set; }
    public IncomeType Type { get; set; }
    public PaymentMethod PaymentMethod { get; set; }
    public DateTime CreatedAt { get; set; }
}

public class IncomeCreateDto
{
    [Required(ErrorMessage = "Title is required")]
    [StringLength(200)]
    public string Title { get; set; } = string.Empty;
    
    [Required(ErrorMessage = "Source is required")]
    [StringLength(100)]
    public string Source { get; set; } = string.Empty;
    
    [Required(ErrorMessage = "Description is required")]
    [StringLength(500)]
    public string Description { get; set; } = string.Empty;
    
    [Required(ErrorMessage = "Amount is required")]
    [Range(0.01, double.MaxValue, ErrorMessage = "Amount must be greater than 0")]
    public decimal Amount { get; set; }
    
    public IncomeType Type { get; set; }
    
    [Required(ErrorMessage = "Income date is required")]
    public DateTime IncomeDate { get; set; }
    
    public PaymentMethod PaymentMethodEnum { get; set; } = Models.PaymentMethod.Cash;
    
    [StringLength(50)]
    public string? PaymentMethod { get; set; }
    
    [StringLength(50)]
    public string? ReferenceNumber { get; set; }
    
    [StringLength(100)]
    public string? Reference { get; set; }
    
    public int? ReservationId { get; set; }
    
    [StringLength(500)]
    public string? Notes { get; set; }
}

public class IncomeUpdateDto : IncomeCreateDto
{
    public int Id { get; set; }
}

// Financial Summary DTOs
public class FinancialSummaryDto
{
    public decimal TotalIncome { get; set; }
    public decimal TotalExpenses { get; set; }
    public decimal TotalSalaries { get; set; }
    public decimal NetProfit { get; set; }
    public decimal PendingExpenses { get; set; }
    public decimal PendingSalaries { get; set; }
    public int TotalTransactions { get; set; }
    public decimal IncomeChangePercent { get; set; }
    public decimal ExpenseChangePercent { get; set; }
}

// Financial Report DTO for Reports view
public class FinancialReportDto
{
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public decimal TotalIncome { get; set; }
    public decimal TotalExpenses { get; set; }
    public decimal NetProfit { get; set; }
    public Dictionary<string, decimal>? IncomeBySource { get; set; }
    public Dictionary<string, decimal>? ExpensesByCategory { get; set; }
    public Dictionary<string, MonthlyDataDto>? MonthlyData { get; set; }
}

public class MonthlyDataDto
{
    public decimal Income { get; set; }
    public decimal Expenses { get; set; }
}

public class MonthlyFinancialDto
{
    public int Month { get; set; }
    public int Year { get; set; }
    public string MonthName { get; set; } = string.Empty;
    public decimal Income { get; set; }
    public decimal Expenses { get; set; }
    public decimal Salaries { get; set; }
    public decimal Profit { get; set; }
}

public class IncomeByTypeDto
{
    public string TypeName { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public int Count { get; set; }
    public decimal Percentage { get; set; }
}

public class ExpenseByCategoryDto
{
    public string CategoryName { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public int Count { get; set; }
    public decimal Percentage { get; set; }
}
