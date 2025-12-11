using HotelReservation.Core.DTOs;
using HotelReservation.Core.Enums;
using HotelReservation.Core.Models;
using HotelReservation.Data.Context;
using HotelReservation.Services.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;

namespace HotelReservation.Services;

public class DashboardService : IDashboardService
{
    private readonly ApplicationDbContext _context;
    private readonly IMemoryCache _cache;
    private static readonly TimeSpan CacheDuration = TimeSpan.FromMinutes(2); // Cache for 2 minutes

    public DashboardService(ApplicationDbContext context, IMemoryCache cache)
    {
        _context = context;
        _cache = cache;
    }

    public async Task<DashboardSummaryDto> GetDashboardSummaryAsync()
    {
        // Don't use cache - always fetch fresh data for dashboard
        var today = DateTime.Today;
        var currentMonth = today.Month;
        var currentYear = today.Year;

        // Sequential queries - DbContext is not thread-safe
        var reservationStats = await GetReservationStatsOptimizedAsync(today);
        var financialStats = await GetFinancialStatsOptimizedAsync(today, currentMonth, currentYear);
        var roomStats = await GetRoomStatsOptimizedAsync(today);
        var staffStats = await GetStaffStatsOptimizedAsync(today);
        var inventoryStats = await GetInventoryStatsOptimizedAsync();
        var pendingStats = await GetPendingStatsAsync(currentMonth, currentYear);
        var recentReservations = await GetRecentReservationsAsync(5);

        var summary = new DashboardSummaryDto
        {
            // Reservation Summary
            TotalReservations = reservationStats.TotalReservations,
            TodayCheckIns = reservationStats.TodayCheckIns,
            TodayCheckOuts = reservationStats.TodayCheckOuts,
            PendingReservations = reservationStats.PendingReservations,
            ConfirmedReservations = reservationStats.ConfirmedReservations,
            
            // Financial Summary
            TodayRevenue = financialStats.TodayRevenue,
            MonthlyRevenue = financialStats.MonthlyRevenue,
            MonthlyIncome = financialStats.MonthlyRevenue, // Same as MonthlyRevenue
            MonthlyExpenses = financialStats.MonthlyExpenses,
            NetProfit = financialStats.NetProfit,
            MonthlyProfit = financialStats.NetProfit,
            
            // Pending counts
            PendingExpenses = pendingStats.PendingExpenses,
            PendingSalaries = pendingStats.PendingSalaries,
            
            // Room Summary
            TotalRooms = roomStats.TotalRooms,
            AvailableRooms = roomStats.AvailableRooms,
            OccupiedRooms = roomStats.OccupiedRooms,
            OccupancyRate = roomStats.OccupancyRate,
            
            // Staff Summary
            TotalEmployees = staffStats.TotalEmployees,
            ActiveEmployees = staffStats.ActiveEmployees,
            TodayAttendance = staffStats.TodayAttendance,
            PresentToday = staffStats.TodayAttendance, // Same as TodayAttendance
            
            // Inventory Summary
            TotalInventoryItems = inventoryStats.TotalItems,
            LowStockItems = inventoryStats.LowStockItems,
            InventoryValue = inventoryStats.TotalValue,
            
            // Recent Reservations
            RecentReservations = recentReservations
        };

        return summary;
    }
    
    private async Task<List<RecentReservationDto>> GetRecentReservationsAsync(int count)
    {
        return await _context.Reservations
            .AsNoTracking()
            .Include(r => r.User)
            .Include(r => r.Room)
            .OrderByDescending(r => r.CreatedAt)
            .Take(count)
            .Select(r => new RecentReservationDto
            {
                Id = r.Id,
                GuestName = r.User.FirstName + " " + r.User.LastName,
                RoomNumber = r.Room.RoomNumber,
                CheckInDate = r.CheckInDate,
                CheckOutDate = r.CheckOutDate,
                TotalAmount = r.TotalPrice,
                Status = r.Status.ToString()
            })
            .ToListAsync();
    }
    
