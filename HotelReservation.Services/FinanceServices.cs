using HotelReservation.Core.DTOs;
using HotelReservation.Core.Models;
using HotelReservation.Data.Context;
using HotelReservation.Services.Interfaces;
using Microsoft.EntityFrameworkCore;
using System.Globalization;

namespace HotelReservation.Services;

public class ExpenseCategoryService : IExpenseCategoryService
{
    private readonly ApplicationDbContext _context;

    public ExpenseCategoryService(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<ExpenseCategoryListDto>> GetAllCategoriesAsync()
    {
        var categories = await _context.ExpenseCategories
            .Include(c => c.Expenses)
            .OrderBy(c => c.Name)
            .ToListAsync();

        return categories.Select(c => new ExpenseCategoryListDto
        {
            Id = c.Id,
            Name = c.Name,
            Description = c.Description,
            IconClass = c.IconClass,
            IsActive = c.IsActive,
            ExpenseCount = c.Expenses.Count,
            TotalAmount = c.Expenses.Sum(e => e.Amount)
        });
    }

    public async Task<IEnumerable<ExpenseCategoryListDto>> GetActiveCategoriesAsync()
    {
        var categories = await _context.ExpenseCategories
            .Where(c => c.IsActive)
            .Include(c => c.Expenses)
            .OrderBy(c => c.Name)
            .ToListAsync();

        return categories.Select(c => new ExpenseCategoryListDto
        {
            Id = c.Id,
            Name = c.Name,
            Description = c.Description,
            IconClass = c.IconClass,
            IsActive = c.IsActive,
            ExpenseCount = c.Expenses.Count,
            TotalAmount = c.Expenses.Sum(e => e.Amount)
        });
    }

    public async Task<ExpenseCategoryListDto?> GetCategoryByIdAsync(int id)
    {
        var category = await _context.ExpenseCategories
            .Include(c => c.Expenses)
            .FirstOrDefaultAsync(c => c.Id == id);

        if (category == null) return null;

        return new ExpenseCategoryListDto
        {
            Id = category.Id,
            Name = category.Name,
            Description = category.Description,
            IconClass = category.IconClass,
            IsActive = category.IsActive,
            ExpenseCount = category.Expenses.Count,
            TotalAmount = category.Expenses.Sum(e => e.Amount)
        };
    }

    public async Task<int> CreateCategoryAsync(ExpenseCategoryCreateDto dto)
    {
        var category = new ExpenseCategory
        {
            Name = dto.Name,
            Description = dto.Description,
            IconClass = dto.IconClass,
            Budget = dto.Budget,
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };

        _context.ExpenseCategories.Add(category);
        await _context.SaveChangesAsync();
        return category.Id;
    }

    public async Task<bool> UpdateCategoryAsync(ExpenseCategoryUpdateDto dto)
    {
        var category = await _context.ExpenseCategories.FindAsync(dto.Id);
        if (category == null) return false;

        category.Name = dto.Name;
        category.Description = dto.Description;
        category.IconClass = dto.IconClass;
        category.Budget = dto.Budget;
        category.IsActive = dto.IsActive;

        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<bool> DeleteCategoryAsync(int id)
    {
        var category = await _context.ExpenseCategories
            .Include(c => c.Expenses)
            .FirstOrDefaultAsync(c => c.Id == id);

        if (category == null) return false;
        if (category.Expenses.Any()) return false;

        _context.ExpenseCategories.Remove(category);
        await _context.SaveChangesAsync();
        return true;
    }
}

public class ExpenseService : IExpenseService
{
    private readonly ApplicationDbContext _context;

    public ExpenseService(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<ExpenseListDto>> GetAllExpensesAsync()
    {
        // Limit to 100 most recent expenses for performance
        var expenses = await _context.Expenses
            .Include(e => e.Category)
            .OrderByDescending(e => e.ExpenseDate)
            .Take(100)
            .AsNoTracking()
            .ToListAsync();

        return expenses.Select(MapToListDto);
    }

    public async Task<IEnumerable<ExpenseListDto>> GetExpensesByDateRangeAsync(DateTime startDate, DateTime endDate)
    {
        var expenses = await _context.Expenses
            .Include(e => e.Category)
            .Where(e => e.ExpenseDate >= startDate && e.ExpenseDate <= endDate)
            .OrderByDescending(e => e.ExpenseDate)
            .Take(500)
            .AsNoTracking()
            .ToListAsync();

        return expenses.Select(MapToListDto);
    }

    public async Task<IEnumerable<ExpenseListDto>> GetExpensesByCategoryAsync(int categoryId)
    {
        var expenses = await _context.Expenses
            .Include(e => e.Category)
            .Where(e => e.CategoryId == categoryId)
            .OrderByDescending(e => e.ExpenseDate)
            .Take(500)
            .AsNoTracking()
            .ToListAsync();

        return expenses.Select(MapToListDto);
    }

    public async Task<ExpenseDetailsDto?> GetExpenseByIdAsync(int id)
    {
        var expense = await _context.Expenses
            .Include(e => e.Category)
            .FirstOrDefaultAsync(e => e.Id == id);

        if (expense == null) return null;

        return new ExpenseDetailsDto
        {
            Id = expense.Id,
            Title = expense.Title,
            Description = expense.Description,
            Amount = expense.Amount,
            CategoryId = expense.CategoryId,
            CategoryName = expense.Category.Name,
            ExpenseDate = expense.ExpenseDate,
            PaymentMethod = expense.PaymentMethod,
            PaymentMethodName = expense.PaymentMethod.ToString(),
            Status = expense.Status,
            StatusName = expense.Status.ToString(),
            Reference = expense.Reference,
            ReceiptUrl = expense.ReceiptUrl,
            Vendor = expense.Vendor,
            ApprovedBy = expense.ApprovedBy,
            ApprovedDate = expense.ApprovedDate,
            CreatedBy = expense.CreatedBy,
            CreatedAt = expense.CreatedAt
        };
    }

    public async Task<int> CreateExpenseAsync(ExpenseCreateDto dto, string createdBy)
    {
        var expense = new Expense
        {
            Title = dto.Title,
            Description = dto.Description,
            Amount = dto.Amount,
            CategoryId = dto.CategoryId,
            ExpenseDate = dto.ExpenseDate,
            PaymentMethod = dto.PaymentMethod,
            Reference = dto.Reference,
            ReceiptUrl = dto.ReceiptUrl,
            Vendor = dto.Vendor,
            Status = ExpenseStatus.Pending,
            CreatedBy = createdBy,
            CreatedAt = DateTime.UtcNow
        };

        _context.Expenses.Add(expense);
        await _context.SaveChangesAsync();
        return expense.Id;
    }

    public async Task<bool> UpdateExpenseAsync(ExpenseUpdateDto dto)
    {
        var expense = await _context.Expenses.FindAsync(dto.Id);
        if (expense == null) return false;

        expense.Title = dto.Title;
        expense.Description = dto.Description;
        expense.Amount = dto.Amount;
        expense.CategoryId = dto.CategoryId;
        expense.ExpenseDate = dto.ExpenseDate;
        expense.PaymentMethod = dto.PaymentMethod;
        expense.Reference = dto.Reference;
        expense.ReceiptUrl = dto.ReceiptUrl;
        expense.Vendor = dto.Vendor;
        expense.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<bool> ApproveExpenseAsync(int id, string approvedBy)
    {
        var expense = await _context.Expenses.FindAsync(id);
        if (expense == null) return false;

        expense.Status = ExpenseStatus.Approved;
        expense.ApprovedBy = approvedBy;
        expense.ApprovedDate = DateTime.UtcNow;
        expense.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<bool> RejectExpenseAsync(int id, string rejectedBy)
    {
        var expense = await _context.Expenses.FindAsync(id);
        if (expense == null) return false;

        expense.Status = ExpenseStatus.Rejected;
        expense.ApprovedBy = rejectedBy;
        expense.ApprovedDate = DateTime.UtcNow;
        expense.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<bool> DeleteExpenseAsync(int id)
    {
        var expense = await _context.Expenses.FindAsync(id);
        if (expense == null) return false;

        _context.Expenses.Remove(expense);
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<decimal> GetTotalExpensesByMonthAsync(int month, int year)
    {
        return await _context.Expenses
            .Where(e => e.ExpenseDate.Month == month && e.ExpenseDate.Year == year)
            .SumAsync(e => e.Amount);
    }

    public async Task<IEnumerable<ExpenseByCategoryDto>> GetExpensesByCategoryReportAsync(int month, int year)
    {
        var expenses = await _context.Expenses
            .Include(e => e.Category)
            .Where(e => e.ExpenseDate.Month == month && e.ExpenseDate.Year == year)
            .GroupBy(e => new { e.CategoryId, e.Category.Name })
            .Select(g => new ExpenseByCategoryDto
            {
                CategoryName = g.Key.Name,
                Amount = g.Sum(e => e.Amount),
                Count = g.Count()
            })
            .ToListAsync();

        var total = expenses.Sum(e => e.Amount);
        foreach (var exp in expenses)
        {
            exp.Percentage = total > 0 ? Math.Round((exp.Amount / total) * 100, 2) : 0;
        }

        return expenses.OrderByDescending(e => e.Amount);
    }

    public async Task<Dictionary<string, decimal>> GetExpenseTotalsByCategoryAsync(DateTime startDate, DateTime endDate)
    {
        if (startDate > endDate)
        {
            (startDate, endDate) = (endDate, startDate);
        }

        var totals = await _context.Expenses
            .Where(e => e.ExpenseDate >= startDate && e.ExpenseDate <= endDate)
            .GroupBy(e => new { e.CategoryId, e.Category.Name })
            .Select(g => new { g.Key.Name, Amount = g.Sum(e => e.Amount) })
            .OrderByDescending(x => x.Amount)
            .ToListAsync();

        return totals.ToDictionary(x => x.Name, x => x.Amount);
    }

    private static ExpenseListDto MapToListDto(Expense e) => new()
    {
        Id = e.Id,
        Title = e.Title,
        Amount = e.Amount,
        CategoryName = e.Category.Name,
        ExpenseDate = e.ExpenseDate,
        PaymentMethodName = e.PaymentMethod.ToString(),
        StatusName = e.Status.ToString(),
        Vendor = e.Vendor,
        CreatedBy = e.CreatedBy
    };
}

public class IncomeService : IIncomeService
{
    private readonly ApplicationDbContext _context;

    public IncomeService(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<IncomeListDto>> GetAllIncomesAsync()
    {
        // Limit to 100 most recent incomes for performance
        var incomes = await _context.Incomes
            .OrderByDescending(i => i.IncomeDate)
            .Take(100)
            .AsNoTracking()
            .ToListAsync();

        return incomes.Select(MapToListDto);
    }

    public async Task<IEnumerable<IncomeListDto>> GetIncomesByDateRangeAsync(DateTime startDate, DateTime endDate)
    {
        var incomes = await _context.Incomes
            .Where(i => i.IncomeDate >= startDate && i.IncomeDate <= endDate)
            .OrderByDescending(i => i.IncomeDate)
            .Take(500)
            .AsNoTracking()
            .ToListAsync();

        return incomes.Select(MapToListDto);
    }

    public async Task<IEnumerable<IncomeListDto>> GetIncomesByTypeAsync(int type)
    {
        var incomes = await _context.Incomes
            .Where(i => (int)i.Type == type)
            .OrderByDescending(i => i.IncomeDate)
            .Take(500)
            .AsNoTracking()
            .ToListAsync();

        return incomes.Select(MapToListDto);
    }

    public async Task<IncomeDetailsDto?> GetIncomeByIdAsync(int id)
    {
        var income = await _context.Incomes.FindAsync(id);
        if (income == null) return null;

        return new IncomeDetailsDto
        {
            Id = income.Id,
            Title = income.Title,
            Description = income.Description,
            Amount = income.Amount,
            Type = income.Type,
            TypeName = income.Type.ToString(),
            IncomeDate = income.IncomeDate,
            PaymentMethod = income.PaymentMethod,
            PaymentMethodName = income.PaymentMethod.ToString(),
            Reference = income.Reference,
            ReservationId = income.ReservationId,
            CreatedBy = income.CreatedBy,
            CreatedAt = income.CreatedAt
        };
    }

    public async Task<int> CreateIncomeAsync(IncomeCreateDto dto, string createdBy)
    {
        var income = new Income
        {
            Title = dto.Title,
            Description = dto.Description,
            Amount = dto.Amount,
            Type = dto.Type,
            IncomeDate = dto.IncomeDate,
            PaymentMethod = dto.PaymentMethodEnum,
            Reference = dto.Reference ?? dto.ReferenceNumber,
            ReservationId = dto.ReservationId,
            CreatedBy = createdBy,
            CreatedAt = DateTime.UtcNow
        };

        _context.Incomes.Add(income);
        await _context.SaveChangesAsync();
        return income.Id;
    }

    public async Task<bool> UpdateIncomeAsync(IncomeUpdateDto dto)
    {
        var income = await _context.Incomes.FindAsync(dto.Id);
        if (income == null) return false;

        income.Title = dto.Title;
        income.Description = dto.Description;
        income.Amount = dto.Amount;
        income.Type = dto.Type;
        income.IncomeDate = dto.IncomeDate;
        income.PaymentMethod = dto.PaymentMethodEnum;
        income.Reference = dto.Reference ?? dto.ReferenceNumber;
        income.ReservationId = dto.ReservationId;
        income.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<bool> DeleteIncomeAsync(int id)
    {
        var income = await _context.Incomes.FindAsync(id);
        if (income == null) return false;

        _context.Incomes.Remove(income);
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<decimal> GetTotalIncomeByMonthAsync(int month, int year)
    {
        return await _context.Incomes
            .Where(i => i.IncomeDate.Month == month && i.IncomeDate.Year == year)
            .SumAsync(i => i.Amount);
    }

    public async Task<IEnumerable<IncomeByTypeDto>> GetIncomeByTypeReportAsync(int month, int year)
    {
        var incomes = await _context.Incomes
            .Where(i => i.IncomeDate.Month == month && i.IncomeDate.Year == year)
            .GroupBy(i => i.Type)
            .Select(g => new IncomeByTypeDto
            {
                TypeName = g.Key.ToString(),
                Amount = g.Sum(i => i.Amount),
                Count = g.Count()
            })
            .ToListAsync();

        var total = incomes.Sum(i => i.Amount);
        foreach (var inc in incomes)
        {
            inc.Percentage = total > 0 ? Math.Round((inc.Amount / total) * 100, 2) : 0;
        }

        return incomes.OrderByDescending(i => i.Amount);
    }

    public async Task<int> CreateIncomeFromReservationAsync(int reservationId, decimal amount, string createdBy)
    {
        var reservation = await _context.Reservations
            .Include(r => r.Room)
            .FirstOrDefaultAsync(r => r.Id == reservationId);

        if (reservation == null) return 0;

        var income = new Income
        {
            Title = $"Room Booking - {reservation.Room.RoomNumber}",
            Description = $"Reservation #{reservationId}",
            Amount = amount,
            Type = IncomeType.RoomBooking,
            IncomeDate = DateTime.UtcNow,
            PaymentMethod = PaymentMethod.Cash,
            ReservationId = reservationId,
            CreatedBy = createdBy,
            CreatedAt = DateTime.UtcNow
        };

        _context.Incomes.Add(income);
        await _context.SaveChangesAsync();
        return income.Id;
    }

    private static IncomeListDto MapToListDto(Income i) => new()
    {
        Id = i.Id,
        Title = i.Title,
        Amount = i.Amount,
        TypeName = i.Type.ToString(),
        IncomeDate = i.IncomeDate,
        PaymentMethodName = i.PaymentMethod.ToString(),
        Reference = i.Reference,
        ReservationId = i.ReservationId,
        CreatedBy = i.CreatedBy
    };
}

public class FinancialReportService : IFinancialReportService
{
    private readonly ApplicationDbContext _context;

    public FinancialReportService(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<FinancialSummaryDto> GetFinancialSummaryAsync(int month, int year)
    {
        month = Math.Clamp(month, 1, 12);
        if (year < 2000 || year > 2100)
        {
            year = DateTime.Today.Year;
        }

        var prevMonth = month == 1 ? 12 : month - 1;
        var prevYear = month == 1 ? year - 1 : year;
        var minYear = Math.Min(year, prevYear);
        var maxYear = Math.Max(year, prevYear);

        if (minYear < 1)
        {
            minYear = 1;
        }

        if (maxYear < 1)
        {
            maxYear = 1;
        }

        var incomeTotals = await GetIncomeTotalsAsync(minYear, maxYear);
        var expenseTotals = await GetExpenseTotalsAsync(minYear, maxYear);
        var salaryTotals = await GetSalaryTotalsAsync(minYear, maxYear);

        var totalIncome = GetMonthlyTotal(incomeTotals, year, month);
        var totalExpenses = GetMonthlyTotal(expenseTotals, year, month);
        var totalSalaries = GetMonthlyTotal(salaryTotals, year, month);

        var prevIncome = GetMonthlyTotal(incomeTotals, prevYear, prevMonth);
        var prevExpenses = GetMonthlyTotal(expenseTotals, prevYear, prevMonth);

        var incomeChange = prevIncome > 0 ? ((totalIncome - prevIncome) / prevIncome) * 100 : 0;
        var expenseChange = prevExpenses > 0 ? ((totalExpenses - prevExpenses) / prevExpenses) * 100 : 0;

        var pendingExpenses = await _context.Expenses
            .Where(e => e.Status == ExpenseStatus.Pending && e.ExpenseDate.Month == month && e.ExpenseDate.Year == year)
            .SumAsync(e => e.Amount);

        var pendingSalaries = await _context.Salaries
            .Where(s => s.Status != SalaryStatus.Paid && s.Month == month && s.Year == year)
            .SumAsync(s => s.NetSalary);

        return new FinancialSummaryDto
        {
            TotalIncome = totalIncome,
            TotalExpenses = totalExpenses,
            TotalSalaries = totalSalaries,
            NetProfit = totalIncome - totalExpenses - totalSalaries,
            PendingExpenses = pendingExpenses,
            PendingSalaries = pendingSalaries,
            IncomeChangePercent = Math.Round(incomeChange, 2),
            ExpenseChangePercent = Math.Round(expenseChange, 2)
        };
    }

    public async Task<IEnumerable<MonthlyFinancialDto>> GetMonthlyFinancialsAsync(int year)
    {
        if (year < 2000 || year > 2100)
        {
            year = DateTime.Today.Year;
        }

        var incomeTotals = await GetIncomeTotalsAsync(year, year);
        var expenseTotals = await GetExpenseTotalsAsync(year, year);
        var salaryTotals = await GetSalaryTotalsAsync(year, year);

        var results = new List<MonthlyFinancialDto>();

        for (var month = 1; month <= 12; month++)
        {
            var income = GetMonthlyTotal(incomeTotals, year, month);
            var expenses = GetMonthlyTotal(expenseTotals, year, month);
            var salaries = GetMonthlyTotal(salaryTotals, year, month);

            results.Add(new MonthlyFinancialDto
            {
                Month = month,
                Year = year,
                MonthName = new DateTime(year, month, 1).ToString("MMMM", CultureInfo.CurrentCulture),
                Income = income,
                Expenses = expenses,
                Salaries = salaries,
                Profit = income - expenses - salaries
            });
        }

        return results;
    }

    public async Task<FinancialSummaryDto> GetTodaySummaryAsync()
    {
        var today = DateTime.Today;

        var todayIncome = await _context.Incomes
            .Where(i => i.IncomeDate.Date == today)
            .SumAsync(i => i.Amount);

        var todayExpenses = await _context.Expenses
            .Where(e => e.ExpenseDate.Date == today)
            .SumAsync(e => e.Amount);

        return new FinancialSummaryDto
        {
            TotalIncome = todayIncome,
            TotalExpenses = todayExpenses,
            NetProfit = todayIncome - todayExpenses
        };
    }

    public async Task GenerateMonthlyReportAsync(int month, int year, string generatedBy)
    {
        var summary = await GetFinancialSummaryAsync(month, year);

        var report = new FinancialReport
        {
            Type = ReportType.Monthly,
            Month = month,
            Year = year,
            TotalIncome = summary.TotalIncome,
            TotalExpenses = summary.TotalExpenses,
            TotalSalaries = summary.TotalSalaries,
            NetProfit = summary.NetProfit,
            GeneratedBy = generatedBy,
            GeneratedAt = DateTime.UtcNow
        };

        _context.FinancialReports.Add(report);
        await _context.SaveChangesAsync();
    }

    private async Task<Dictionary<(int Year, int Month), decimal>> GetIncomeTotalsAsync(int minYear, int maxYear)
    {
        return await _context.Incomes
            .Where(i => i.IncomeDate.Year >= minYear && i.IncomeDate.Year <= maxYear)
            .GroupBy(i => new { i.IncomeDate.Year, i.IncomeDate.Month })
            .Select(g => new { g.Key.Year, g.Key.Month, Amount = g.Sum(i => i.Amount) })
            .ToDictionaryAsync(x => (x.Year, x.Month), x => x.Amount);
    }

    private async Task<Dictionary<(int Year, int Month), decimal>> GetExpenseTotalsAsync(int minYear, int maxYear)
    {
        return await _context.Expenses
            .Where(e => e.ExpenseDate.Year >= minYear && e.ExpenseDate.Year <= maxYear)
            .GroupBy(e => new { e.ExpenseDate.Year, e.ExpenseDate.Month })
            .Select(g => new { g.Key.Year, g.Key.Month, Amount = g.Sum(e => e.Amount) })
            .ToDictionaryAsync(x => (x.Year, x.Month), x => x.Amount);
    }

    private async Task<Dictionary<(int Year, int Month), decimal>> GetSalaryTotalsAsync(int minYear, int maxYear)
    {
        return await _context.Salaries
            .Where(s => s.Year >= minYear && s.Year <= maxYear && s.Status == SalaryStatus.Paid)
            .GroupBy(s => new { s.Year, s.Month })
            .Select(g => new { g.Key.Year, g.Key.Month, Amount = g.Sum(s => s.NetSalary) })
            .ToDictionaryAsync(x => (x.Year, x.Month), x => x.Amount);
    }

    private static decimal GetMonthlyTotal(Dictionary<(int Year, int Month), decimal> totals, int year, int month)
    {
        if (year <= 0 || month is < 1 or > 12)
        {
            return 0;
        }

        return totals.TryGetValue((year, month), out var value) ? value : 0;
    }
}
