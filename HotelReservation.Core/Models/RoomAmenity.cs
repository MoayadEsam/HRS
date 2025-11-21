namespace HotelReservation.Core.Models;

public class RoomAmenity
{
    public int RoomId { get; set; }
    public int AmenityId { get; set; }
    
    // Navigation properties
    public virtual Room Room { get; set; } = null!;
    public virtual Amenity Amenity { get; set; } = null!;
}
