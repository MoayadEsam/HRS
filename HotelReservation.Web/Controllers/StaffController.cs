using HotelReservation.Core.DTOs;
using HotelReservation.Core.Models;
using HotelReservation.Data.Context;
using HotelReservation.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Localization;

namespace HotelReservation.Web.Controllers;

[Authorize(Roles = "Admin,Staff")]
public class StaffController : Controller
{
    private readonly IDepartmentService _departmentService;
    private readonly IEmployeeService _employeeService;
    private readonly ISalaryService _salaryService;
    private readonly IAttendanceService _attendanceService;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly ApplicationDbContext _context;
    private readonly IStringLocalizer<SharedResource> _localizer;

    public StaffController(
        IDepartmentService departmentService,
        IEmployeeService employeeService,
        ISalaryService salaryService,
        IAttendanceService attendanceService,
        UserManager<ApplicationUser> userManager,
        ApplicationDbContext context,
        IStringLocalizer<SharedResource> localizer)
    {
        _departmentService = departmentService;
        _employeeService = employeeService;
        _salaryService = salaryService;
        _attendanceService = attendanceService;
        _userManager = userManager;
        _context = context;
        _localizer = localizer;
    }

    #region Departments

    public async Task<IActionResult> Departments()
    {
        var departments = await _departmentService.GetAllDepartmentsAsync();
        return View(departments);
    }

