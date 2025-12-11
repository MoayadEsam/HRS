using AutoMapper;
using HotelReservation.Core.DTOs;
using HotelReservation.Core.Models;
using HotelReservation.Data.Context;
using HotelReservation.Services.Interfaces;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace HotelReservation.Services;

public class DepartmentService : IDepartmentService
{
    private readonly ApplicationDbContext _context;
    private readonly IMapper _mapper;

    public DepartmentService(ApplicationDbContext context, IMapper mapper)
    {
        _context = context;
        _mapper = mapper;
    }

    public async Task<IEnumerable<DepartmentListDto>> GetAllDepartmentsAsync()
    {
        var departments = await _context.Departments
            .Include(d => d.Employees)
            .OrderBy(d => d.Name)
            .ToListAsync();

        return departments.Select(d => new DepartmentListDto
        {
            Id = d.Id,
            Name = d.Name,
            Description = d.Description,
            IsActive = d.IsActive,
            EmployeeCount = d.Employees.Count(e => e.IsActive)
        });
    }

    public async Task<IEnumerable<DepartmentListDto>> GetActiveDepartmentsAsync()
    {
        var departments = await _context.Departments
            .Where(d => d.IsActive)
            .Include(d => d.Employees)
            .OrderBy(d => d.Name)
            .ToListAsync();

        return departments.Select(d => new DepartmentListDto
        {
            Id = d.Id,
            Name = d.Name,
            Description = d.Description,
            IsActive = d.IsActive,
            EmployeeCount = d.Employees.Count(e => e.IsActive)
        });
    }

    public async Task<DepartmentListDto?> GetDepartmentByIdAsync(int id)
    {
        var department = await _context.Departments
            .Include(d => d.Employees)
            .FirstOrDefaultAsync(d => d.Id == id);

        if (department == null) return null;

        return new DepartmentListDto
        {
            Id = department.Id,
            Name = department.Name,
            Description = department.Description,
            IsActive = department.IsActive,
            EmployeeCount = department.Employees.Count(e => e.IsActive)
        };
    }

    public async Task<int> CreateDepartmentAsync(DepartmentCreateDto dto)
    {
        var department = new Department
        {
            Name = dto.Name,
            Description = dto.Description,
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };

        _context.Departments.Add(department);
        await _context.SaveChangesAsync();
        return department.Id;
    }

    public async Task<bool> UpdateDepartmentAsync(DepartmentUpdateDto dto)
    {
        var department = await _context.Departments.FindAsync(dto.Id);
        if (department == null) return false;

        department.Name = dto.Name;
        department.Description = dto.Description;
        department.IsActive = dto.IsActive;

        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<bool> DeleteDepartmentAsync(int id)
    {
        var department = await _context.Departments
            .Include(d => d.Employees)
            .FirstOrDefaultAsync(d => d.Id == id);

        if (department == null) return false;
        if (department.Employees.Any(e => e.IsActive))
            return false; // Can't delete department with active employees

        _context.Departments.Remove(department);
        await _context.SaveChangesAsync();
        return true;
    }
}

public class EmployeeService : IEmployeeService
{
    private readonly ApplicationDbContext _context;
    private readonly IMapper _mapper;
    private readonly UserManager<ApplicationUser> _userManager;

    public EmployeeService(ApplicationDbContext context, IMapper mapper, UserManager<ApplicationUser> userManager)
    {
        _context = context;
        _mapper = mapper;
        _userManager = userManager;
    }

    public async Task<IEnumerable<EmployeeListDto>> GetAllEmployeesAsync()
    {
        var employees = await _context.Employees
            .Include(e => e.Department)
            .OrderBy(e => e.LastName)
            .ThenBy(e => e.FirstName)
            .ToListAsync();

        return employees.Select(MapToListDto);
    }

    public async Task<IEnumerable<EmployeeListDto>> GetEmployeesByDepartmentAsync(int departmentId)
    {
        var employees = await _context.Employees
            .Include(e => e.Department)
            .Where(e => e.DepartmentId == departmentId)
            .OrderBy(e => e.LastName)
            .ToListAsync();

        return employees.Select(MapToListDto);
    }

