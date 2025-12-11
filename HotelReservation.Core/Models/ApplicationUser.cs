using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.AspNetCore.Identity;

namespace HotelReservation.Core.Models;

public class ApplicationUser : IdentityUser
{
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    
    [NotMapped]
    public string FullName => $"{FirstName} {LastName}".Trim();
    
    // Navigation properties
    public virtual ICollection<Reservation> Reservations { get; set; } = new List<Reservation>();
    public virtual Employee? Employee { get; set; }
}
