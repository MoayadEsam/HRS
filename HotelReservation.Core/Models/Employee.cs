using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace HotelReservation.Core.Models;

public class Employee
{
    public int Id { get; set; }
    
    [Required]
    [StringLength(100)]
    public string FirstName { get; set; } = string.Empty;
    
    [Required]
    [StringLength(100)]
    public string LastName { get; set; } = string.Empty;
    
    [Required]
    [EmailAddress]
    [StringLength(256)]
    public string Email { get; set; } = string.Empty;
    
    [StringLength(20)]
    public string? Phone { get; set; }
    
    [StringLength(500)]
    public string? Address { get; set; }
    
    public DateTime DateOfBirth { get; set; }
    
    public DateTime HireDate { get; set; }
    
    public DateTime? TerminationDate { get; set; }
    
    [Required]
    [StringLength(100)]
    public string Position { get; set; } = string.Empty;
    
    [Column(TypeName = "decimal(18,2)")]
    public decimal BaseSalary { get; set; }
    
    public int DepartmentId { get; set; }
    
    public bool IsActive { get; set; } = true;
    
    [StringLength(50)]
    public string? EmergencyContact { get; set; }
    
    [StringLength(20)]
    public string? EmergencyPhone { get; set; }
    
    [StringLength(500)]
    public string? Notes { get; set; }
    
    // Link to ApplicationUser for login
    [StringLength(450)]
    public string? UserId { get; set; }
    
    // Employee ID for registration verification
    [StringLength(20)]
    public string? EmployeeCode { get; set; }
    
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    
    public DateTime? UpdatedAt { get; set; }
    
    // Computed property
    [NotMapped]
    public string FullName => $"{FirstName} {LastName}";
    
    // Navigation
    public virtual Department Department { get; set; } = null!;
    public virtual ApplicationUser? User { get; set; }
    public virtual ICollection<Salary> Salaries { get; set; } = new List<Salary>();
    public virtual ICollection<Attendance> Attendances { get; set; } = new List<Attendance>();
}
