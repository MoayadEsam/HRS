using System.ComponentModel.DataAnnotations;

namespace HotelReservation.Core.Models;

public class Attendance
{
    public int Id { get; set; }
    
    public int EmployeeId { get; set; }
    
    [Required]
    public DateTime Date { get; set; }
    
    public DateTime? CheckInTime { get; set; }
    
    public DateTime? CheckOutTime { get; set; }
    
    public AttendanceStatus Status { get; set; } = AttendanceStatus.Present;
    
    public double? WorkedHours { get; set; }
    
    public double? OvertimeHours { get; set; }
    
    [StringLength(500)]
    public string? Notes { get; set; }
    
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    
    // Navigation
    public virtual Employee Employee { get; set; } = null!;
}

public enum AttendanceStatus
{
    Present,
    Absent,
    Late,
    HalfDay,
    OnLeave,
    Holiday
}
