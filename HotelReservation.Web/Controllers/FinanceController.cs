using HotelReservation.Core.DTOs;
using HotelReservation.Core.Models;
using HotelReservation.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Extensions.Localization;

namespace HotelReservation.Web.Controllers;

[Authorize(Roles = "Admin")]
public class FinanceController : Controller
{
    private readonly IExpenseCategoryService _expenseCategoryService;
    private readonly IExpenseService _expenseService;
    private readonly IIncomeService _incomeService;
    private readonly IFinancialReportService _reportService;
    private readonly IStringLocalizer<SharedResource> _localizer;

    public FinanceController(
        IExpenseCategoryService expenseCategoryService,
        IExpenseService expenseService,
        IIncomeService incomeService,
        IFinancialReportService reportService,
        IStringLocalizer<SharedResource> localizer)
    {
        _expenseCategoryService = expenseCategoryService;
        _expenseService = expenseService;
        _incomeService = incomeService;
        _reportService = reportService;
        _localizer = localizer;
    }

    #region Overview

    public async Task<IActionResult> Index(int? month, int? year)
    {
        month ??= DateTime.Today.Month;
        year ??= DateTime.Today.Year;

        var summary = await _reportService.GetFinancialSummaryAsync(month.Value, year.Value);
        var monthlyData = await _reportService.GetMonthlyFinancialsAsync(year.Value);

        ViewBag.SelectedMonth = month;
        ViewBag.SelectedYear = year;
        ViewBag.Months = GetMonthSelectList(month.Value);
        ViewBag.Years = GetYearSelectList(year.Value);
        ViewBag.MonthlyData = monthlyData;

        return View(summary);
    }

    #endregion

    #region Expense Categories

    public async Task<IActionResult> ExpenseCategories()
    {
        var categories = await _expenseCategoryService.GetAllCategoriesAsync();
        return View(categories);
    }

