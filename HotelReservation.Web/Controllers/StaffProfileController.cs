using HotelReservation.Core.Enums;
using HotelReservation.Core.Models;
using HotelReservation.Data.Context;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Localization;

namespace HotelReservation.Web.Controllers;

[Authorize(Roles = "Staff")]
public class StaffProfileController : Controller
{
    private readonly ApplicationDbContext _context;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly IStringLocalizer<SharedResource> _localizer;

    public StaffProfileController(
        ApplicationDbContext context,
        UserManager<ApplicationUser> userManager,
        IStringLocalizer<SharedResource> localizer)
    {
        _context = context;
        _userManager = userManager;
        _localizer = localizer;
    }

    public async Task<IActionResult> Index()
    {
        var userId = _userManager.GetUserId(User);
        var employee = await _context.Employees
            .Include(e => e.Department)
            .FirstOrDefaultAsync(e => e.UserId == userId);

        if (employee == null)
        {
            ViewBag.Message = _localizer["YourAccountNotLinked"];
            return View("NotLinked");
        }

        // Get today's attendance
        var today = DateTime.Today;
        var todayAttendance = await _context.Attendances
            .FirstOrDefaultAsync(a => a.EmployeeId == employee.Id && a.Date.Date == today);

        // Get this month's attendance summary
        var monthStart = new DateTime(today.Year, today.Month, 1);
        var monthEnd = monthStart.AddMonths(1).AddDays(-1);
        var monthAttendance = await _context.Attendances
            .Where(a => a.EmployeeId == employee.Id && a.Date >= monthStart && a.Date <= monthEnd)
            .ToListAsync();

        // Get recent salaries
        var recentSalaries = await _context.Salaries
            .Where(s => s.EmployeeId == employee.Id)
            .OrderByDescending(s => s.Year)
            .ThenByDescending(s => s.Month)
            .Take(6)
            .ToListAsync();

        // Get upcoming reservations count (for staff to manage)
        var upcomingReservations = await _context.Reservations
            .CountAsync(r => r.CheckInDate >= today && r.CheckInDate <= today.AddDays(7) &&
                           r.Status == ReservationStatus.Confirmed);

        ViewBag.Employee = employee;
        ViewBag.TodayAttendance = todayAttendance;
        ViewBag.MonthAttendance = monthAttendance;
        ViewBag.PresentDays = monthAttendance.Count(a => a.Status == AttendanceStatus.Present);
        ViewBag.AbsentDays = monthAttendance.Count(a => a.Status == AttendanceStatus.Absent);
        ViewBag.LateDays = monthAttendance.Count(a => a.Status == AttendanceStatus.Late);
        ViewBag.LeaveDays = monthAttendance.Count(a => a.Status == AttendanceStatus.OnLeave);
        ViewBag.RecentSalaries = recentSalaries;
        ViewBag.UpcomingReservations = upcomingReservations;

        return View(employee);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> CheckIn()
    {
        var userId = _userManager.GetUserId(User);
        var employee = await _context.Employees.FirstOrDefaultAsync(e => e.UserId == userId);

        if (employee == null)
        {
            TempData["Error"] = _localizer["YourAccountNotLinked"];
            return RedirectToAction(nameof(Index));
        }

        var today = DateTime.Today;
        var existingAttendance = await _context.Attendances
            .FirstOrDefaultAsync(a => a.EmployeeId == employee.Id && a.Date.Date == today);

        if (existingAttendance != null)
        {
            TempData["Warning"] = _localizer["AlreadyCheckedIn"];
            return RedirectToAction(nameof(Index));
        }

        var now = DateTime.UtcNow;
        var isLate = now.Hour >= 9; // Consider late if after 9 AM

        var attendance = new Attendance
        {
            EmployeeId = employee.Id,
            Date = today,
            CheckInTime = now,
            Status = isLate ? AttendanceStatus.Late : AttendanceStatus.Present,
            Notes = isLate ? "Late check-in" : null
        };

        _context.Attendances.Add(attendance);
        await _context.SaveChangesAsync();

        TempData["Success"] = _localizer["CheckedInSuccessfully"];
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> CheckOut()
    {
        var userId = _userManager.GetUserId(User);
        var employee = await _context.Employees.FirstOrDefaultAsync(e => e.UserId == userId);

        if (employee == null)
        {
            TempData["Error"] = _localizer["YourAccountNotLinked"];
            return RedirectToAction(nameof(Index));
        }

        var today = DateTime.Today;
        var attendance = await _context.Attendances
            .FirstOrDefaultAsync(a => a.EmployeeId == employee.Id && a.Date.Date == today);

        if (attendance == null)
        {
            TempData["Error"] = _localizer["PleaseCheckInFirst"];
            return RedirectToAction(nameof(Index));
        }

        if (attendance.CheckOutTime.HasValue)
        {
            TempData["Warning"] = _localizer["AlreadyCheckedOut"];
            return RedirectToAction(nameof(Index));
        }

        attendance.CheckOutTime = DateTime.UtcNow;
        await _context.SaveChangesAsync();

        TempData["Success"] = _localizer["CheckedOutSuccessfully"];
        return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> Attendance()
    {
        var userId = _userManager.GetUserId(User);
        var employee = await _context.Employees.FirstOrDefaultAsync(e => e.UserId == userId);

        if (employee == null)
        {
            return View("NotLinked");
        }

        var attendances = await _context.Attendances
            .Where(a => a.EmployeeId == employee.Id)
            .OrderByDescending(a => a.Date)
            .Take(30)
            .ToListAsync();

        ViewBag.Employee = employee;
        return View(attendances);
    }

    public async Task<IActionResult> Salaries()
    {
        var userId = _userManager.GetUserId(User);
        var employee = await _context.Employees.FirstOrDefaultAsync(e => e.UserId == userId);

        if (employee == null)
        {
            return View("NotLinked");
        }

        var salaries = await _context.Salaries
            .Where(s => s.EmployeeId == employee.Id)
            .OrderByDescending(s => s.Year)
            .ThenByDescending(s => s.Month)
            .ToListAsync();

        ViewBag.Employee = employee;
        return View(salaries);
    }
}