    public IActionResult CreateDepartment()
    {
        return View();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> CreateDepartment(DepartmentCreateDto dto)
    {
        if (!ModelState.IsValid)
            return View(dto);

        await _departmentService.CreateDepartmentAsync(dto);
        TempData["Success"] = _localizer["DepartmentCreated"].Value;
        return RedirectToAction(nameof(Departments));
    }

    public async Task<IActionResult> EditDepartment(int id)
    {
        var department = await _departmentService.GetDepartmentByIdAsync(id);
        if (department == null)
            return NotFound();

        var dto = new DepartmentUpdateDto
        {
            Id = department.Id,
            Name = department.Name,
            Description = department.Description,
            IsActive = department.IsActive
        };
        return View(dto);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> EditDepartment(DepartmentUpdateDto dto)
    {
        if (!ModelState.IsValid)
            return View(dto);

        var result = await _departmentService.UpdateDepartmentAsync(dto);
        if (!result)
        {
            TempData["Error"] = _localizer["UpdateFailed"].Value;
            return View(dto);
        }

        TempData["Success"] = _localizer["DepartmentUpdated"].Value;
        return RedirectToAction(nameof(Departments));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteDepartment(int id)
    {
        var result = await _departmentService.DeleteDepartmentAsync(id);
        if (!result)
            TempData["Error"] = _localizer["DeleteFailed"].Value;
        else
            TempData["Success"] = _localizer["DepartmentDeleted"].Value;

        return RedirectToAction(nameof(Departments));
    }

    #endregion

    #region Employees

    public async Task<IActionResult> Employees(int? departmentId)
    {
        var employees = departmentId.HasValue
            ? await _employeeService.GetEmployeesByDepartmentAsync(departmentId.Value)
            : await _employeeService.GetAllEmployeesAsync();

        var departments = await _departmentService.GetActiveDepartmentsAsync();
        ViewBag.Departments = new SelectList(departments, "Id", "Name", departmentId);
        ViewBag.SelectedDepartment = departmentId;

        return View(employees);
    }

    public async Task<IActionResult> EmployeeDetails(int id)
    {
        var employee = await _employeeService.GetEmployeeByIdAsync(id);
        if (employee == null)
            return NotFound();

        return View(employee);
    }

    public async Task<IActionResult> CreateEmployee()
    {
        var departments = await _departmentService.GetActiveDepartmentsAsync();
        ViewBag.Departments = new SelectList(departments, "Id", "Name");
        return View();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> CreateEmployee(EmployeeCreateDto dto)
    {
        if (!ModelState.IsValid)
        {
            var departments = await _departmentService.GetActiveDepartmentsAsync();
            ViewBag.Departments = new SelectList(departments, "Id", "Name");
            return View(dto);
        }

        await _employeeService.CreateEmployeeAsync(dto);
        TempData["Success"] = _localizer["EmployeeCreated"].Value;
        return RedirectToAction(nameof(Employees));
    }

    public async Task<IActionResult> EditEmployee(int id)
    {
        var employee = await _employeeService.GetEmployeeByIdAsync(id);
        if (employee == null)
            return NotFound();

        var departments = await _departmentService.GetActiveDepartmentsAsync();
        ViewBag.Departments = new SelectList(departments, "Id", "Name", employee.DepartmentId);

        var dto = new EmployeeUpdateDto
        {
            Id = employee.Id,
            FirstName = employee.FirstName,
            LastName = employee.LastName,
            Email = employee.Email,
            Phone = employee.Phone,
            Address = employee.Address,
            DateOfBirth = employee.DateOfBirth,
            HireDate = employee.HireDate,
            Position = employee.Position,
            BaseSalary = employee.BaseSalary,
            DepartmentId = employee.DepartmentId,
            EmergencyContact = employee.EmergencyContact,
            EmergencyPhone = employee.EmergencyPhone,
            Notes = employee.Notes,
            IsActive = employee.IsActive,
            TerminationDate = employee.TerminationDate
        };
        return View(dto);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> EditEmployee(EmployeeUpdateDto dto)
    {
        if (!ModelState.IsValid)
        {
            var departments = await _departmentService.GetActiveDepartmentsAsync();
            ViewBag.Departments = new SelectList(departments, "Id", "Name", dto.DepartmentId);
            return View(dto);
        }

        var result = await _employeeService.UpdateEmployeeAsync(dto);
        if (!result)
        {
            TempData["Error"] = _localizer["UpdateFailed"].Value;
            var departments = await _departmentService.GetActiveDepartmentsAsync();
            ViewBag.Departments = new SelectList(departments, "Id", "Name", dto.DepartmentId);
            return View(dto);
        }

        TempData["Success"] = _localizer["EmployeeUpdated"].Value;
        return RedirectToAction(nameof(Employees));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> TerminateEmployee(int id)
    {
        var result = await _employeeService.TerminateEmployeeAsync(id, DateTime.Today);
        if (!result)
            TempData["Error"] = _localizer["OperationFailed"].Value;
        else
            TempData["Success"] = _localizer["EmployeeTerminated"].Value;

        return RedirectToAction(nameof(Employees));
    }

    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> CreateStaffAccount(int id)
    {
        var employee = await _context.Employees
            .Include(e => e.Department)
            .FirstOrDefaultAsync(e => e.Id == id);
            
        if (employee == null)
            return NotFound();

        if (!string.IsNullOrEmpty(employee.UserId))
        {
            TempData["Warning"] = _localizer["EmployeeAlreadyHasAccount"];
            return RedirectToAction(nameof(EmployeeDetails), new { id });
        }

        ViewBag.Employee = employee;
        return View(new StaffAccountCreateDto { EmployeeId = id });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> CreateStaffAccount(StaffAccountCreateDto dto)
    {
        var employee = await _context.Employees
            .Include(e => e.Department)
            .FirstOrDefaultAsync(e => e.Id == dto.EmployeeId);

        if (employee == null)
            return NotFound();

        if (!string.IsNullOrEmpty(employee.UserId))
        {
            TempData["Warning"] = _localizer["EmployeeAlreadyHasAccount"];
            return RedirectToAction(nameof(EmployeeDetails), new { id = dto.EmployeeId });
        }

        if (!ModelState.IsValid)
        {
            ViewBag.Employee = employee;
            return View(dto);
        }

        // Create the user account
        var user = new ApplicationUser
        {
            UserName = employee.Email,
            Email = employee.Email,
            FirstName = employee.FirstName,
            LastName = employee.LastName,
            EmailConfirmed = true
        };

        var result = await _userManager.CreateAsync(user, dto.Password);

        if (result.Succeeded)
        {
            // Assign Staff role
            await _userManager.AddToRoleAsync(user, "Staff");

            // Link employee to user
            employee.UserId = user.Id;
            
            // Generate employee code if not exists
            if (string.IsNullOrEmpty(employee.EmployeeCode))
            {
                employee.EmployeeCode = $"EMP-{employee.Id:D4}";
            }
            
            await _context.SaveChangesAsync();

            TempData["Success"] = _localizer["StaffAccountCreated"];
            return RedirectToAction(nameof(EmployeeDetails), new { id = dto.EmployeeId });
        }

        foreach (var error in result.Errors)
        {
            ModelState.AddModelError(string.Empty, error.Description);
        }

        ViewBag.Employee = employee;
        return View(dto);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> UnlinkStaffAccount(int id)
    {
        var employee = await _context.Employees.FindAsync(id);
        if (employee == null)
            return NotFound();

        if (!string.IsNullOrEmpty(employee.UserId))
        {
            var user = await _userManager.FindByIdAsync(employee.UserId);
            if (user != null)
            {
                // Remove from Staff role
                await _userManager.RemoveFromRoleAsync(user, "Staff");
            }
            
            employee.UserId = null;
            await _context.SaveChangesAsync();
            TempData["Success"] = _localizer["StaffAccountUnlinked"];
        }

        return RedirectToAction(nameof(EmployeeDetails), new { id });
    }

    #endregion

    #region Salaries

    public async Task<IActionResult> Salaries(int? month, int? year, int? employeeId, bool? isPaid)
    {
        month ??= DateTime.Today.Month;
        year ??= DateTime.Today.Year;

        var salaries = await _salaryService.GetSalariesByMonthAsync(month.Value, year.Value);
        
        // Apply filters
        if (employeeId.HasValue)
            salaries = salaries.Where(s => s.EmployeeId == employeeId.Value);
        if (isPaid.HasValue)
            salaries = salaries.Where(s => s.IsPaid == isPaid.Value);
        
        var employees = await _employeeService.GetActiveEmployeesAsync();

        ViewBag.SelectedMonth = month;
        ViewBag.SelectedYear = year;
        ViewBag.SelectedEmployeeId = employeeId;
        ViewBag.SelectedIsPaid = isPaid;
        ViewBag.Months = GetMonthSelectList(month.Value);
        ViewBag.Years = GetYearSelectList(year.Value);
        ViewBag.Employees = new SelectList(employees, "Id", "FullName", employeeId);

        return View(salaries);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ProcessSalaries(int month, int year)
    {
        var count = await _salaryService.GenerateMonthlySalariesAsync(month, year);
        TempData["Success"] = string.Format(_localizer["SalariesGenerated"].Value, count);
        return RedirectToAction(nameof(Salaries), new { month, year });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> PaySalary(int id)
    {
        var result = await _salaryService.MarkAsPaidAsync(id);
        if (!result)
            TempData["Error"] = _localizer["OperationFailed"].Value;
        else
            TempData["Success"] = _localizer["SalaryPaid"].Value;

        return RedirectToAction(nameof(Salaries));
    }

    public async Task<IActionResult> EditSalary(int id)
    {
        var salary = await _salaryService.GetSalaryByIdAsync(id);
        if (salary == null)
            return NotFound();

        var dto = new SalaryUpdateDto
        {
            Id = salary.Id,
            EmployeeId = salary.EmployeeId,
            Month = salary.Month,
            Year = salary.Year,
            Bonus = salary.Bonus,
            Deductions = salary.Deductions,
            Overtime = salary.Overtime
        };
        
        var employees = await _employeeService.GetActiveEmployeesAsync();
        ViewBag.Employees = new SelectList(employees, "Id", "FullName", salary.EmployeeId);
        ViewBag.SalaryDetails = salary;
        
        return View(dto);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> EditSalary(SalaryUpdateDto dto)
    {
        if (!ModelState.IsValid)
        {
            var employees = await _employeeService.GetActiveEmployeesAsync();
            ViewBag.Employees = new SelectList(employees, "Id", "FullName", dto.EmployeeId);
            return View(dto);
        }

        var result = await _salaryService.UpdateSalaryAsync(dto);
        if (!result)
        {
            TempData["Error"] = _localizer["UpdateFailed"].Value;
            return View(dto);
        }

        TempData["Success"] = _localizer["SalaryUpdated"].Value;
        return RedirectToAction(nameof(Salaries), new { month = dto.Month, year = dto.Year });
    }

    public async Task<IActionResult> CreateSalary()
    {
        var employees = await _employeeService.GetActiveEmployeesAsync();
        ViewBag.Employees = new SelectList(employees, "Id", "FullName");
        ViewBag.Months = GetMonthSelectList(DateTime.Today.Month);
        ViewBag.Years = GetYearSelectList(DateTime.Today.Year);

        return View(new SalaryCreateDto
        {
            Month = DateTime.Today.Month,
            Year = DateTime.Today.Year
        });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> CreateSalary(SalaryCreateDto dto)
    {
        if (!ModelState.IsValid)
        {
            var employees = await _employeeService.GetActiveEmployeesAsync();
            ViewBag.Employees = new SelectList(employees, "Id", "FullName");
            ViewBag.Months = GetMonthSelectList(dto.Month);
            ViewBag.Years = GetYearSelectList(dto.Year);
            return View(dto);
        }

        await _salaryService.CreateSalaryAsync(dto);
        TempData["Success"] = _localizer["SalaryCreated"].Value;
        return RedirectToAction(nameof(Salaries), new { month = dto.Month, year = dto.Year });
    }

    #endregion

    #region Attendance

    public async Task<IActionResult> Attendance(DateTime? date, int? employeeId, int? departmentId, DateTime? startDate, DateTime? endDate)
    {
        // Default to today's date for single day view
        date ??= DateTime.Today;
        startDate ??= date.Value;
        endDate ??= date.Value;

        var attendance = await _attendanceService.GetAttendanceByDateAsync(date.Value);
        
        // Get active employees for dropdowns
        var activeEmployees = await _employeeService.GetActiveEmployeesAsync();
        var departments = await _departmentService.GetActiveDepartmentsAsync();
        
        // Get today's check-ins to filter out already checked-in employees
        var todayAttendance = await _attendanceService.GetAttendanceByDateAsync(DateTime.Today);
        var checkedInEmployeeIds = todayAttendance
            .Where(a => a.CheckInTime.HasValue && !a.CheckOutTime.HasValue)
            .Select(a => a.EmployeeId)
            .ToList();
        var notCheckedInEmployees = activeEmployees.Where(e => !checkedInEmployeeIds.Contains(e.Id));
        var checkedInEmployees = activeEmployees.Where(e => checkedInEmployeeIds.Contains(e.Id));

        ViewBag.SelectedDate = date.Value;
        ViewBag.StartDate = startDate;
        ViewBag.EndDate = endDate;
        ViewBag.SelectedEmployeeId = employeeId;
        ViewBag.SelectedDepartmentId = departmentId;
        ViewBag.PresentCount = await _attendanceService.GetPresentCountTodayAsync();
        ViewBag.Employees = new SelectList(activeEmployees, "Id", "FullName", employeeId);
        ViewBag.ActiveEmployees = new SelectList(notCheckedInEmployees, "Id", "FullName");
        ViewBag.CheckedInEmployees = new SelectList(checkedInEmployees, "Id", "FullName");
        ViewBag.Departments = new SelectList(departments, "Id", "Name", departmentId);

        return View(attendance);
    }

    public async Task<IActionResult> EmployeeAttendance(int id, DateTime? startDate, DateTime? endDate)
    {
        startDate ??= DateTime.Today.AddDays(-30);
        endDate ??= DateTime.Today;

        var employee = await _employeeService.GetEmployeeByIdAsync(id);
        if (employee == null)
            return NotFound();

        var attendance = await _attendanceService.GetAttendanceByEmployeeAsync(id, startDate.Value, endDate.Value);

        ViewBag.Employee = employee;
        ViewBag.StartDate = startDate;
        ViewBag.EndDate = endDate;

        return View(attendance);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> CheckIn(int employeeId)
    {
        var result = await _attendanceService.CheckInAsync(employeeId);
        if (result == 0)
            TempData["Error"] = _localizer["CheckInFailed"].Value;
        else
            TempData["Success"] = _localizer["CheckInSuccess"].Value;

        return RedirectToAction(nameof(Attendance));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> CheckOut(int employeeId)
    {
        var result = await _attendanceService.CheckOutAsync(employeeId);
        if (!result)
            TempData["Error"] = _localizer["CheckOutFailed"].Value;
        else
            TempData["Success"] = _localizer["CheckOutSuccess"].Value;

        return RedirectToAction(nameof(Attendance));
    }

    public async Task<IActionResult> CreateAttendance()
    {
        var employees = await _employeeService.GetActiveEmployeesAsync();
        ViewBag.Employees = new SelectList(employees, "Id", "FullName");
        return View(new AttendanceCreateDto { Date = DateTime.Today });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> CreateAttendance(AttendanceCreateDto dto)
    {
        if (!ModelState.IsValid)
        {
            var employees = await _employeeService.GetActiveEmployeesAsync();
            ViewBag.Employees = new SelectList(employees, "Id", "FullName");
            return View(dto);
        }

        await _attendanceService.CreateAttendanceAsync(dto);
        TempData["Success"] = _localizer["AttendanceCreated"].Value;
        return RedirectToAction(nameof(Attendance), new { date = dto.Date });
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

    #endregion
}