    private async Task<(int PendingExpenses, int PendingSalaries)> GetPendingStatsAsync(int month, int year)
    {
        // Count pending/unpaid expenses (if there's a status field, otherwise count recent ones)
        var pendingExpenses = await _context.Expenses
            .CountAsync(e => e.ExpenseDate.Month == month && e.ExpenseDate.Year == year);
            
        // Count unpaid salaries for current month
        var pendingSalaries = await _context.Salaries
            .CountAsync(s => s.Month == month && s.Year == year && s.Status != SalaryStatus.Paid);
            
        return (pendingExpenses, pendingSalaries);
    }

    public async Task<FinancialChartDataDto> GetFinancialChartDataAsync(int year)
    {
        // Validate year - default to current year if invalid
        if (year < 1900 || year > 2100)
        {
            year = DateTime.Today.Year;
        }
        
        // Batch query - get all data in 3 sequential queries
        var startDate = new DateTime(year, 1, 1);
        var endDate = new DateTime(year, 12, 31);

        var incomes = await _context.Incomes
            .Where(i => i.IncomeDate >= startDate && i.IncomeDate <= endDate)
            .GroupBy(i => i.IncomeDate.Month)
            .Select(g => new { Month = g.Key, Amount = g.Sum(i => i.Amount) })
            .ToDictionaryAsync(x => x.Month, x => x.Amount);

        var expenses = await _context.Expenses
            .Where(e => e.ExpenseDate >= startDate && e.ExpenseDate <= endDate)
            .GroupBy(e => e.ExpenseDate.Month)
            .Select(g => new { Month = g.Key, Amount = g.Sum(e => e.Amount) })
            .ToDictionaryAsync(x => x.Month, x => x.Amount);

        var salaries = await _context.Salaries
            .Where(s => s.Year == year && s.Status == SalaryStatus.Paid)
            .GroupBy(s => s.Month)
            .Select(g => new { Month = g.Key, Amount = g.Sum(s => s.NetSalary) })
            .ToDictionaryAsync(x => x.Month, x => x.Amount);

        var monthlyData = new List<MonthlyChartDataDto>();
        for (int month = 1; month <= 12; month++)
        {
            var income = incomes.GetValueOrDefault(month, 0);
            var expense = expenses.GetValueOrDefault(month, 0);
            var salary = salaries.GetValueOrDefault(month, 0);
            var totalExpense = expense + salary;

            monthlyData.Add(new MonthlyChartDataDto
            {
                Month = month,
                MonthName = new DateTime(year, month, 1).ToString("MMM"),
                Income = income,
                Expenses = totalExpense,
                Profit = income - totalExpense
            });
        }

        return new FinancialChartDataDto
        {
            Year = year,
            MonthlyData = monthlyData,
            TotalIncome = monthlyData.Sum(m => m.Income),
            TotalExpenses = monthlyData.Sum(m => m.Expenses),
            TotalProfit = monthlyData.Sum(m => m.Profit)
        };
    }

    public async Task<OccupancyChartDataDto> GetOccupancyChartDataAsync(int year)
    {
        var totalRooms = await _context.Rooms.CountAsync(r => r.IsAvailable);
        
        // Get all reservations for the year in one query
        var startOfYear = new DateTime(year, 1, 1);
        var endOfYear = new DateTime(year, 12, 31);
        
        var reservations = await _context.Reservations
            .Where(r => r.CheckInDate <= endOfYear && r.CheckOutDate >= startOfYear &&
                       r.Status != ReservationStatus.Cancelled)
            .Select(r => new { r.CheckInDate, r.CheckOutDate })
            .ToListAsync();

        var monthlyData = new List<MonthlyOccupancyDto>();
        
        for (int month = 1; month <= 12; month++)
        {
            var daysInMonth = DateTime.DaysInMonth(year, month);
            var monthStart = new DateTime(year, month, 1);
            var monthEnd = monthStart.AddMonths(1).AddDays(-1);

            var totalBookedDays = reservations
                .Where(r => r.CheckInDate <= monthEnd && r.CheckOutDate >= monthStart)
                .Sum(r => 
                {
                    var start = r.CheckInDate < monthStart ? monthStart : r.CheckInDate;
                    var end = r.CheckOutDate > monthEnd ? monthEnd : r.CheckOutDate;
                    return (end - start).Days + 1;
                });

            var maxRoomDays = totalRooms * daysInMonth;
            var occupancyRate = maxRoomDays > 0 ? (decimal)totalBookedDays / maxRoomDays * 100 : 0;

            monthlyData.Add(new MonthlyOccupancyDto
            {
                Month = month,
                MonthName = new DateTime(year, month, 1).ToString("MMM"),
                OccupancyRate = Math.Round(occupancyRate, 1),
                BookedRoomDays = totalBookedDays,
                AvailableRoomDays = maxRoomDays - totalBookedDays
            });
        }

        return new OccupancyChartDataDto
        {
            Year = year,
            TotalRooms = totalRooms,
            MonthlyData = monthlyData,
            AverageOccupancy = monthlyData.Count > 0 ? monthlyData.Average(m => m.OccupancyRate) : 0
        };
    }

