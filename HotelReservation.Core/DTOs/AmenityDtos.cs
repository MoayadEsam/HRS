using System.ComponentModel.DataAnnotations;

namespace HotelReservation.Core.DTOs;

public class AmenityCreateDto
{
    [Required(ErrorMessage = "Amenity name is required")]
    [StringLength(100)]
    public string Name { get; set; } = string.Empty;
    
    public string? Description { get; set; }
    
    public string? IconClass { get; set; }
}

public class AmenityUpdateDto : AmenityCreateDto
{
    public int Id { get; set; }
    public bool IsActive { get; set; }
}

public class AmenityListDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string? IconClass { get; set; }
    public bool IsActive { get; set; }
}
