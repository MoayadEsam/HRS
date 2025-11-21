using HotelReservation.Core.Enums;
using HotelReservation.Core.Models;
using HotelReservation.Data.Context;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace HotelReservation.Data.Seed;

public static class DbInitializer
{
    public static async Task InitializeAsync(
        ApplicationDbContext context,
        UserManager<ApplicationUser> userManager,
        RoleManager<IdentityRole> roleManager)
    {
        // Ensure database is created (for development without migrations)
        // Note: For production, use MigrateAsync() with proper migrations
        await context.Database.EnsureCreatedAsync();

        // Seed Roles
        await SeedRolesAsync(roleManager);

        // Seed Users
        await SeedUsersAsync(userManager);

        // Seed Data
        await SeedHotelsAsync(context);
        await SeedAmenitiesAsync(context);
        await SeedRoomsAsync(context);
    }

    private static async Task SeedRolesAsync(RoleManager<IdentityRole> roleManager)
    {
        string[] roles = { "Admin", "Staff", "User" };

        foreach (var roleName in roles)
        {
            if (!await roleManager.RoleExistsAsync(roleName))
            {
                await roleManager.CreateAsync(new IdentityRole(roleName));
            }
        }
    }

    private static async Task SeedUsersAsync(UserManager<ApplicationUser> userManager)
    {
        // Admin User
        if (await userManager.FindByEmailAsync("admin@hotel.com") == null)
        {
            var adminUser = new ApplicationUser
            {
                UserName = "admin@hotel.com",
                Email = "admin@hotel.com",
                FirstName = "Admin",
                LastName = "User",
                EmailConfirmed = true
            };

            var result = await userManager.CreateAsync(adminUser, "Admin123!");
            if (result.Succeeded)
            {
                await userManager.AddToRoleAsync(adminUser, "Admin");
            }
        }

        // Staff User
        if (await userManager.FindByEmailAsync("staff@hotel.com") == null)
        {
            var staffUser = new ApplicationUser
            {
                UserName = "staff@hotel.com",
                Email = "staff@hotel.com",
                FirstName = "Staff",
                LastName = "Member",
                EmailConfirmed = true
            };

            var result = await userManager.CreateAsync(staffUser, "Staff123!");
            if (result.Succeeded)
            {
                await userManager.AddToRoleAsync(staffUser, "Staff");
            }
        }

        // Regular User
        if (await userManager.FindByEmailAsync("user@hotel.com") == null)
        {
            var regularUser = new ApplicationUser
            {
                UserName = "user@hotel.com",
                Email = "user@hotel.com",
                FirstName = "Regular",
                LastName = "User",
                EmailConfirmed = true
            };

            var result = await userManager.CreateAsync(regularUser, "User123!");
            if (result.Succeeded)
            {
                await userManager.AddToRoleAsync(regularUser, "User");
            }
        }
    }

    private static async Task SeedHotelsAsync(ApplicationDbContext context)
    {
        if (await context.Hotels.AnyAsync())
            return;

        var hotels = new List<Hotel>
        {
            new Hotel
            {
                Name = "Grand Plaza Hotel",
                Address = "123 Main Street",
                City = "New York",
                Country = "USA",
                PhoneNumber = "+1-555-0100",
                Email = "info@grandplaza.com",
                StarRating = 5,
                Description = "Luxury hotel in the heart of Manhattan",
                ImageUrl = "https://images.unsplash.com/photo-1566073771259-6a8506099945?w=800&q=80",
                IsActive = true
            },
            new Hotel
            {
                Name = "Seaside Resort",
                Address = "456 Ocean Drive",
                City = "Miami",
                Country = "USA",
                PhoneNumber = "+1-555-0200",
                Email = "info@seasideresort.com",
                StarRating = 4,
                Description = "Beautiful beachfront resort with stunning ocean views",
                ImageUrl = "https://images.unsplash.com/photo-1520250497591-112f2f40a3f4?w=800&q=80",
                IsActive = true
            },
            new Hotel
            {
                Name = "Mountain View Lodge",
                Address = "789 Alpine Road",
                City = "Denver",
                Country = "USA",
                PhoneNumber = "+1-555-0300",
                Email = "info@mountainviewlodge.com",
                StarRating = 4,
                Description = "Cozy lodge with breathtaking mountain views",
                ImageUrl = "https://images.unsplash.com/photo-1542314831-068cd1dbfeeb?w=800&q=80",
                IsActive = true
            }
        };

        await context.Hotels.AddRangeAsync(hotels);
        await context.SaveChangesAsync();
    }