    public async Task<IEnumerable<EmployeeListDto>> GetActiveEmployeesAsync()
    {
        var employees = await _context.Employees
            .Include(e => e.Department)
            .Where(e => e.IsActive)
            .OrderBy(e => e.LastName)
            .ToListAsync();

        return employees.Select(MapToListDto);
    }

    public async Task<EmployeeDetailDto?> GetEmployeeByIdAsync(int id)
    {
        var employee = await _context.Employees
            .Include(e => e.Department)
            .FirstOrDefaultAsync(e => e.Id == id);

        if (employee == null) return null;

        return new EmployeeDetailDto
        {
            Id = employee.Id,
            FirstName = employee.FirstName,
            LastName = employee.LastName,
            FullName = employee.FullName,
            Email = employee.Email,
            Phone = employee.Phone,
            Address = employee.Address,
            DateOfBirth = employee.DateOfBirth,
            HireDate = employee.HireDate,
            TerminationDate = employee.TerminationDate,
            Position = employee.Position,
            BaseSalary = employee.BaseSalary,
            DepartmentId = employee.DepartmentId,
            DepartmentName = employee.Department.Name,
            EmergencyContact = employee.EmergencyContact,
            EmergencyPhone = employee.EmergencyPhone,
            Notes = employee.Notes,
            IsActive = employee.IsActive,
            CreatedAt = employee.CreatedAt
        };
    }

    public async Task<int> CreateEmployeeAsync(EmployeeCreateDto dto)
    {
        var employee = new Employee
        {
            FirstName = dto.FirstName,
            LastName = dto.LastName,
            Email = dto.Email,
            Phone = dto.Phone,
            Address = dto.Address,
            DateOfBirth = dto.DateOfBirth,
            HireDate = dto.HireDate,
            Position = dto.Position,
            BaseSalary = dto.BaseSalary,
            DepartmentId = dto.DepartmentId,
            EmergencyContact = dto.EmergencyContact,
            EmergencyPhone = dto.EmergencyPhone,
            Notes = dto.Notes,
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };

        _context.Employees.Add(employee);
        await _context.SaveChangesAsync();

        // Auto-create user account with default password "Staff@1234"
        if (!string.IsNullOrEmpty(dto.Email))
        {
            var existingUser = await _userManager.FindByEmailAsync(dto.Email);
            if (existingUser == null)
            {
                var user = new ApplicationUser
                {
                    UserName = dto.Email,
                    Email = dto.Email,
                    FirstName = dto.FirstName,
                    LastName = dto.LastName,
                    EmailConfirmed = true
                };

                var result = await _userManager.CreateAsync(user, "Staff@1234");
                if (result.Succeeded)
                {
                    await _userManager.AddToRoleAsync(user, "Staff");
                    employee.UserId = user.Id;
                    employee.EmployeeCode = $"EMP-{employee.Id:D4}";
                    await _context.SaveChangesAsync();
                }
            }
        }

        return employee.Id;
    }