    public async Task<IEnumerable<RecentActivityDto>> GetRecentActivitiesAsync(int count = 10)
    {
        // Sequential queries - DbContext is not thread-safe
        var reservations = await _context.Reservations
            .AsNoTracking()
            .Include(r => r.Room)
            .Include(r => r.User)
            .OrderByDescending(r => r.CreatedAt)
            .Take(count)
            .Select(r => new RecentActivityDto
            {
                Type = "Reservation",
                Title = "New reservation - Room " + r.Room.RoomNumber,
                Description = r.User.FirstName + " " + r.User.LastName,
                Time = r.CreatedAt,
                Icon = "fas fa-calendar-check",
                IconColor = "primary"
            })
            .ToListAsync();

        var incomes = await _context.Incomes
            .AsNoTracking()
            .OrderByDescending(i => i.CreatedAt)
            .Take(count)
            .Select(i => new RecentActivityDto
            {
                Type = "Income",
                Title = "Income: " + i.Title,
                Description = "$" + i.Amount.ToString("N2"),
                Time = i.CreatedAt,
                Icon = "fas fa-arrow-down",
                IconColor = "success"
            })
            .ToListAsync();

        var expenses = await _context.Expenses
            .AsNoTracking()
            .OrderByDescending(e => e.CreatedAt)
            .Take(count)
            .Select(e => new RecentActivityDto
            {
                Type = "Expense",
                Title = "Expense: " + e.Title,
                Description = "$" + e.Amount.ToString("N2"),
                Time = e.CreatedAt,
                Icon = "fas fa-arrow-up",
                IconColor = "danger"
            })
            .ToListAsync();

        var inventoryTransactions = await _context.InventoryTransactions
            .AsNoTracking()
            .Include(t => t.Item)
            .OrderByDescending(t => t.TransactionDate)
            .Take(count)
            .Select(t => new RecentActivityDto
            {
                Type = "Inventory",
                Title = "Inventory: " + t.Item.Name,
                Description = t.Type.ToString() + ": " + t.Quantity + " units",
                Time = t.TransactionDate,
                Icon = "fas fa-box",
                IconColor = "info"
            })
            .ToListAsync();

        var activities = new List<RecentActivityDto>();
        activities.AddRange(reservations);
        activities.AddRange(incomes);
        activities.AddRange(expenses);
        activities.AddRange(inventoryTransactions);

        return activities.OrderByDescending(a => a.Time).Take(count);
    }

    public async Task<IEnumerable<TopRoomDto>> GetTopRoomsAsync(int count = 5)
    {
        var rooms = await _context.Rooms
            .AsNoTracking()
            .Include(r => r.Reservations)
            .Where(r => r.IsAvailable)
            .ToListAsync();

        var result = rooms.Select(r => new TopRoomDto
        {
            RoomId = r.Id,
            RoomNumber = r.RoomNumber,
            RoomType = r.Type.ToString(),
            TotalBookings = r.Reservations.Count(res => res.Status != ReservationStatus.Cancelled),
            TotalRevenue = r.Reservations
                .Where(res => res.Status != ReservationStatus.Cancelled)
                .Sum(res => res.TotalPrice),
            OccupancyDays = r.Reservations
                .Where(res => res.Status != ReservationStatus.Cancelled)
                .Sum(res => (res.CheckOutDate - res.CheckInDate).Days)
        })
        .OrderByDescending(r => r.TotalRevenue)
        .ThenByDescending(r => r.TotalBookings)
        .Take(count)
        .ToList();

        return result;
    }

