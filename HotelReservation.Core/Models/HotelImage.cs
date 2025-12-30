using System.ComponentModel.DataAnnotations;

namespace HotelReservation.Core.Models;

/// <summary>
/// Represents an image associated with a hotel for gallery/carousel display
/// </summary>
public class HotelImage
{
    public int Id { get; set; }
    
    public int HotelId { get; set; }
    
    [Required]
    [StringLength(1000)]
    public string ImageUrl { get; set; } = string.Empty;
    
    /// <summary>
    /// Order in which images appear in the carousel (lower = first)
    /// </summary>
    public int DisplayOrder { get; set; } = 0;
    
    /// <summary>
    /// Primary image is shown as the main hotel thumbnail
    /// </summary>
    public bool IsPrimary { get; set; } = false;
    
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    
    // Navigation property
    public virtual Hotel Hotel { get; set; } = null!;
}
