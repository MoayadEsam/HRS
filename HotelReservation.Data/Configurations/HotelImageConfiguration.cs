using HotelReservation.Core.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HotelReservation.Data.Configurations;

public class HotelImageConfiguration : IEntityTypeConfiguration<HotelImage>
{
    public void Configure(EntityTypeBuilder<HotelImage> builder)
    {
        builder.HasKey(hi => hi.Id);
        
        builder.Property(hi => hi.ImageUrl)
            .IsRequired()
            .HasMaxLength(1000);
            
        builder.Property(hi => hi.DisplayOrder)
            .HasDefaultValue(0);
            
        builder.Property(hi => hi.IsPrimary)
            .HasDefaultValue(false);
            
        builder.HasOne(hi => hi.Hotel)
            .WithMany(h => h.Images)
            .HasForeignKey(hi => hi.HotelId)
            .OnDelete(DeleteBehavior.Cascade);
            
        // Index for efficient querying by hotel
        builder.HasIndex(hi => hi.HotelId);
        
        // Index for ordering
        builder.HasIndex(hi => new { hi.HotelId, hi.DisplayOrder });
    }
}