    public async Task<IEnumerable<UpcomingCheckInOutDto>> GetUpcomingCheckInsAsync(int days = 7)
    {
        var today = DateTime.Today;
        var endDate = today.AddDays(days);

        return await _context.Reservations
            .AsNoTracking()
            .Include(r => r.Room)
            .Include(r => r.User)
            .Where(r => r.CheckInDate >= today && r.CheckInDate <= endDate &&
                       r.Status == ReservationStatus.Confirmed)
            .OrderBy(r => r.CheckInDate)
            .Select(r => new UpcomingCheckInOutDto
            {
                ReservationId = r.Id,
                GuestName = r.User.FirstName + " " + r.User.LastName,
                RoomNumber = r.Room.RoomNumber,
                Date = r.CheckInDate,
                Type = "Check-In"
            })
            .ToListAsync();
    }

    public async Task<IEnumerable<UpcomingCheckInOutDto>> GetUpcomingCheckOutsAsync(int days = 7)
    {
        var today = DateTime.Today;
        var endDate = today.AddDays(days);

        return await _context.Reservations
            .AsNoTracking()
            .Include(r => r.Room)
            .Include(r => r.User)
            .Where(r => r.CheckOutDate >= today && r.CheckOutDate <= endDate &&
                       r.Status == ReservationStatus.CheckedIn)
            .OrderBy(r => r.CheckOutDate)
            .Select(r => new UpcomingCheckInOutDto
            {
                ReservationId = r.Id,
                GuestName = r.User.FirstName + " " + r.User.LastName,
                RoomNumber = r.Room.RoomNumber,
                Date = r.CheckOutDate,
                Type = "Check-Out"
            })
            .ToListAsync();
    }

    // Optimized: Single query for all reservation stats
    private async Task<(int TotalReservations, int TodayCheckIns, int TodayCheckOuts, int PendingReservations, int ConfirmedReservations)> GetReservationStatsOptimizedAsync(DateTime today)
    {
        var stats = await _context.Reservations
            .GroupBy(r => 1) // Group all into one
            .Select(g => new
            {
                Total = g.Count(),
                // Count all reservations for today (Pending, Confirmed, or CheckedIn)
                TodayCheckIns = g.Count(r => r.CheckInDate.Date == today && 
                    (r.Status == ReservationStatus.Pending || 
                     r.Status == ReservationStatus.Confirmed || 
                     r.Status == ReservationStatus.CheckedIn)),
                TodayCheckOuts = g.Count(r => r.CheckOutDate.Date == today && 
                    (r.Status == ReservationStatus.CheckedIn || r.Status == ReservationStatus.CheckedOut)),
                Pending = g.Count(r => r.Status == ReservationStatus.Pending),
                Confirmed = g.Count(r => r.Status == ReservationStatus.Confirmed)
            })
            .FirstOrDefaultAsync();

        return stats != null 
            ? (stats.Total, stats.TodayCheckIns, stats.TodayCheckOuts, stats.Pending, stats.Confirmed)
            : (0, 0, 0, 0, 0);
    }

    // Optimized: Sequential financial queries (DbContext is not thread-safe)
    private async Task<(decimal TodayRevenue, decimal MonthlyRevenue, decimal MonthlyExpenses, decimal NetProfit)> GetFinancialStatsOptimizedAsync(DateTime today, int month, int year)
    {
        var monthStart = new DateTime(year, month, 1);
        var monthEnd = monthStart.AddMonths(1).AddDays(-1);

        // Sequential queries
        var todayRevenue = await _context.Incomes
            .Where(i => i.IncomeDate.Date == today)
            .SumAsync(i => (decimal?)i.Amount) ?? 0;

        var monthlyRevenue = await _context.Incomes
            .Where(i => i.IncomeDate >= monthStart && i.IncomeDate <= monthEnd)
            .SumAsync(i => (decimal?)i.Amount) ?? 0;

        var monthlyExpenses = await _context.Expenses
            .Where(e => e.ExpenseDate >= monthStart && e.ExpenseDate <= monthEnd)
            .SumAsync(e => (decimal?)e.Amount) ?? 0;

        var monthlySalaries = await _context.Salaries
            .Where(s => s.Month == month && s.Year == year && s.Status == SalaryStatus.Paid)
            .SumAsync(s => (decimal?)s.NetSalary) ?? 0;

        var totalExpenses = monthlyExpenses + monthlySalaries;
        var netProfit = monthlyRevenue - totalExpenses;

        return (todayRevenue, monthlyRevenue, totalExpenses, netProfit);
    }

