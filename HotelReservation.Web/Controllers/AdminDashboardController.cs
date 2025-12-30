using HotelReservation.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;

namespace HotelReservation.Web.Controllers;

[Authorize(Roles = "Admin")]
public class AdminDashboardController : Controller
{
    private readonly IDashboardService _dashboardService;
    private readonly IStringLocalizer<SharedResource> _localizer;

    public AdminDashboardController(
        IDashboardService dashboardService,
        IStringLocalizer<SharedResource> localizer)
    {
        _dashboardService = dashboardService;
        _localizer = localizer;
    }

    public async Task<IActionResult> Index()
    {
        // DbContext is not thread-safe, so run queries sequentially
        var summary = await _dashboardService.GetDashboardSummaryAsync();
        ViewBag.RecentActivities = await _dashboardService.GetRecentActivitiesAsync(10);
        ViewBag.UpcomingCheckIns = await _dashboardService.GetUpcomingCheckInsAsync(7);
        ViewBag.UpcomingCheckOuts = await _dashboardService.GetUpcomingCheckOutsAsync(7);
        ViewBag.TopRooms = await _dashboardService.GetTopRoomsAsync(5);

        return View(summary);
    }

    [HttpGet]
    public async Task<IActionResult> GetFinancialChartData(int year = 0)
    {
        if (year == 0) year = DateTime.Today.Year;
        
        var data = await _dashboardService.GetFinancialChartDataAsync(year);
        
        // Format data for Chart.js
        return Json(new {
            labels = data.MonthlyData.Select(m => m.MonthName).ToArray(),
            income = data.MonthlyData.Select(m => m.Income).ToArray(),
            expenses = data.MonthlyData.Select(m => m.Expenses).ToArray(),
            profit = data.MonthlyData.Select(m => m.Profit).ToArray()
        });
    }

    [HttpGet]
    public async Task<IActionResult> GetOccupancyChartData(int year)
    {
        var data = await _dashboardService.GetOccupancyChartDataAsync(year);
        return Json(data);
    }

    [HttpGet]
    public async Task<IActionResult> GetOccupancyReport(DateTime startDate, DateTime endDate)
    {
        var data = await _dashboardService.GetOccupancyReportAsync(startDate, endDate);
        return Json(data);
    }

    public async Task<IActionResult> OccupancyReport(DateTime? startDate, DateTime? endDate)
    {
        startDate ??= DateTime.Today.AddDays(-30);
        endDate ??= DateTime.Today;

        var report = await _dashboardService.GetOccupancyReportAsync(startDate.Value, endDate.Value);

        ViewBag.StartDate = startDate;
        ViewBag.EndDate = endDate;

        return View(report);
    }
}