    public async Task<bool> UpdateEmployeeAsync(EmployeeUpdateDto dto)
    {
        var employee = await _context.Employees.FindAsync(dto.Id);
        if (employee == null) return false;

        employee.FirstName = dto.FirstName;
        employee.LastName = dto.LastName;
        employee.Email = dto.Email;
        employee.Phone = dto.Phone;
        employee.Address = dto.Address;
        employee.DateOfBirth = dto.DateOfBirth;
        employee.HireDate = dto.HireDate;
        employee.TerminationDate = dto.TerminationDate;
        employee.Position = dto.Position;
        employee.BaseSalary = dto.BaseSalary;
        employee.DepartmentId = dto.DepartmentId;
        employee.EmergencyContact = dto.EmergencyContact;
        employee.EmergencyPhone = dto.EmergencyPhone;
        employee.Notes = dto.Notes;
        employee.IsActive = dto.IsActive;
        employee.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<bool> TerminateEmployeeAsync(int id, DateTime terminationDate)
    {
        var employee = await _context.Employees.FindAsync(id);
        if (employee == null) return false;

        employee.TerminationDate = terminationDate;
        employee.IsActive = false;
        employee.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<bool> DeleteEmployeeAsync(int id)
    {
        var employee = await _context.Employees.FindAsync(id);
        if (employee == null) return false;

        _context.Employees.Remove(employee);
        await _context.SaveChangesAsync();
        return true;
    }

    private static EmployeeListDto MapToListDto(Employee e) => new()
    {
        Id = e.Id,
        FullName = e.FullName,
        Email = e.Email,
        Phone = e.Phone,
        Position = e.Position,
        DepartmentName = e.Department.Name,
        BaseSalary = e.BaseSalary,
        HireDate = e.HireDate,
        IsActive = e.IsActive
    };
}

public class SalaryService : ISalaryService
{
    private readonly ApplicationDbContext _context;

    public SalaryService(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<SalaryListDto>> GetAllSalariesAsync()
    {
        var salaries = await _context.Salaries
            .Include(s => s.Employee)
            .ThenInclude(e => e.Department)
            .OrderByDescending(s => s.Year)
            .ThenByDescending(s => s.Month)
            .ToListAsync();

        return salaries.Select(MapToListDto);
    }

    public async Task<IEnumerable<SalaryListDto>> GetSalariesByMonthAsync(int month, int year)
    {
        var salaries = await _context.Salaries
            .Include(s => s.Employee)
            .ThenInclude(e => e.Department)
            .Where(s => s.Month == month && s.Year == year)
            .OrderBy(s => s.Employee.LastName)
            .ToListAsync();

        return salaries.Select(MapToListDto);
    }

    public async Task<IEnumerable<SalaryListDto>> GetSalariesByEmployeeAsync(int employeeId)
    {
        var salaries = await _context.Salaries
            .Include(s => s.Employee)
            .ThenInclude(e => e.Department)
            .Where(s => s.EmployeeId == employeeId)
            .OrderByDescending(s => s.Year)
            .ThenByDescending(s => s.Month)
            .ToListAsync();

        return salaries.Select(MapToListDto);
    }

    public async Task<SalaryListDto?> GetSalaryByIdAsync(int id)
    {
        var salary = await _context.Salaries
            .Include(s => s.Employee)
            .ThenInclude(e => e.Department)
            .FirstOrDefaultAsync(s => s.Id == id);

        return salary == null ? null : MapToListDto(salary);
    }

    public async Task<int> CreateSalaryAsync(SalaryCreateDto dto)
    {
        var employee = await _context.Employees.FindAsync(dto.EmployeeId);
        if (employee == null) return 0;

        var salary = new Salary
        {
            EmployeeId = dto.EmployeeId,
            Month = dto.Month,
            Year = dto.Year,
            BaseSalary = employee.BaseSalary,
            Bonus = dto.Bonus,
            Deductions = dto.Deductions,
            Overtime = dto.Overtime,
            NetSalary = employee.BaseSalary + dto.Bonus + dto.Overtime - dto.Deductions,
            Notes = dto.Notes,
            Status = SalaryStatus.Pending,
            CreatedAt = DateTime.UtcNow
        };

        _context.Salaries.Add(salary);
        await _context.SaveChangesAsync();
        return salary.Id;
    }

    public async Task<bool> UpdateSalaryAsync(SalaryUpdateDto dto)
    {
        var salary = await _context.Salaries.FindAsync(dto.Id);
        if (salary == null) return false;

        var employee = await _context.Employees.FindAsync(dto.EmployeeId);
        if (employee == null) return false;

        salary.Bonus = dto.Bonus;
        salary.Deductions = dto.Deductions;
        salary.Overtime = dto.Overtime;
        salary.NetSalary = salary.BaseSalary + dto.Bonus + dto.Overtime - dto.Deductions;
        salary.Notes = dto.Notes;

        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<bool> ApproveSalaryAsync(int id, string approvedBy)
    {
        var salary = await _context.Salaries.FindAsync(id);
        if (salary == null) return false;

        salary.Status = SalaryStatus.Approved;
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<bool> MarkAsPaidAsync(int id)
    {
        var salary = await _context.Salaries.FindAsync(id);
        if (salary == null) return false;

        salary.Status = SalaryStatus.Paid;
        salary.PaidDate = DateTime.UtcNow;
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<bool> DeleteSalaryAsync(int id)
    {
        var salary = await _context.Salaries.FindAsync(id);
        if (salary == null) return false;

        _context.Salaries.Remove(salary);
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<int> GenerateMonthlySalariesAsync(int month, int year)
    {
        var activeEmployees = await _context.Employees
            .Where(e => e.IsActive)
            .ToListAsync();

        var existingSalaries = await _context.Salaries
            .Where(s => s.Month == month && s.Year == year)
            .Select(s => s.EmployeeId)
            .ToListAsync();

        var count = 0;
        foreach (var employee in activeEmployees)
        {
            if (existingSalaries.Contains(employee.Id)) continue;

            var salary = new Salary
            {
                EmployeeId = employee.Id,
                Month = month,
                Year = year,
                BaseSalary = employee.BaseSalary,
                Bonus = 0,
                Deductions = 0,
                Overtime = 0,
                NetSalary = employee.BaseSalary,
                Status = SalaryStatus.Pending,
                CreatedAt = DateTime.UtcNow
            };

            _context.Salaries.Add(salary);
            count++;
        }

        await _context.SaveChangesAsync();
        return count;
    }

    public async Task<decimal> GetTotalSalariesByMonthAsync(int month, int year)
    {
        return await _context.Salaries
            .Where(s => s.Month == month && s.Year == year && s.Status == SalaryStatus.Paid)
            .SumAsync(s => s.NetSalary);
    }

    private static SalaryListDto MapToListDto(Salary s) => new()
    {
        Id = s.Id,
        EmployeeId = s.EmployeeId,
        EmployeeName = s.Employee.FullName,
        DepartmentName = s.Employee.Department.Name,
        Month = s.Month,
        Year = s.Year,
        BaseSalary = s.BaseSalary,
        Bonus = s.Bonus,
        Deductions = s.Deductions,
        Overtime = s.Overtime,
        NetSalary = s.NetSalary,
        StatusName = s.Status.ToString(),
        PaidDate = s.PaidDate
    };
}

public class AttendanceService : IAttendanceService
{
    private readonly ApplicationDbContext _context;

    public AttendanceService(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<AttendanceListDto>> GetAttendanceByDateAsync(DateTime date)
    {
        var attendances = await _context.Attendances
            .Include(a => a.Employee)
            .Where(a => a.Date.Date == date.Date)
            .OrderBy(a => a.Employee.LastName)
            .ToListAsync();

        return attendances.Select(MapToListDto);
    }

    public async Task<IEnumerable<AttendanceListDto>> GetAttendanceByEmployeeAsync(int employeeId, DateTime startDate, DateTime endDate)
    {
        var attendances = await _context.Attendances
            .Include(a => a.Employee)
            .Where(a => a.EmployeeId == employeeId && a.Date >= startDate && a.Date <= endDate)
            .OrderByDescending(a => a.Date)
            .ToListAsync();

        return attendances.Select(MapToListDto);
    }

    public async Task<AttendanceListDto?> GetAttendanceByIdAsync(int id)
    {
        var attendance = await _context.Attendances
            .Include(a => a.Employee)
            .FirstOrDefaultAsync(a => a.Id == id);

        return attendance == null ? null : MapToListDto(attendance);
    }

    public async Task<int> CheckInAsync(int employeeId)
    {
        var today = DateTime.Today;
        var existing = await _context.Attendances
            .FirstOrDefaultAsync(a => a.EmployeeId == employeeId && a.Date == today);

        if (existing != null)
        {
            if (existing.CheckInTime == null)
            {
                existing.CheckInTime = DateTime.Now;
                await _context.SaveChangesAsync();
            }
            return existing.Id;
        }

        var attendance = new Attendance
        {
            EmployeeId = employeeId,
            Date = today,
            CheckInTime = DateTime.Now,
            Status = AttendanceStatus.Present,
            CreatedAt = DateTime.UtcNow
        };

        _context.Attendances.Add(attendance);
        await _context.SaveChangesAsync();
        return attendance.Id;
    }

    public async Task<bool> CheckOutAsync(int employeeId)
    {
        var today = DateTime.Today;
        var attendance = await _context.Attendances
            .FirstOrDefaultAsync(a => a.EmployeeId == employeeId && a.Date == today);

        if (attendance == null || attendance.CheckInTime == null) return false;

        attendance.CheckOutTime = DateTime.Now;
        attendance.WorkedHours = (attendance.CheckOutTime.Value - attendance.CheckInTime.Value).TotalHours;
        
        // Calculate overtime (assuming 8 hours is standard)
        if (attendance.WorkedHours > 8)
        {
            attendance.OvertimeHours = attendance.WorkedHours - 8;
        }

        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<int> CreateAttendanceAsync(AttendanceCreateDto dto)
    {
        var attendance = new Attendance
        {
            EmployeeId = dto.EmployeeId,
            Date = dto.Date,
            CheckInTime = dto.CheckInTime,
            CheckOutTime = dto.CheckOutTime,
            Status = (AttendanceStatus)dto.Status,
            Notes = dto.Notes,
            CreatedAt = DateTime.UtcNow
        };

        if (dto.CheckInTime.HasValue && dto.CheckOutTime.HasValue)
        {
            attendance.WorkedHours = (dto.CheckOutTime.Value - dto.CheckInTime.Value).TotalHours;
            if (attendance.WorkedHours > 8)
                attendance.OvertimeHours = attendance.WorkedHours - 8;
        }

        _context.Attendances.Add(attendance);
        await _context.SaveChangesAsync();
        return attendance.Id;
    }

    public async Task<bool> UpdateAttendanceAsync(int id, AttendanceCreateDto dto)
    {
        var attendance = await _context.Attendances.FindAsync(id);
        if (attendance == null) return false;

        attendance.CheckInTime = dto.CheckInTime;
        attendance.CheckOutTime = dto.CheckOutTime;
        attendance.Status = (AttendanceStatus)dto.Status;
        attendance.Notes = dto.Notes;

        if (dto.CheckInTime.HasValue && dto.CheckOutTime.HasValue)
        {
            attendance.WorkedHours = (dto.CheckOutTime.Value - dto.CheckInTime.Value).TotalHours;
            attendance.OvertimeHours = attendance.WorkedHours > 8 ? attendance.WorkedHours - 8 : 0;
        }

        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<bool> DeleteAttendanceAsync(int id)
    {
        var attendance = await _context.Attendances.FindAsync(id);
        if (attendance == null) return false;

        _context.Attendances.Remove(attendance);
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<int> GetPresentCountTodayAsync()
    {
        var today = DateTime.Today;
        return await _context.Attendances
            .CountAsync(a => a.Date == today && a.Status == AttendanceStatus.Present);
    }

    private static AttendanceListDto MapToListDto(Attendance a) => new()
    {
        Id = a.Id,
        EmployeeId = a.EmployeeId,
        EmployeeName = a.Employee.FullName,
        DepartmentName = a.Employee.Department?.Name ?? "",
        Date = a.Date,
        CheckInTime = a.CheckInTime,
        CheckOutTime = a.CheckOutTime,
        StatusName = a.Status.ToString(),
        WorkedHours = a.WorkedHours,
        OvertimeHours = a.OvertimeHours,
        Notes = a.Notes
    };
}
