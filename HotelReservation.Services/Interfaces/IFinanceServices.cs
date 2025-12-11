using HotelReservation.Core.DTOs;

namespace HotelReservation.Services.Interfaces;

public interface IExpenseCategoryService
{
    Task<IEnumerable<ExpenseCategoryListDto>> GetAllCategoriesAsync();
    Task<IEnumerable<ExpenseCategoryListDto>> GetActiveCategoriesAsync();
    Task<ExpenseCategoryListDto?> GetCategoryByIdAsync(int id);
    Task<int> CreateCategoryAsync(ExpenseCategoryCreateDto dto);
    Task<bool> UpdateCategoryAsync(ExpenseCategoryUpdateDto dto);
    Task<bool> DeleteCategoryAsync(int id);
}

public interface IExpenseService
{
    Task<IEnumerable<ExpenseListDto>> GetAllExpensesAsync();
    Task<IEnumerable<ExpenseListDto>> GetExpensesByDateRangeAsync(DateTime startDate, DateTime endDate);
    Task<IEnumerable<ExpenseListDto>> GetExpensesByCategoryAsync(int categoryId);
    Task<ExpenseDetailsDto?> GetExpenseByIdAsync(int id);
    Task<int> CreateExpenseAsync(ExpenseCreateDto dto, string createdBy);
    Task<bool> UpdateExpenseAsync(ExpenseUpdateDto dto);
    Task<bool> ApproveExpenseAsync(int id, string approvedBy);
    Task<bool> RejectExpenseAsync(int id, string rejectedBy);
    Task<bool> DeleteExpenseAsync(int id);
    Task<decimal> GetTotalExpensesByMonthAsync(int month, int year);
    Task<IEnumerable<ExpenseByCategoryDto>> GetExpensesByCategoryReportAsync(int month, int year);
    Task<Dictionary<string, decimal>> GetExpenseTotalsByCategoryAsync(DateTime startDate, DateTime endDate);
}

public interface IIncomeService
{
    Task<IEnumerable<IncomeListDto>> GetAllIncomesAsync();
    Task<IEnumerable<IncomeListDto>> GetIncomesByDateRangeAsync(DateTime startDate, DateTime endDate);
    Task<IEnumerable<IncomeListDto>> GetIncomesByTypeAsync(int type);
    Task<IncomeDetailsDto?> GetIncomeByIdAsync(int id);
    Task<int> CreateIncomeAsync(IncomeCreateDto dto, string createdBy);
    Task<bool> UpdateIncomeAsync(IncomeUpdateDto dto);
    Task<bool> DeleteIncomeAsync(int id);
    Task<decimal> GetTotalIncomeByMonthAsync(int month, int year);
    Task<IEnumerable<IncomeByTypeDto>> GetIncomeByTypeReportAsync(int month, int year);
    Task<int> CreateIncomeFromReservationAsync(int reservationId, decimal amount, string createdBy);
}

public interface IFinancialReportService
{
    Task<FinancialSummaryDto> GetFinancialSummaryAsync(int month, int year);
    Task<IEnumerable<MonthlyFinancialDto>> GetMonthlyFinancialsAsync(int year);
    Task<FinancialSummaryDto> GetTodaySummaryAsync();
    Task GenerateMonthlyReportAsync(int month, int year, string generatedBy);
}