    // Optimized: Sequential room stats queries
    private async Task<(int TotalRooms, int AvailableRooms, int OccupiedRooms, decimal OccupancyRate)> GetRoomStatsOptimizedAsync(DateTime today)
    {
        var totalRooms = await _context.Rooms.CountAsync(r => r.IsAvailable);
        
        // Count rooms with active reservations (Confirmed or CheckedIn) for today
        var occupiedRooms = await _context.Reservations
            .Where(r => r.CheckInDate <= today && r.CheckOutDate > today &&
                       (r.Status == ReservationStatus.Confirmed || 
                        r.Status == ReservationStatus.CheckedIn ||
                        r.Status == ReservationStatus.Pending))
            .Select(r => r.RoomId)
            .Distinct()
            .CountAsync();

        var availableRooms = totalRooms - occupiedRooms;
        var occupancyRate = totalRooms > 0 ? (decimal)occupiedRooms / totalRooms * 100 : 0;

        return (totalRooms, availableRooms, occupiedRooms, Math.Round(occupancyRate, 1));
    }

    // Optimized: Sequential staff stats queries
    private async Task<(int TotalEmployees, int ActiveEmployees, int TodayAttendance)> GetStaffStatsOptimizedAsync(DateTime today)
    {
        var employeeStats = await _context.Employees
            .GroupBy(e => 1)
            .Select(g => new
            {
                Total = g.Count(),
                Active = g.Count(e => e.IsActive)
            })
            .FirstOrDefaultAsync();

        var todayAttendance = await _context.Attendances
            .CountAsync(a => a.Date.Date == today && a.Status == AttendanceStatus.Present);

        return employeeStats != null
            ? (employeeStats.Total, employeeStats.Active, todayAttendance)
            : (0, 0, todayAttendance);
    }

    // Optimized: Single query for inventory stats
    private async Task<(int TotalItems, int LowStockItems, decimal TotalValue)> GetInventoryStatsOptimizedAsync()
    {
        var stats = await _context.InventoryItems
            .Where(i => i.IsActive)
            .GroupBy(i => 1)
            .Select(g => new
            {
                Total = g.Count(),
                LowStock = g.Count(i => i.Quantity <= i.ReorderLevel),
                TotalValue = g.Sum(i => i.Quantity * i.UnitPrice)
            })
            .FirstOrDefaultAsync();

        return stats != null
            ? (stats.Total, stats.LowStock, stats.TotalValue)
            : (0, 0, 0);
    }

    public async Task<IEnumerable<OccupancyReportDto>> GetOccupancyReportAsync(DateTime startDate, DateTime endDate)
    {
        var totalRooms = await _context.Rooms.CountAsync(r => r.IsAvailable);

        // Get all data needed in batch queries
        var reservations = await _context.Reservations
            .Where(r => r.CheckInDate <= endDate && r.CheckOutDate >= startDate &&
                       r.Status != ReservationStatus.Cancelled)
            .Select(r => new { r.CheckInDate, r.CheckOutDate, r.RoomId })
            .ToListAsync();

        var incomes = await _context.Incomes
            .Where(i => i.IncomeDate >= startDate && i.IncomeDate <= endDate)
            .GroupBy(i => i.IncomeDate.Date)
            .Select(g => new { Date = g.Key, Amount = g.Sum(i => i.Amount) })
            .ToDictionaryAsync(x => x.Date, x => x.Amount);

        var reports = new List<OccupancyReportDto>();

        for (var date = startDate; date <= endDate; date = date.AddDays(1))
        {
            var occupiedRoomIds = reservations
                .Where(r => r.CheckInDate <= date && r.CheckOutDate > date)
                .Select(r => r.RoomId)
                .Distinct()
                .Count();

            var dailyRevenue = incomes.GetValueOrDefault(date, 0);

            reports.Add(new OccupancyReportDto
            {
                Date = date,
                TotalRooms = totalRooms,
                OccupiedRooms = occupiedRoomIds,
                AvailableRooms = totalRooms - occupiedRoomIds,
                OccupancyRate = totalRooms > 0 ? Math.Round((decimal)occupiedRoomIds / totalRooms * 100, 1) : 0,
                Revenue = dailyRevenue
            });
        }

        return reports;
    }
}
