using System.ComponentModel.DataAnnotations;

namespace HotelReservation.Core.DTOs;

// Department DTOs
public class DepartmentListDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public bool IsActive { get; set; }
    public int EmployeeCount { get; set; }
}

public class DepartmentCreateDto
{
    [Required(ErrorMessage = "Department name is required")]
    [StringLength(100)]
    public string Name { get; set; } = string.Empty;
    
    [StringLength(500)]
    public string? Description { get; set; }
}

public class DepartmentUpdateDto : DepartmentCreateDto
{
    public int Id { get; set; }
    public bool IsActive { get; set; } = true;
}

// Employee DTOs
public class EmployeeListDto
{
    public int Id { get; set; }
    public string FullName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string? Phone { get; set; }
    public string Position { get; set; } = string.Empty;
    public string DepartmentName { get; set; } = string.Empty;
    public decimal BaseSalary { get; set; }
    public DateTime HireDate { get; set; }
    public bool IsActive { get; set; }
}

public class EmployeeDetailsDto : EmployeeListDto
{
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string? Address { get; set; }
    public DateTime DateOfBirth { get; set; }
    public DateTime? TerminationDate { get; set; }
    public int DepartmentId { get; set; }
    public string? EmergencyContact { get; set; }
    public string? EmergencyPhone { get; set; }
    public string? Notes { get; set; }
    public DateTime CreatedAt { get; set; }
    public string? UserId { get; set; }
    public string? EmployeeCode { get; set; }
    public bool HasAccount => !string.IsNullOrEmpty(UserId);
}

// Alias for views compatibility
public class EmployeeDetailDto : EmployeeDetailsDto
{
    public List<SalaryListDto>? RecentSalaries { get; set; }
    public List<AttendanceListDto>? RecentAttendance { get; set; }
}

public class EmployeeCreateDto
{
    [Required(ErrorMessage = "First name is required")]
    [StringLength(100)]
    public string FirstName { get; set; } = string.Empty;
    
    [Required(ErrorMessage = "Last name is required")]
    [StringLength(100)]
    public string LastName { get; set; } = string.Empty;
    
    [Required(ErrorMessage = "Email is required")]
    [EmailAddress]
    public string Email { get; set; } = string.Empty;
    
    [StringLength(20)]
    public string? Phone { get; set; }
    
    [StringLength(500)]
    public string? Address { get; set; }
    
    [Required(ErrorMessage = "Date of birth is required")]
    public DateTime DateOfBirth { get; set; }
    
    [Required(ErrorMessage = "Hire date is required")]
    public DateTime HireDate { get; set; }
    
    [Required(ErrorMessage = "Position is required")]
    [StringLength(100)]
    public string Position { get; set; } = string.Empty;
    
    [Required(ErrorMessage = "Base salary is required")]
    [Range(0, double.MaxValue)]
    public decimal BaseSalary { get; set; }
    
    [Required(ErrorMessage = "Department is required")]
    public int DepartmentId { get; set; }
    
    [StringLength(50)]
    public string? EmergencyContact { get; set; }
    
    [StringLength(20)]
    public string? EmergencyPhone { get; set; }
    
    [StringLength(500)]
    public string? Notes { get; set; }
}

public class EmployeeUpdateDto : EmployeeCreateDto
{
    public int Id { get; set; }
    public bool IsActive { get; set; } = true;
    public DateTime? TerminationDate { get; set; }
}

// Salary DTOs
public class SalaryListDto
{
    public int Id { get; set; }
    public int EmployeeId { get; set; }
    public string EmployeeName { get; set; } = string.Empty;
    public string DepartmentName { get; set; } = string.Empty;
    public int Month { get; set; }
    public int Year { get; set; }
    public DateTime PayPeriodStart => new DateTime(Year, Month, 1);
    public DateTime PayPeriodEnd => new DateTime(Year, Month, DateTime.DaysInMonth(Year, Month));
    public decimal BaseSalary { get; set; }
    public decimal GrossAmount => BaseSalary + Bonus + Overtime;
    public decimal Bonus { get; set; }
    public decimal Deductions { get; set; }
    public decimal Overtime { get; set; }
    public decimal NetSalary { get; set; }
    public decimal NetAmount => NetSalary;
    public string StatusName { get; set; } = string.Empty;
    public bool IsPaid => StatusName == "Paid";
    public DateTime? PaidDate { get; set; }
    public DateTime? PaymentDate => PaidDate;
}

public class SalaryCreateDto
{
    [Required]
    public int EmployeeId { get; set; }
    
    [Required]
    [Range(1, 12)]
    public int Month { get; set; }
    
    [Required]
    [Range(2000, 2100)]
    public int Year { get; set; }
    
    [Range(0, double.MaxValue)]
    public decimal Bonus { get; set; }
    
    [Range(0, double.MaxValue)]
    public decimal Deductions { get; set; }
    
    [Range(0, double.MaxValue)]
    public decimal Overtime { get; set; }
    
    [StringLength(500)]
    public string? Notes { get; set; }
}

public class SalaryUpdateDto : SalaryCreateDto
{
    public int Id { get; set; }
}

// Attendance DTOs
public class AttendanceListDto
{
    public int Id { get; set; }
    public int EmployeeId { get; set; }
    public string EmployeeName { get; set; } = string.Empty;
    public string DepartmentName { get; set; } = string.Empty;
    public DateTime Date { get; set; }
    public DateTime? CheckInTime { get; set; }
    public DateTime? CheckOutTime { get; set; }
    public DateTime? CheckIn => CheckInTime;
    public DateTime? CheckOut => CheckOutTime;
    public string StatusName { get; set; } = string.Empty;
    public string Status => StatusName;
    public double? WorkedHours { get; set; }
    public double TotalHours => WorkedHours ?? 0;
    public double? OvertimeHours { get; set; }
    public string? Notes { get; set; }
}

public class AttendanceCreateDto
{
    [Required]
    public int EmployeeId { get; set; }
    
    [Required]
    public DateTime Date { get; set; }
    
    public DateTime? CheckInTime { get; set; }
    
    public DateTime? CheckOutTime { get; set; }
    
    public int Status { get; set; }
    
    [StringLength(500)]
    public string? Notes { get; set; }
}

// Staff Account DTOs
public class StaffAccountCreateDto
{
    [Required(ErrorMessage = "Employee is required")]
    public int EmployeeId { get; set; }
    
    [Required(ErrorMessage = "Password is required")]
    [StringLength(100, MinimumLength = 6, ErrorMessage = "Password must be at least 6 characters")]
    [DataType(DataType.Password)]
    public string Password { get; set; } = string.Empty;
    
    [Compare("Password", ErrorMessage = "Passwords do not match")]
    [DataType(DataType.Password)]
    public string ConfirmPassword { get; set; } = string.Empty;
}

public class StaffRegistrationDto
{
    [Required(ErrorMessage = "Employee code is required")]
    [StringLength(20)]
    public string EmployeeCode { get; set; } = string.Empty;
    
    [Required(ErrorMessage = "Email is required")]
    [EmailAddress]
    public string Email { get; set; } = string.Empty;
    
    [Required(ErrorMessage = "Password is required")]
    [StringLength(100, MinimumLength = 6, ErrorMessage = "Password must be at least 6 characters")]
    [DataType(DataType.Password)]
    public string Password { get; set; } = string.Empty;
    
    [Compare("Password", ErrorMessage = "Passwords do not match")]
    [DataType(DataType.Password)]
    public string ConfirmPassword { get; set; } = string.Empty;
}
