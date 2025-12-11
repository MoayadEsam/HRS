using HotelReservation.Core.DTOs;

namespace HotelReservation.Services.Interfaces;

public interface IDepartmentService
{
    Task<IEnumerable<DepartmentListDto>> GetAllDepartmentsAsync();
    Task<IEnumerable<DepartmentListDto>> GetActiveDepartmentsAsync();
    Task<DepartmentListDto?> GetDepartmentByIdAsync(int id);
    Task<int> CreateDepartmentAsync(DepartmentCreateDto dto);
    Task<bool> UpdateDepartmentAsync(DepartmentUpdateDto dto);
    Task<bool> DeleteDepartmentAsync(int id);
}

public interface IEmployeeService
{
    Task<IEnumerable<EmployeeListDto>> GetAllEmployeesAsync();
    Task<IEnumerable<EmployeeListDto>> GetEmployeesByDepartmentAsync(int departmentId);
    Task<IEnumerable<EmployeeListDto>> GetActiveEmployeesAsync();
    Task<EmployeeDetailDto?> GetEmployeeByIdAsync(int id);
    Task<int> CreateEmployeeAsync(EmployeeCreateDto dto);
    Task<bool> UpdateEmployeeAsync(EmployeeUpdateDto dto);
    Task<bool> TerminateEmployeeAsync(int id, DateTime terminationDate);
    Task<bool> DeleteEmployeeAsync(int id);
}

public interface ISalaryService
{
    Task<IEnumerable<SalaryListDto>> GetAllSalariesAsync();
    Task<IEnumerable<SalaryListDto>> GetSalariesByMonthAsync(int month, int year);
    Task<IEnumerable<SalaryListDto>> GetSalariesByEmployeeAsync(int employeeId);
    Task<SalaryListDto?> GetSalaryByIdAsync(int id);
    Task<int> CreateSalaryAsync(SalaryCreateDto dto);
    Task<bool> UpdateSalaryAsync(SalaryUpdateDto dto);
    Task<bool> ApproveSalaryAsync(int id, string approvedBy);
    Task<bool> MarkAsPaidAsync(int id);
    Task<bool> DeleteSalaryAsync(int id);
    Task<int> GenerateMonthlySalariesAsync(int month, int year);
    Task<decimal> GetTotalSalariesByMonthAsync(int month, int year);
}

public interface IAttendanceService
{
    Task<IEnumerable<AttendanceListDto>> GetAttendanceByDateAsync(DateTime date);
    Task<IEnumerable<AttendanceListDto>> GetAttendanceByEmployeeAsync(int employeeId, DateTime startDate, DateTime endDate);
    Task<AttendanceListDto?> GetAttendanceByIdAsync(int id);
    Task<int> CheckInAsync(int employeeId);
    Task<bool> CheckOutAsync(int employeeId);
    Task<int> CreateAttendanceAsync(AttendanceCreateDto dto);
    Task<bool> UpdateAttendanceAsync(int id, AttendanceCreateDto dto);
    Task<bool> DeleteAttendanceAsync(int id);
    Task<int> GetPresentCountTodayAsync();
}