    public IActionResult CreateExpenseCategory()
    {
        return View();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> CreateExpenseCategory(ExpenseCategoryCreateDto dto)
    {
        if (!ModelState.IsValid)
            return View(dto);

        await _expenseCategoryService.CreateCategoryAsync(dto);
        TempData["Success"] = _localizer["CategoryCreated"].Value;
        return RedirectToAction(nameof(ExpenseCategories));
    }

    public async Task<IActionResult> EditExpenseCategory(int id)
    {
        var category = await _expenseCategoryService.GetCategoryByIdAsync(id);
        if (category == null)
            return NotFound();

        var dto = new ExpenseCategoryUpdateDto
        {
            Id = category.Id,
            Name = category.Name,
            Description = category.Description,
            IconClass = category.IconClass,
            IsActive = category.IsActive
        };
        return View(dto);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> EditExpenseCategory(ExpenseCategoryUpdateDto dto)
    {
        if (!ModelState.IsValid)
            return View(dto);

        var result = await _expenseCategoryService.UpdateCategoryAsync(dto);
        if (!result)
        {
            TempData["Error"] = _localizer["UpdateFailed"].Value;
            return View(dto);
        }

        TempData["Success"] = _localizer["CategoryUpdated"].Value;
        return RedirectToAction(nameof(ExpenseCategories));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteExpenseCategory(int id)
    {
        var result = await _expenseCategoryService.DeleteCategoryAsync(id);
        if (!result)
            TempData["Error"] = _localizer["DeleteFailedHasItems"].Value;
        else
            TempData["Success"] = _localizer["CategoryDeleted"].Value;

        return RedirectToAction(nameof(ExpenseCategories));
    }

    #endregion

    #region Expenses

    public async Task<IActionResult> Expenses(int? categoryId, DateTime? startDate, DateTime? endDate)
    {
        IEnumerable<ExpenseListDto> expenses;
        
        if (categoryId.HasValue)
        {
            expenses = await _expenseService.GetExpensesByCategoryAsync(categoryId.Value);
        }
        else if (startDate.HasValue && endDate.HasValue)
        {
            expenses = await _expenseService.GetExpensesByDateRangeAsync(startDate.Value, endDate.Value);
        }
        else
        {
            // Show all expenses when no filter is applied
            expenses = await _expenseService.GetAllExpensesAsync();
        }

        var categories = await _expenseCategoryService.GetActiveCategoriesAsync();
        ViewBag.Categories = new SelectList(categories, "Id", "Name", categoryId);
        ViewBag.StartDate = startDate;
        ViewBag.EndDate = endDate;
        ViewBag.SelectedCategory = categoryId;

        return View(expenses);
    }

    public async Task<IActionResult> ExpenseDetails(int id)
    {
        var expense = await _expenseService.GetExpenseByIdAsync(id);
        if (expense == null)
            return NotFound();

        return View(expense);
    }

    public async Task<IActionResult> CreateExpense()
    {
        var categories = await _expenseCategoryService.GetActiveCategoriesAsync();
        ViewBag.Categories = new SelectList(categories, "Id", "Name");
        ViewBag.PaymentMethods = GetPaymentMethodSelectList();

        return View(new ExpenseCreateDto { ExpenseDate = DateTime.Today });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> CreateExpense(ExpenseCreateDto dto)
    {
        if (!ModelState.IsValid)
        {
            var categories = await _expenseCategoryService.GetActiveCategoriesAsync();
            ViewBag.Categories = new SelectList(categories, "Id", "Name", dto.CategoryId);
            ViewBag.PaymentMethods = GetPaymentMethodSelectList(dto.PaymentMethod);
            return View(dto);
        }

        await _expenseService.CreateExpenseAsync(dto, User.Identity?.Name ?? "System");
        TempData["Success"] = _localizer["ExpenseCreated"].Value;
        return RedirectToAction(nameof(Expenses));
    }

    public async Task<IActionResult> EditExpense(int id)
    {
        var expense = await _expenseService.GetExpenseByIdAsync(id);
        if (expense == null)
            return NotFound();

        var categories = await _expenseCategoryService.GetActiveCategoriesAsync();
        ViewBag.Categories = new SelectList(categories, "Id", "Name", expense.CategoryId);
        ViewBag.PaymentMethods = GetPaymentMethodSelectList(expense.PaymentMethod);

        var dto = new ExpenseUpdateDto
        {
            Id = expense.Id,
            Title = expense.Title,
            Description = expense.Description,
            Amount = expense.Amount,
            CategoryId = expense.CategoryId,
            ExpenseDate = expense.ExpenseDate,
            PaymentMethod = expense.PaymentMethod,
            Reference = expense.Reference,
            ReceiptUrl = expense.ReceiptUrl,
            Vendor = expense.Vendor
        };
        return View(dto);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> EditExpense(ExpenseUpdateDto dto)
    {
        if (!ModelState.IsValid)
        {
            var categories = await _expenseCategoryService.GetActiveCategoriesAsync();
            ViewBag.Categories = new SelectList(categories, "Id", "Name", dto.CategoryId);
            ViewBag.PaymentMethods = GetPaymentMethodSelectList(dto.PaymentMethod);
            return View(dto);
        }

        var result = await _expenseService.UpdateExpenseAsync(dto);
        if (!result)
        {
            TempData["Error"] = _localizer["UpdateFailed"].Value;
            return View(dto);
        }

        TempData["Success"] = _localizer["ExpenseUpdated"].Value;
        return RedirectToAction(nameof(Expenses));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ApproveExpense(int id)
    {
        var result = await _expenseService.ApproveExpenseAsync(id, User.Identity?.Name ?? "System");
        if (!result)
            TempData["Error"] = _localizer["OperationFailed"].Value;
        else
            TempData["Success"] = _localizer["ExpenseApproved"].Value;

        return RedirectToAction(nameof(Expenses));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> RejectExpense(int id)
    {
        var result = await _expenseService.RejectExpenseAsync(id, User.Identity?.Name ?? "System");
        if (!result)
            TempData["Error"] = _localizer["OperationFailed"].Value;
        else
            TempData["Success"] = _localizer["ExpenseRejected"].Value;

        return RedirectToAction(nameof(Expenses));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteExpense(int id)
    {
        var result = await _expenseService.DeleteExpenseAsync(id);
        if (!result)
            TempData["Error"] = _localizer["DeleteFailed"].Value;
        else
            TempData["Success"] = _localizer["ExpenseDeleted"].Value;

        return RedirectToAction(nameof(Expenses));
    }

    #endregion

    #region Income

    public async Task<IActionResult> Income(int? type, DateTime? startDate, DateTime? endDate)
    {
        IEnumerable<IncomeListDto> incomes;
        
        if (type.HasValue)
        {
            incomes = await _incomeService.GetIncomesByTypeAsync(type.Value);
        }
        else if (startDate.HasValue && endDate.HasValue)
        {
            incomes = await _incomeService.GetIncomesByDateRangeAsync(startDate.Value, endDate.Value);
        }
        else
        {
            // Show all incomes when no filter is applied
            incomes = await _incomeService.GetAllIncomesAsync();
        }

        ViewBag.IncomeTypes = GetIncomeTypeSelectList(type);
        ViewBag.StartDate = startDate;
        ViewBag.EndDate = endDate;
        ViewBag.SelectedType = type;

        return View(incomes);
    }

    public async Task<IActionResult> IncomeDetails(int id)
    {
        var income = await _incomeService.GetIncomeByIdAsync(id);
        if (income == null)
            return NotFound();

        return View(income);
    }

    public IActionResult CreateIncome()
    {
        ViewBag.IncomeTypes = GetIncomeTypeSelectList();
        ViewBag.PaymentMethods = GetPaymentMethodSelectList();

        return View(new IncomeCreateDto { IncomeDate = DateTime.Today });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> CreateIncome(IncomeCreateDto dto)
    {
        if (!ModelState.IsValid)
        {
            ViewBag.IncomeTypes = GetIncomeTypeSelectList((int)dto.Type);
            ViewBag.PaymentMethods = GetPaymentMethodSelectList(dto.PaymentMethodEnum);
            return View(dto);
        }

        await _incomeService.CreateIncomeAsync(dto, User.Identity?.Name ?? "System");
        TempData["Success"] = _localizer["IncomeCreated"].Value;
        return RedirectToAction(nameof(Income));
    }

    public async Task<IActionResult> EditIncome(int id)
    {
        var income = await _incomeService.GetIncomeByIdAsync(id);
        if (income == null)
            return NotFound();

        ViewBag.IncomeTypes = GetIncomeTypeSelectList((int)income.Type);
        ViewBag.PaymentMethods = GetPaymentMethodSelectList(income.PaymentMethod);

        var dto = new IncomeUpdateDto
        {
            Id = income.Id,
            Title = income.Title,
            Description = income.Description,
            Amount = income.Amount,
            Type = income.Type,
            IncomeDate = income.IncomeDate,
            PaymentMethodEnum = income.PaymentMethod,
            Reference = income.Reference,
            ReservationId = income.ReservationId
        };
        return View(dto);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> EditIncome(IncomeUpdateDto dto)
    {
        if (!ModelState.IsValid)
        {
            ViewBag.IncomeTypes = GetIncomeTypeSelectList((int)dto.Type);
            ViewBag.PaymentMethods = GetPaymentMethodSelectList(dto.PaymentMethodEnum);
            return View(dto);
        }

        var result = await _incomeService.UpdateIncomeAsync(dto);
        if (!result)
        {
            TempData["Error"] = _localizer["UpdateFailed"].Value;
            return View(dto);
        }

        TempData["Success"] = _localizer["IncomeUpdated"].Value;
        return RedirectToAction(nameof(Income));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteIncome(int id)
    {
        var result = await _incomeService.DeleteIncomeAsync(id);
        if (!result)
            TempData["Error"] = _localizer["DeleteFailed"].Value;
        else
            TempData["Success"] = _localizer["IncomeDeleted"].Value;

        return RedirectToAction(nameof(Income));
    }

    #endregion

    #region Reports

    public async Task<IActionResult> Reports(int? year, DateTime? startDate, DateTime? endDate)
    {
        year ??= DateTime.Today.Year;
        startDate ??= new DateTime(year.Value, 1, 1);
        endDate ??= new DateTime(year.Value, 12, 31);

        var monthlyData = await _reportService.GetMonthlyFinancialsAsync(year.Value);
        
        // Build the FinancialReportDto from monthly data
        var reportDto = new FinancialReportDto
        {
            StartDate = startDate.Value,
            EndDate = endDate.Value,
            TotalIncome = monthlyData.Sum(m => m.Income),
            TotalExpenses = monthlyData.Sum(m => m.Expenses) + monthlyData.Sum(m => m.Salaries),
            NetProfit = monthlyData.Sum(m => m.Profit),
            IncomeBySource = new Dictionary<string, decimal>
            {
                { "Reservations", monthlyData.Sum(m => m.Income) }
            },
            ExpensesByCategory = await _expenseService.GetExpenseTotalsByCategoryAsync(startDate.Value, endDate.Value),
            MonthlyData = monthlyData.ToDictionary(
                m => m.MonthName,
                m => new MonthlyDataDto { Income = m.Income, Expenses = m.Expenses + m.Salaries }
            )
        };

        ViewBag.SelectedYear = year;
        ViewBag.StartDate = startDate;
        ViewBag.EndDate = endDate;
        ViewBag.Years = GetYearSelectList(year.Value);

        return View(reportDto);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> GenerateReport(int month, int year)
    {
        await _reportService.GenerateMonthlyReportAsync(month, year, User.Identity?.Name ?? "System");
        TempData["Success"] = _localizer["ReportGenerated"].Value;
        return RedirectToAction(nameof(Reports), new { year });
    }

    #endregion

    #region Helper Methods

    private SelectList GetMonthSelectList(int selectedMonth)
    {
        var months = Enumerable.Range(1, 12).Select(m => new
        {
            Value = m,
            Text = new DateTime(2000, m, 1).ToString("MMMM")
        });
        return new SelectList(months, "Value", "Text", selectedMonth);
    }

    private SelectList GetYearSelectList(int selectedYear)
    {
        var currentYear = DateTime.Today.Year;
        var years = Enumerable.Range(currentYear - 5, 10).Select(y => new
        {
            Value = y,
            Text = y.ToString()
        });
        return new SelectList(years, "Value", "Text", selectedYear);
    }

    private SelectList GetPaymentMethodSelectList(PaymentMethod? selected = null)
    {
        var methods = Enum.GetValues<PaymentMethod>().Select(m => new
        {
            Value = (int)m,
            Text = m.ToString()
        });
        return new SelectList(methods, "Value", "Text", selected.HasValue ? (int)selected : null);
    }

    private SelectList GetIncomeTypeSelectList(int? selected = null)
    {
        var types = Enum.GetValues<IncomeType>().Select(t => new
        {
            Value = (int)t,
            Text = t.ToString()
        });
        return new SelectList(types, "Value", "Text", selected);
    }

    #endregion
}