    private static async Task SeedAmenitiesAsync(ApplicationDbContext context)
    {
        if (await context.Amenities.AnyAsync())
            return;

        var amenities = new List<Amenity>
        {
            new Amenity { Name = "Wi-Fi", Description = "Free high-speed internet", IconClass = "fa-wifi", IsActive = true },
            new Amenity { Name = "Air Conditioning", Description = "Climate control system", IconClass = "fa-snowflake", IsActive = true },
            new Amenity { Name = "TV", Description = "Flat-screen television", IconClass = "fa-tv", IsActive = true },
            new Amenity { Name = "Mini Bar", Description = "In-room refreshments", IconClass = "fa-wine-bottle", IsActive = true },
            new Amenity { Name = "Safe", Description = "Personal safe box", IconClass = "fa-lock", IsActive = true },
            new Amenity { Name = "Balcony", Description = "Private balcony", IconClass = "fa-door-open", IsActive = true },
            new Amenity { Name = "Room Service", Description = "24/7 room service", IconClass = "fa-concierge-bell", IsActive = true },
            new Amenity { Name = "Ocean View", Description = "Stunning ocean views", IconClass = "fa-water", IsActive = true },
            new Amenity { Name = "Bathtub", Description = "Luxurious bathtub", IconClass = "fa-bath", IsActive = true },
            new Amenity { Name = "Coffee Maker", Description = "In-room coffee maker", IconClass = "fa-mug-hot", IsActive = true }
        };

        await context.Amenities.AddRangeAsync(amenities);
        await context.SaveChangesAsync();
    }

    private static async Task SeedRoomsAsync(ApplicationDbContext context)
    {
        if (await context.Rooms.AnyAsync())
            return;

        var hotels = await context.Hotels.ToListAsync();
        var amenities = await context.Amenities.ToListAsync();

        if (!hotels.Any() || !amenities.Any())
            return;

        var rooms = new List<Room>();
        var roomAmenities = new List<RoomAmenity>();

        // Grand Plaza Hotel rooms
        var grandPlaza = hotels[0];
        for (int i = 1; i <= 10; i++)
        {
            var room = new Room
            {
                RoomNumber = $"10{i}",
                Type = i <= 3 ? RoomType.Single : i <= 6 ? RoomType.Double : i <= 8 ? RoomType.Suite : RoomType.Deluxe,
                Capacity = i <= 3 ? 1 : i <= 6 ? 2 : i <= 8 ? 3 : 4,
                PricePerNight = i <= 3 ? 150 : i <= 6 ? 250 : i <= 8 ? 400 : 600,
                Description = $"Comfortable room on floor 1",
                FloorNumber = 1,
                IsAvailable = true,
                HotelId = grandPlaza.Id
            };
            rooms.Add(room);
        }

        // Seaside Resort rooms
        var seasideResort = hotels[1];
        for (int i = 1; i <= 8; i++)
        {
            var room = new Room
            {
                RoomNumber = $"20{i}",
                Type = i <= 2 ? RoomType.Single : i <= 5 ? RoomType.Double : RoomType.Suite,
                Capacity = i <= 2 ? 1 : i <= 5 ? 2 : 4,
                PricePerNight = i <= 2 ? 180 : i <= 5 ? 280 : 450,
                Description = $"Beach view room",
                FloorNumber = 2,
                IsAvailable = true,
                HotelId = seasideResort.Id
            };
            rooms.Add(room);
        }

        // Mountain View Lodge rooms
        var mountainView = hotels[2];
        for (int i = 1; i <= 6; i++)
        {
            var room = new Room
            {
                RoomNumber = $"30{i}",
                Type = i <= 2 ? RoomType.Double : RoomType.Suite,
                Capacity = i <= 2 ? 2 : 3,
                PricePerNight = i <= 2 ? 220 : 380,
                Description = $"Mountain view room",
                FloorNumber = 3,
                IsAvailable = true,
                HotelId = mountainView.Id
            };
            rooms.Add(room);
        }

        await context.Rooms.AddRangeAsync(rooms);
        await context.SaveChangesAsync();

        // Assign random amenities to rooms
        var random = new Random();
        foreach (var room in rooms)
        {
            var amenityCount = random.Next(3, 7);
            var selectedAmenities = amenities.OrderBy(x => random.Next()).Take(amenityCount);

            foreach (var amenity in selectedAmenities)
            {
                roomAmenities.Add(new RoomAmenity
                {
                    RoomId = room.Id,
                    AmenityId = amenity.Id
                });
            }
        }

        await context.RoomAmenities.AddRangeAsync(roomAmenities);
        await context.SaveChangesAsync();
    }
}
