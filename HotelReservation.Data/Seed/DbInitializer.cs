using HotelReservation.Core.Enums;
using HotelReservation.Core.Models;
using HotelReservation.Data.Context;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace HotelReservation.Data.Seed;

public static class DbInitializer
{
    // Cache to store what data already exists - checked once at startup
    private class SeedStatus
    {
        public bool HasHotels { get; set; }
        public bool HasAmenities { get; set; }
        public bool HasRooms { get; set; }
        public bool HasDepartments { get; set; }
        public bool HasExpenseCategories { get; set; }
        public bool HasInventoryCategories { get; set; }
        public bool HasEmployees { get; set; }
        public bool HasSalaries { get; set; }
        public bool HasExpenses { get; set; }
        public bool HasIncomes { get; set; }
        public bool HasInventoryItems { get; set; }
        public bool HasInventoryTransactions { get; set; }
        public bool HasAttendances { get; set; }
        public bool HasReservations { get; set; }
    }

    public static async Task InitializeAsync(
        ApplicationDbContext context,
        UserManager<ApplicationUser> userManager,
        RoleManager<IdentityRole> roleManager)
    {
        // Apply any pending migrations
        await context.Database.MigrateAsync();

        // Seed Roles and Users (required for Identity)
        await SeedRolesAsync(roleManager);
        await SeedUsersAsync(userManager);

        // Check all seed status in a SINGLE query to minimize network round-trips
        var seedStatus = await CheckSeedStatusAsync(context);
        
        // Only seed data that doesn't exist - skip all checks if data exists
        if (seedStatus.HasHotels && seedStatus.HasAmenities && seedStatus.HasRooms &&
            seedStatus.HasDepartments && seedStatus.HasExpenseCategories && seedStatus.HasInventoryCategories &&
            seedStatus.HasEmployees && seedStatus.HasSalaries && seedStatus.HasExpenses &&
            seedStatus.HasIncomes && seedStatus.HasInventoryItems && seedStatus.HasInventoryTransactions &&
            seedStatus.HasAttendances && seedStatus.HasReservations)
        {
            // All data already seeded - skip everything
            return;
        }

        // Seed Data only if missing
        if (!seedStatus.HasHotels) await SeedHotelsAsync(context);
        if (!seedStatus.HasAmenities) await SeedAmenitiesAsync(context);
        if (!seedStatus.HasRooms) await SeedRoomsAsync(context);
        
        // Seed new management tables - Always attempt (methods have internal duplicate checks)
        await SeedExpenseCategoriesAsync(context);
        await SeedInventoryCategoriesAsync(context);
        if (!seedStatus.HasDepartments) await SeedDepartmentsAsync(context);
        
        // Seed comprehensive sample data
        if (!seedStatus.HasEmployees) await SeedEmployeesAsync(context);
        if (!seedStatus.HasSalaries) await SeedSalariesAsync(context);
        if (!seedStatus.HasExpenses) await SeedExpensesAsync(context);
        if (!seedStatus.HasIncomes) await SeedIncomesAsync(context);
        if (!seedStatus.HasInventoryItems) await SeedInventoryItemsAsync(context);
        if (!seedStatus.HasInventoryTransactions) await SeedInventoryTransactionsAsync(context);
        if (!seedStatus.HasAttendances) await SeedAttendancesAsync(context);
        if (!seedStatus.HasReservations) await SeedReservationsAsync(context);
    }

    /// <summary>
    /// Check all seed statuses in a single database round-trip using raw SQL
    /// This replaces 14 separate AnyAsync() calls with 1 query
    /// </summary>
    private static async Task<SeedStatus> CheckSeedStatusAsync(ApplicationDbContext context)
    {
        var sql = @"
            SELECT 
                EXISTS(SELECT 1 FROM ""Hotels"" LIMIT 1) as ""HasHotels"",
                EXISTS(SELECT 1 FROM ""Amenities"" LIMIT 1) as ""HasAmenities"",
                EXISTS(SELECT 1 FROM ""Rooms"" LIMIT 1) as ""HasRooms"",
                EXISTS(SELECT 1 FROM ""Departments"" LIMIT 1) as ""HasDepartments"",
                EXISTS(SELECT 1 FROM ""ExpenseCategories"" LIMIT 1) as ""HasExpenseCategories"",
                EXISTS(SELECT 1 FROM ""InventoryCategories"" LIMIT 1) as ""HasInventoryCategories"",
                EXISTS(SELECT 1 FROM ""Employees"" LIMIT 1) as ""HasEmployees"",
                EXISTS(SELECT 1 FROM ""Salaries"" LIMIT 1) as ""HasSalaries"",
                EXISTS(SELECT 1 FROM ""Expenses"" LIMIT 1) as ""HasExpenses"",
                EXISTS(SELECT 1 FROM ""Incomes"" LIMIT 1) as ""HasIncomes"",
                EXISTS(SELECT 1 FROM ""InventoryItems"" LIMIT 1) as ""HasInventoryItems"",
                EXISTS(SELECT 1 FROM ""InventoryTransactions"" LIMIT 1) as ""HasInventoryTransactions"",
                EXISTS(SELECT 1 FROM ""Attendances"" LIMIT 1) as ""HasAttendances"",
                EXISTS(SELECT 1 FROM ""Reservations"" LIMIT 1) as ""HasReservations""
        ";

        using var connection = context.Database.GetDbConnection();
        await connection.OpenAsync();
        
        using var command = connection.CreateCommand();
        command.CommandText = sql;
        
        using var reader = await command.ExecuteReaderAsync();
        if (await reader.ReadAsync())
        {
            return new SeedStatus
            {
                HasHotels = reader.GetBoolean(0),
                HasAmenities = reader.GetBoolean(1),
                HasRooms = reader.GetBoolean(2),
                HasDepartments = reader.GetBoolean(3),
                HasExpenseCategories = reader.GetBoolean(4),
                HasInventoryCategories = reader.GetBoolean(5),
                HasEmployees = reader.GetBoolean(6),
                HasSalaries = reader.GetBoolean(7),
                HasExpenses = reader.GetBoolean(8),
                HasIncomes = reader.GetBoolean(9),
                HasInventoryItems = reader.GetBoolean(10),
                HasInventoryTransactions = reader.GetBoolean(11),
                HasAttendances = reader.GetBoolean(12),
                HasReservations = reader.GetBoolean(13)
            };
        }

        // Default to empty database
        return new SeedStatus();
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
        // Check removed - handled by CheckSeedStatusAsync

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
        // Check removed - handled by CheckSeedStatusAsync

        var amenities = new List<Amenity>
        {
            new Amenity { Name = "Wi-Fi", Description = "Ücretsiz yüksek hızlı internet", IconClass = "fa-wifi", IsActive = true },
            new Amenity { Name = "Klima", Description = "İklim kontrol sistemi", IconClass = "fa-snowflake", IsActive = true },
            new Amenity { Name = "TV", Description = "Düz ekran televizyon", IconClass = "fa-tv", IsActive = true },
            new Amenity { Name = "Mini Bar", Description = "Oda içi içecekler", IconClass = "fa-wine-bottle", IsActive = true },
            new Amenity { Name = "Kasa", Description = "Kişisel kasa", IconClass = "fa-lock", IsActive = true },
            new Amenity { Name = "Balkon", Description = "Özel balkon", IconClass = "fa-door-open", IsActive = true },
            new Amenity { Name = "Oda Servisi", Description = "7/24 oda servisi", IconClass = "fa-concierge-bell", IsActive = true },
            new Amenity { Name = "Deniz Manzarası", Description = "Muhteşem deniz manzarası", IconClass = "fa-water", IsActive = true },
            new Amenity { Name = "Küvet", Description = "Lüks küvet", IconClass = "fa-bath", IsActive = true },
            new Amenity { Name = "Kahve Makinesi", Description = "Oda içi kahve makinesi", IconClass = "fa-mug-hot", IsActive = true }
        };

        await context.Amenities.AddRangeAsync(amenities);
        await context.SaveChangesAsync();
    }

    private static async Task SeedRoomsAsync(ApplicationDbContext context)
    {
        // Check removed - handled by CheckSeedStatusAsync

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

    private static async Task SeedDepartmentsAsync(ApplicationDbContext context)
    {
        // Check removed - handled by CheckSeedStatusAsync

        var departments = new List<Department>
        {
            new Department { Name = "Ön Büro", Description = "Misafir giriş, çıkış ve resepsiyon hizmetleri", IsActive = true },
            new Department { Name = "Kat Hizmetleri", Description = "Oda temizlik ve bakım", IsActive = true },
            new Department { Name = "Yiyecek & İçecek", Description = "Restoran ve oda servisi operasyonları", IsActive = true },
            new Department { Name = "Teknik Servis", Description = "Bina ve ekipman bakımı", IsActive = true },
            new Department { Name = "Güvenlik", Description = "Misafir ve tesis güvenliği", IsActive = true },
            new Department { Name = "İnsan Kaynakları", Description = "Personel yönetimi ve işe alım", IsActive = true },
            new Department { Name = "Finans", Description = "Muhasebe ve mali yönetim", IsActive = true },
            new Department { Name = "Satış & Pazarlama", Description = "Satış ve tanıtım faaliyetleri", IsActive = true }
        };

        await context.Departments.AddRangeAsync(departments);
        await context.SaveChangesAsync();
    }

    private static async Task SeedExpenseCategoriesAsync(ApplicationDbContext context)
    {
        // Double-check if categories already exist to prevent duplicates
        if (await context.ExpenseCategories.AnyAsync())
        {
            return;
        }

        var categories = new List<ExpenseCategory>
        {
            new ExpenseCategory { Name = "Utilities", Description = "Electricity, water, gas bills", IconClass = "fa-bolt", IsActive = true },
            new ExpenseCategory { Name = "Salaries", Description = "Staff wages and benefits", IconClass = "fa-money-bill", IsActive = true },
            new ExpenseCategory { Name = "Maintenance", Description = "Building and equipment repairs", IconClass = "fa-wrench", IsActive = true },
            new ExpenseCategory { Name = "Supplies", Description = "Office and cleaning supplies", IconClass = "fa-box", IsActive = true },
            new ExpenseCategory { Name = "Food & Beverages", Description = "Restaurant and kitchen supplies", IconClass = "fa-utensils", IsActive = true },
            new ExpenseCategory { Name = "Marketing", Description = "Advertising and promotional costs", IconClass = "fa-bullhorn", IsActive = true },
            new ExpenseCategory { Name = "Insurance", Description = "Property and liability insurance", IconClass = "fa-shield-alt", IsActive = true },
            new ExpenseCategory { Name = "Taxes", Description = "Property and business taxes", IconClass = "fa-landmark", IsActive = true },
            new ExpenseCategory { Name = "Technology", Description = "Software, hardware, and IT services", IconClass = "fa-laptop", IsActive = true },
            new ExpenseCategory { Name = "Miscellaneous", Description = "Other operational expenses", IconClass = "fa-ellipsis-h", IsActive = true }
        };

        await context.ExpenseCategories.AddRangeAsync(categories);
        await context.SaveChangesAsync();
    }

    private static async Task SeedInventoryCategoriesAsync(ApplicationDbContext context)
    {
        // Double-check if categories already exist to prevent duplicates
        if (await context.InventoryCategories.AnyAsync())
        {
            return;
        }

        var categories = new List<InventoryCategory>
        {
            new InventoryCategory { Name = "Linens", Description = "Bed sheets, towels, and fabrics", IsActive = true },
            new InventoryCategory { Name = "Toiletries", Description = "Soaps, shampoos, and bathroom amenities", IsActive = true },
            new InventoryCategory { Name = "Cleaning Supplies", Description = "Detergents and cleaning materials", IsActive = true },
            new InventoryCategory { Name = "Kitchen Supplies", Description = "Cooking utensils and kitchenware", IsActive = true },
            new InventoryCategory { Name = "Office Supplies", Description = "Stationery and office materials", IsActive = true },
            new InventoryCategory { Name = "Furniture", Description = "Room and common area furniture", IsActive = true },
            new InventoryCategory { Name = "Electronics", Description = "TVs, phones, and electronic equipment", IsActive = true },
            new InventoryCategory { Name = "Food & Beverages", Description = "Restaurant and minibar stock", IsActive = true },
            new InventoryCategory { Name = "Maintenance", Description = "Tools and repair supplies", IsActive = true },
            new InventoryCategory { Name = "Guest Amenities", Description = "Complimentary guest items", IsActive = true }
        };

        await context.InventoryCategories.AddRangeAsync(categories);
        await context.SaveChangesAsync();
    }

    private static async Task SeedEmployeesAsync(ApplicationDbContext context)
    {
        // Check removed - handled by CheckSeedStatusAsync

        var departments = await context.Departments.ToListAsync();
        if (!departments.Any()) return;

        var employees = new List<Employee>
        {
            // Front Office
            new Employee
            {
                FirstName = "Ahmet",
                LastName = "Yılmaz",
                Email = "ahmet.yilmaz@hotel.com",
                Phone = "+90-555-111-1111",
                Address = "İstanbul, Kadıköy",
                DateOfBirth = new DateTime(1988, 3, 15),
                HireDate = new DateTime(2020, 1, 10),
                Position = "Front Office Manager",
                BaseSalary = 45000,
                DepartmentId = departments.First(d => d.Name == "Ön Büro").Id,
                IsActive = true,
                EmergencyContact = "Fatma Yılmaz",
                EmergencyPhone = "+90-555-111-2222",
                EmployeeCode = "EMP001"
            },
            new Employee
            {
                FirstName = "Zeynep",
                LastName = "Kaya",
                Email = "zeynep.kaya@hotel.com",
                Phone = "+90-555-222-1111",
                Address = "İstanbul, Beşiktaş",
                DateOfBirth = new DateTime(1995, 7, 22),
                HireDate = new DateTime(2021, 6, 15),
                Position = "Receptionist",
                BaseSalary = 28000,
                DepartmentId = departments.First(d => d.Name == "Ön Büro").Id,
                IsActive = true,
                EmergencyContact = "Mehmet Kaya",
                EmergencyPhone = "+90-555-222-2222",
                EmployeeCode = "EMP002"
            },
            // Housekeeping
            new Employee
            {
                FirstName = "Ayşe",
                LastName = "Demir",
                Email = "ayse.demir@hotel.com",
                Phone = "+90-555-333-1111",
                Address = "İstanbul, Üsküdar",
                DateOfBirth = new DateTime(1985, 11, 8),
                HireDate = new DateTime(2019, 3, 1),
                Position = "Housekeeping Supervisor",
                BaseSalary = 35000,
                DepartmentId = departments.First(d => d.Name == "Kat Hizmetleri").Id,
                IsActive = true,
                EmergencyContact = "Ali Demir",
                EmergencyPhone = "+90-555-333-2222",
                EmployeeCode = "EMP003"
            },
            new Employee
            {
                FirstName = "Mustafa",
                LastName = "Şahin",
                Email = "mustafa.sahin@hotel.com",
                Phone = "+90-555-444-1111",
                Address = "İstanbul, Maltepe",
                DateOfBirth = new DateTime(1990, 5, 30),
                HireDate = new DateTime(2022, 2, 14),
                Position = "Housekeeping Staff",
                BaseSalary = 22000,
                DepartmentId = departments.First(d => d.Name == "Kat Hizmetleri").Id,
                IsActive = true,
                EmployeeCode = "EMP004"
            },
            // Food & Beverage
            new Employee
            {
                FirstName = "Mehmet",
                LastName = "Öztürk",
                Email = "mehmet.ozturk@hotel.com",
                Phone = "+90-555-555-1111",
                Address = "İstanbul, Şişli",
                DateOfBirth = new DateTime(1982, 9, 12),
                HireDate = new DateTime(2018, 8, 20),
                Position = "Executive Chef",
                BaseSalary = 55000,
                DepartmentId = departments.First(d => d.Name == "Yiyecek & İçecek").Id,
                IsActive = true,
                EmergencyContact = "Elif Öztürk",
                EmergencyPhone = "+90-555-555-2222",
                EmployeeCode = "EMP005"
            },
            new Employee
            {
                FirstName = "Elif",
                LastName = "Arslan",
                Email = "elif.arslan@hotel.com",
                Phone = "+90-555-666-1111",
                Address = "İstanbul, Bakırköy",
                DateOfBirth = new DateTime(1993, 2, 18),
                HireDate = new DateTime(2021, 11, 1),
                Position = "Restaurant Server",
                BaseSalary = 25000,
                DepartmentId = departments.First(d => d.Name == "Yiyecek & İçecek").Id,
                IsActive = true,
                EmployeeCode = "EMP006"
            },
            // Maintenance
            new Employee
            {
                FirstName = "Hasan",
                LastName = "Çelik",
                Email = "hasan.celik@hotel.com",
                Phone = "+90-555-777-1111",
                Address = "İstanbul, Ataşehir",
                DateOfBirth = new DateTime(1978, 12, 5),
                HireDate = new DateTime(2017, 5, 15),
                Position = "Chief Engineer",
                BaseSalary = 48000,
                DepartmentId = departments.First(d => d.Name == "Teknik Servis").Id,
                IsActive = true,
                EmergencyContact = "Hatice Çelik",
                EmergencyPhone = "+90-555-777-2222",
                EmployeeCode = "EMP007"
            },
            // Security
            new Employee
            {
                FirstName = "Osman",
                LastName = "Koç",
                Email = "osman.koc@hotel.com",
                Phone = "+90-555-888-1111",
                Address = "İstanbul, Pendik",
                DateOfBirth = new DateTime(1980, 6, 25),
                HireDate = new DateTime(2019, 9, 10),
                Position = "Security Chief",
                BaseSalary = 38000,
                DepartmentId = departments.First(d => d.Name == "Güvenlik").Id,
                IsActive = true,
                EmployeeCode = "EMP008"
            },
            // Finance
            new Employee
            {
                FirstName = "Seda",
                LastName = "Yıldırım",
                Email = "seda.yildirim@hotel.com",
                Phone = "+90-555-999-1111",
                Address = "İstanbul, Kartal",
                DateOfBirth = new DateTime(1987, 4, 14),
                HireDate = new DateTime(2020, 7, 1),
                Position = "Finance Manager",
                BaseSalary = 52000,
                DepartmentId = departments.First(d => d.Name == "Finans").Id,
                IsActive = true,
                EmergencyContact = "Can Yıldırım",
                EmergencyPhone = "+90-555-999-2222",
                EmployeeCode = "EMP009"
            },
            // HR
            new Employee
            {
                FirstName = "Deniz",
                LastName = "Aydın",
                Email = "deniz.aydin@hotel.com",
                Phone = "+90-555-100-1111",
                Address = "İstanbul, Beyoğlu",
                DateOfBirth = new DateTime(1991, 10, 3),
                HireDate = new DateTime(2021, 3, 15),
                Position = "HR Specialist",
                BaseSalary = 40000,
                DepartmentId = departments.First(d => d.Name == "İnsan Kaynakları").Id,
                IsActive = true,
                EmployeeCode = "EMP010"
            },
            // Sales & Marketing
            new Employee
            {
                FirstName = "Burak",
                LastName = "Erdoğan",
                Email = "burak.erdogan@hotel.com",
                Phone = "+90-555-200-1111",
                Address = "İstanbul, Levent",
                DateOfBirth = new DateTime(1989, 8, 20),
                HireDate = new DateTime(2020, 4, 1),
                Position = "Marketing Manager",
                BaseSalary = 50000,
                DepartmentId = departments.First(d => d.Name == "Satış & Pazarlama").Id,
                IsActive = true,
                EmployeeCode = "EMP011"
            },
            // Additional staff
            new Employee
            {
                FirstName = "Canan",
                LastName = "Polat",
                Email = "canan.polat@hotel.com",
                Phone = "+90-555-300-1111",
                Address = "İstanbul, Fatih",
                DateOfBirth = new DateTime(1996, 1, 28),
                HireDate = new DateTime(2023, 1, 10),
                Position = "Night Receptionist",
                BaseSalary = 26000,
                DepartmentId = departments.First(d => d.Name == "Ön Büro").Id,
                IsActive = true,
                EmployeeCode = "EMP012"
            }
        };

        await context.Employees.AddRangeAsync(employees);
        await context.SaveChangesAsync();
    }

    private static async Task SeedSalariesAsync(ApplicationDbContext context)
    {
        // Check removed - handled by CheckSeedStatusAsync

        var employees = await context.Employees.ToListAsync();
        if (!employees.Any()) return;

        var salaries = new List<Salary>();
        var random = new Random(42);

        // Create salary records for last 6 months
        for (int monthOffset = 5; monthOffset >= 0; monthOffset--)
        {
            var targetDate = DateTime.UtcNow.AddMonths(-monthOffset);
            
            foreach (var emp in employees)
            {
                var bonus = monthOffset == 0 ? 0 : random.Next(0, 5) * 1000; // Random bonus 0-4000
                var overtime = random.Next(0, 10) * 500; // Random overtime 0-4500
                var deductions = emp.BaseSalary * 0.15m; // 15% tax deduction
                var netSalary = emp.BaseSalary + bonus + overtime - deductions;

                var status = monthOffset == 0 ? SalaryStatus.Pending : 
                             monthOffset == 1 ? SalaryStatus.Approved : SalaryStatus.Paid;

                salaries.Add(new Salary
                {
                    EmployeeId = emp.Id,
                    Month = targetDate.Month,
                    Year = targetDate.Year,
                    BaseSalary = emp.BaseSalary,
                    Bonus = bonus,
                    Overtime = overtime,
                    Deductions = deductions,
                    NetSalary = netSalary,
                    Status = status,
                    PaidDate = status == SalaryStatus.Paid ? targetDate.AddDays(5) : null,
                    Notes = bonus > 0 ? "Performance bonus included" : null,
                    CreatedAt = targetDate
                });
            }
        }

        await context.Salaries.AddRangeAsync(salaries);
        await context.SaveChangesAsync();
    }

    private static async Task SeedExpensesAsync(ApplicationDbContext context)
    {
        // Check removed - handled by CheckSeedStatusAsync

        var categories = await context.ExpenseCategories.ToListAsync();
        if (!categories.Any()) return;

        var expenses = new List<Expense>
        {
            // Utilities
            new Expense
            {
                Title = "Ocak Ayı Elektrik Faturası",
                Description = "January electricity bill for all facilities",
                Amount = 45000,
                CategoryId = categories.First(c => c.Name == "Utilities").Id,
                ExpenseDate = DateTime.UtcNow.AddDays(-60),
                PaymentMethod = PaymentMethod.BankTransfer,
                Vendor = "AYEDAŞ",
                Reference = "ELK-2024-001",
                Status = ExpenseStatus.Paid,
                CreatedBy = "admin@hotel.com"
            },
            new Expense
            {
                Title = "Ocak Ayı Su Faturası",
                Description = "January water bill",
                Amount = 12000,
                CategoryId = categories.First(c => c.Name == "Utilities").Id,
                ExpenseDate = DateTime.UtcNow.AddDays(-58),
                PaymentMethod = PaymentMethod.BankTransfer,
                Vendor = "İSKİ",
                Reference = "SU-2024-001",
                Status = ExpenseStatus.Paid,
                CreatedBy = "admin@hotel.com"
            },
            new Expense
            {
                Title = "Şubat Ayı Doğalgaz Faturası",
                Description = "February natural gas bill",
                Amount = 28000,
                CategoryId = categories.First(c => c.Name == "Utilities").Id,
                ExpenseDate = DateTime.UtcNow.AddDays(-30),
                PaymentMethod = PaymentMethod.BankTransfer,
                Vendor = "İGDAŞ",
                Reference = "GAZ-2024-001",
                Status = ExpenseStatus.Approved,
                CreatedBy = "admin@hotel.com"
            },
            // Maintenance
            new Expense
            {
                Title = "Asansör Bakımı",
                Description = "Monthly elevator maintenance and inspection",
                Amount = 8500,
                CategoryId = categories.First(c => c.Name == "Maintenance").Id,
                ExpenseDate = DateTime.UtcNow.AddDays(-15),
                PaymentMethod = PaymentMethod.BankTransfer,
                Vendor = "Schindler Asansör",
                Reference = "MNT-2024-001",
                Status = ExpenseStatus.Paid,
                CreatedBy = "admin@hotel.com"
            },
            new Expense
            {
                Title = "Klima Sistemi Tamiri",
                Description = "HVAC system repair in lobby area",
                Amount = 15000,
                CategoryId = categories.First(c => c.Name == "Maintenance").Id,
                ExpenseDate = DateTime.UtcNow.AddDays(-10),
                PaymentMethod = PaymentMethod.CreditCard,
                Vendor = "Daikin Servis",
                Reference = "MNT-2024-002",
                Status = ExpenseStatus.Approved,
                CreatedBy = "admin@hotel.com"
            },
            // Supplies
            new Expense
            {
                Title = "Temizlik Malzemeleri",
                Description = "Monthly cleaning supplies purchase",
                Amount = 6500,
                CategoryId = categories.First(c => c.Name == "Supplies").Id,
                ExpenseDate = DateTime.UtcNow.AddDays(-20),
                PaymentMethod = PaymentMethod.CreditCard,
                Vendor = "Metro Grossmarket",
                Reference = "SUP-2024-001",
                Status = ExpenseStatus.Paid,
                CreatedBy = "admin@hotel.com"
            },
            new Expense
            {
                Title = "Ofis Malzemeleri",
                Description = "Office supplies and stationery",
                Amount = 3200,
                CategoryId = categories.First(c => c.Name == "Supplies").Id,
                ExpenseDate = DateTime.UtcNow.AddDays(-5),
                PaymentMethod = PaymentMethod.Cash,
                Vendor = "Migros",
                Reference = "SUP-2024-002",
                Status = ExpenseStatus.Pending,
                CreatedBy = "admin@hotel.com"
            },
            // Food & Beverages
            new Expense
            {
                Title = "Restoran Malzemeleri",
                Description = "Food supplies for restaurant",
                Amount = 35000,
                CategoryId = categories.First(c => c.Name == "Food & Beverages").Id,
                ExpenseDate = DateTime.UtcNow.AddDays(-7),
                PaymentMethod = PaymentMethod.BankTransfer,
                Vendor = "Makro Market",
                Reference = "FB-2024-001",
                Status = ExpenseStatus.Paid,
                CreatedBy = "admin@hotel.com"
            },
            new Expense
            {
                Title = "İçecek Stoku",
                Description = "Beverage stock for minibar and restaurant",
                Amount = 18000,
                CategoryId = categories.First(c => c.Name == "Food & Beverages").Id,
                ExpenseDate = DateTime.UtcNow.AddDays(-3),
                PaymentMethod = PaymentMethod.CreditCard,
                Vendor = "Coca-Cola İçecek",
                Reference = "FB-2024-002",
                Status = ExpenseStatus.Approved,
                CreatedBy = "admin@hotel.com"
            },
            // Marketing
            new Expense
            {
                Title = "Google Ads Kampanyası",
                Description = "Monthly digital advertising campaign",
                Amount = 25000,
                CategoryId = categories.First(c => c.Name == "Marketing").Id,
                ExpenseDate = DateTime.UtcNow.AddDays(-25),
                PaymentMethod = PaymentMethod.Online,
                Vendor = "Google",
                Reference = "MKT-2024-001",
                Status = ExpenseStatus.Paid,
                CreatedBy = "admin@hotel.com"
            },
            new Expense
            {
                Title = "Broşür Basımı",
                Description = "Promotional brochures printing",
                Amount = 4500,
                CategoryId = categories.First(c => c.Name == "Marketing").Id,
                ExpenseDate = DateTime.UtcNow.AddDays(-12),
                PaymentMethod = PaymentMethod.BankTransfer,
                Vendor = "Matbaa Plus",
                Reference = "MKT-2024-002",
                Status = ExpenseStatus.Paid,
                CreatedBy = "admin@hotel.com"
            },
            // Insurance
            new Expense
            {
                Title = "Yıllık Bina Sigortası",
                Description = "Annual property insurance premium",
                Amount = 120000,
                CategoryId = categories.First(c => c.Name == "Insurance").Id,
                ExpenseDate = DateTime.UtcNow.AddDays(-45),
                PaymentMethod = PaymentMethod.BankTransfer,
                Vendor = "Allianz Sigorta",
                Reference = "INS-2024-001",
                Status = ExpenseStatus.Paid,
                CreatedBy = "admin@hotel.com"
            },
            // Technology
            new Expense
            {
                Title = "PMS Yazılım Lisansı",
                Description = "Property management system annual license",
                Amount = 45000,
                CategoryId = categories.First(c => c.Name == "Technology").Id,
                ExpenseDate = DateTime.UtcNow.AddDays(-40),
                PaymentMethod = PaymentMethod.BankTransfer,
                Vendor = "Oracle Hospitality",
                Reference = "TECH-2024-001",
                Status = ExpenseStatus.Paid,
                CreatedBy = "admin@hotel.com"
            },
            new Expense
            {
                Title = "Yeni Bilgisayar Alımı",
                Description = "New computers for front desk",
                Amount = 32000,
                CategoryId = categories.First(c => c.Name == "Technology").Id,
                ExpenseDate = DateTime.UtcNow.AddDays(-8),
                PaymentMethod = PaymentMethod.CreditCard,
                Vendor = "MediaMarkt",
                Reference = "TECH-2024-002",
                Status = ExpenseStatus.Pending,
                CreatedBy = "admin@hotel.com"
            },
            // Taxes
            new Expense
            {
                Title = "KDV Ödemesi - Q1",
                Description = "Quarterly VAT payment",
                Amount = 85000,
                CategoryId = categories.First(c => c.Name == "Taxes").Id,
                ExpenseDate = DateTime.UtcNow.AddDays(-35),
                PaymentMethod = PaymentMethod.BankTransfer,
                Vendor = "Vergi Dairesi",
                Reference = "TAX-2024-001",
                Status = ExpenseStatus.Paid,
                CreatedBy = "admin@hotel.com"
            },
            // Current month expenses - for dashboard display
            new Expense
            {
                Title = "Bu Ay Elektrik Faturası",
                Description = "Current month electricity bill",
                Amount = 35000,
                CategoryId = categories.First(c => c.Name == "Utilities").Id,
                ExpenseDate = DateTime.UtcNow.Date.AddDays(-4),
                PaymentMethod = PaymentMethod.BankTransfer,
                Vendor = "AYEDAŞ",
                Reference = "ELK-" + DateTime.UtcNow.Year + "-CURR01",
                Status = ExpenseStatus.Paid,
                CreatedBy = "admin@hotel.com"
            },
            new Expense
            {
                Title = "Bu Ay Temizlik Malzemeleri",
                Description = "Current month cleaning supplies",
                Amount = 8500,
                CategoryId = categories.First(c => c.Name == "Supplies").Id,
                ExpenseDate = DateTime.UtcNow.Date.AddDays(-2),
                PaymentMethod = PaymentMethod.Cash,
                Vendor = "Metro Grossmarket",
                Reference = "SUP-" + DateTime.UtcNow.Year + "-CURR01",
                Status = ExpenseStatus.Approved,
                CreatedBy = "admin@hotel.com"
            },
            new Expense
            {
                Title = "Bugünkü Gider",
                Description = "Today's maintenance expense",
                Amount = 5000,
                CategoryId = categories.First(c => c.Name == "Maintenance").Id,
                ExpenseDate = DateTime.UtcNow.Date,
                PaymentMethod = PaymentMethod.CreditCard,
                Vendor = "Teknik Servis",
                Reference = "MNT-TODAY-" + DateTime.UtcNow.ToString("yyyyMMdd"),
                Status = ExpenseStatus.Pending,
                CreatedBy = "admin@hotel.com"
            }
        };

        await context.Expenses.AddRangeAsync(expenses);
        await context.SaveChangesAsync();
    }

    private static async Task SeedIncomesAsync(ApplicationDbContext context)
    {
        // Check removed - handled by CheckSeedStatusAsync

        var incomes = new List<Income>
        {
            // Room Bookings
            new Income
            {
                Title = "Suite Rezervasyonu - VIP Misafir",
                Description = "3-night suite booking for corporate client",
                Amount = 12000,
                Type = IncomeType.RoomBooking,
                IncomeDate = DateTime.UtcNow.AddDays(-55),
                PaymentMethod = PaymentMethod.CreditCard,
                Reference = "RES-2024-001",
                CreatedBy = "admin@hotel.com"
            },
            new Income
            {
                Title = "Grup Rezervasyonu - 10 Oda",
                Description = "Group booking for wedding guests",
                Amount = 45000,
                Type = IncomeType.RoomBooking,
                IncomeDate = DateTime.UtcNow.AddDays(-45),
                PaymentMethod = PaymentMethod.BankTransfer,
                Reference = "RES-2024-002",
                CreatedBy = "admin@hotel.com"
            },
            new Income
            {
                Title = "Haftalık Konaklama",
                Description = "7-night stay business traveler",
                Amount = 8400,
                Type = IncomeType.RoomBooking,
                IncomeDate = DateTime.UtcNow.AddDays(-30),
                PaymentMethod = PaymentMethod.CreditCard,
                Reference = "RES-2024-003",
                CreatedBy = "admin@hotel.com"
            },
            // Restaurant
            new Income
            {
                Title = "Restoran Geliri - Ocak",
                Description = "Restaurant revenue for January",
                Amount = 125000,
                Type = IncomeType.Restaurant,
                IncomeDate = DateTime.UtcNow.AddDays(-50),
                PaymentMethod = PaymentMethod.Cash,
                Reference = "REST-2024-001",
                CreatedBy = "admin@hotel.com"
            },
            new Income
            {
                Title = "Restoran Geliri - Şubat",
                Description = "Restaurant revenue for February",
                Amount = 138000,
                Type = IncomeType.Restaurant,
                IncomeDate = DateTime.UtcNow.AddDays(-20),
                PaymentMethod = PaymentMethod.Cash,
                Reference = "REST-2024-002",
                CreatedBy = "admin@hotel.com"
            },
            // Room Service
            new Income
            {
                Title = "Oda Servisi - Ocak",
                Description = "Room service revenue for January",
                Amount = 28000,
                Type = IncomeType.RoomService,
                IncomeDate = DateTime.UtcNow.AddDays(-48),
                PaymentMethod = PaymentMethod.Cash,
                Reference = "RS-2024-001",
                CreatedBy = "admin@hotel.com"
            },
            new Income
            {
                Title = "Oda Servisi - Şubat",
                Description = "Room service revenue for February",
                Amount = 32000,
                Type = IncomeType.RoomService,
                IncomeDate = DateTime.UtcNow.AddDays(-18),
                PaymentMethod = PaymentMethod.Cash,
                Reference = "RS-2024-002",
                CreatedBy = "admin@hotel.com"
            },
            // Spa
            new Income
            {
                Title = "Spa Geliri - Ocak",
                Description = "Spa and wellness services revenue",
                Amount = 42000,
                Type = IncomeType.Spa,
                IncomeDate = DateTime.UtcNow.AddDays(-46),
                PaymentMethod = PaymentMethod.CreditCard,
                Reference = "SPA-2024-001",
                CreatedBy = "admin@hotel.com"
            },
            new Income
            {
                Title = "Spa Geliri - Şubat",
                Description = "Spa and wellness services revenue",
                Amount = 48000,
                Type = IncomeType.Spa,
                IncomeDate = DateTime.UtcNow.AddDays(-16),
                PaymentMethod = PaymentMethod.CreditCard,
                Reference = "SPA-2024-002",
                CreatedBy = "admin@hotel.com"
            },
            // Event Hall
            new Income
            {
                Title = "Düğün Organizasyonu",
                Description = "Wedding reception event",
                Amount = 85000,
                Type = IncomeType.EventHall,
                IncomeDate = DateTime.UtcNow.AddDays(-40),
                PaymentMethod = PaymentMethod.BankTransfer,
                Reference = "EVT-2024-001",
                CreatedBy = "admin@hotel.com"
            },
            new Income
            {
                Title = "Konferans Salonu Kiralama",
                Description = "Corporate conference room rental",
                Amount = 25000,
                Type = IncomeType.EventHall,
                IncomeDate = DateTime.UtcNow.AddDays(-25),
                PaymentMethod = PaymentMethod.BankTransfer,
                Reference = "EVT-2024-002",
                CreatedBy = "admin@hotel.com"
            },
            new Income
            {
                Title = "Doğum Günü Partisi",
                Description = "Birthday party event booking",
                Amount = 18000,
                Type = IncomeType.EventHall,
                IncomeDate = DateTime.UtcNow.AddDays(-10),
                PaymentMethod = PaymentMethod.CreditCard,
                Reference = "EVT-2024-003",
                CreatedBy = "admin@hotel.com"
            },
            // Parking
            new Income
            {
                Title = "Otopark Geliri - Ocak",
                Description = "Parking revenue for January",
                Amount = 15000,
                Type = IncomeType.Parking,
                IncomeDate = DateTime.UtcNow.AddDays(-52),
                PaymentMethod = PaymentMethod.Cash,
                Reference = "PRK-2024-001",
                CreatedBy = "admin@hotel.com"
            },
            new Income
            {
                Title = "Otopark Geliri - Şubat",
                Description = "Parking revenue for February",
                Amount = 16500,
                Type = IncomeType.Parking,
                IncomeDate = DateTime.UtcNow.AddDays(-22),
                PaymentMethod = PaymentMethod.Cash,
                Reference = "PRK-2024-002",
                CreatedBy = "admin@hotel.com"
            },
            // MiniBar
            new Income
            {
                Title = "Minibar Geliri - Ocak",
                Description = "Minibar sales revenue",
                Amount = 8500,
                Type = IncomeType.MiniBar,
                IncomeDate = DateTime.UtcNow.AddDays(-50),
                PaymentMethod = PaymentMethod.Cash,
                Reference = "MB-2024-001",
                CreatedBy = "admin@hotel.com"
            },
            new Income
            {
                Title = "Minibar Geliri - Şubat",
                Description = "Minibar sales revenue",
                Amount = 9200,
                Type = IncomeType.MiniBar,
                IncomeDate = DateTime.UtcNow.AddDays(-20),
                PaymentMethod = PaymentMethod.Cash,
                Reference = "MB-2024-002",
                CreatedBy = "admin@hotel.com"
            },
            // Laundry
            new Income
            {
                Title = "Çamaşırhane Geliri - Ocak",
                Description = "Laundry service revenue",
                Amount = 6000,
                Type = IncomeType.Laundry,
                IncomeDate = DateTime.UtcNow.AddDays(-49),
                PaymentMethod = PaymentMethod.Cash,
                Reference = "LND-2024-001",
                CreatedBy = "admin@hotel.com"
            },
            new Income
            {
                Title = "Çamaşırhane Geliri - Şubat",
                Description = "Laundry service revenue",
                Amount = 6800,
                Type = IncomeType.Laundry,
                IncomeDate = DateTime.UtcNow.AddDays(-19),
                PaymentMethod = PaymentMethod.Cash,
                Reference = "LND-2024-002",
                CreatedBy = "admin@hotel.com"
            },
            // Other
            new Income
            {
                Title = "Hediyelik Eşya Satışı",
                Description = "Gift shop sales",
                Amount = 4200,
                Type = IncomeType.Other,
                IncomeDate = DateTime.UtcNow.AddDays(-15),
                PaymentMethod = PaymentMethod.Cash,
                Reference = "OTH-2024-001",
                CreatedBy = "admin@hotel.com"
            },
            new Income
            {
                Title = "Komisyon Geliri",
                Description = "Tour booking commission",
                Amount = 3500,
                Type = IncomeType.Other,
                IncomeDate = DateTime.UtcNow.AddDays(-8),
                PaymentMethod = PaymentMethod.Cash,
                Reference = "OTH-2024-002",
                CreatedBy = "admin@hotel.com"
            },
            // Current month income - for dashboard display
            new Income
            {
                Title = "Bu Ay Oda Gelirleri",
                Description = "This month room bookings revenue",
                Amount = 85000,
                Type = IncomeType.RoomBooking,
                IncomeDate = DateTime.UtcNow.Date.AddDays(-5),
                PaymentMethod = PaymentMethod.CreditCard,
                Reference = "RES-" + DateTime.UtcNow.Year + "-CURR01",
                CreatedBy = "admin@hotel.com"
            },
            new Income
            {
                Title = "Bu Ay Restoran Geliri",
                Description = "This month restaurant revenue",
                Amount = 42000,
                Type = IncomeType.Restaurant,
                IncomeDate = DateTime.UtcNow.Date.AddDays(-3),
                PaymentMethod = PaymentMethod.Cash,
                Reference = "REST-" + DateTime.UtcNow.Year + "-CURR01",
                CreatedBy = "admin@hotel.com"
            },
            new Income
            {
                Title = "Bugünkü Gelir",
                Description = "Today's total income",
                Amount = 15000,
                Type = IncomeType.RoomBooking,
                IncomeDate = DateTime.UtcNow.Date,
                PaymentMethod = PaymentMethod.CreditCard,
                Reference = "TODAY-" + DateTime.UtcNow.ToString("yyyyMMdd"),
                CreatedBy = "admin@hotel.com"
            }
        };

        await context.Incomes.AddRangeAsync(incomes);
        await context.SaveChangesAsync();
    }

    private static async Task SeedInventoryItemsAsync(ApplicationDbContext context)
    {
        // Check removed - handled by CheckSeedStatusAsync

        var categories = await context.InventoryCategories.ToListAsync();
        if (!categories.Any()) return;

        var items = new List<InventoryItem>
        {
            // Linens
            new InventoryItem
            {
                Name = "Çarşaf (Tek Kişilik)",
                SKU = "LIN-001",
                Description = "Single bed white cotton sheets",
                CategoryId = categories.First(c => c.Name == "Linens").Id,
                Quantity = 150,
                MinimumQuantity = 50,
                ReorderLevel = 60,
                Unit = "adet",
                UnitPrice = 120,
                Location = "Depo A-1",
                Supplier = "Özdilek Tekstil",
                IsActive = true,
                LastRestocked = DateTime.UtcNow.AddDays(-10),
                CreatedBy = "admin@hotel.com"
            },
            new InventoryItem
            {
                Name = "Çarşaf (Çift Kişilik)",
                SKU = "LIN-002",
                Description = "Double bed white cotton sheets",
                CategoryId = categories.First(c => c.Name == "Linens").Id,
                Quantity = 120,
                MinimumQuantity = 40,
                ReorderLevel = 50,
                Unit = "adet",
                UnitPrice = 180,
                Location = "Depo A-1",
                Supplier = "Özdilek Tekstil",
                IsActive = true,
                LastRestocked = DateTime.UtcNow.AddDays(-10),
                CreatedBy = "admin@hotel.com"
            },
            new InventoryItem
            {
                Name = "Havlu (Banyo)",
                SKU = "LIN-003",
                Description = "Large bath towels",
                CategoryId = categories.First(c => c.Name == "Linens").Id,
                Quantity = 200,
                MinimumQuantity = 80,
                ReorderLevel = 100,
                Unit = "adet",
                UnitPrice = 85,
                Location = "Depo A-2",
                Supplier = "Taç Tekstil",
                IsActive = true,
                LastRestocked = DateTime.UtcNow.AddDays(-7),
                CreatedBy = "admin@hotel.com"
            },
            new InventoryItem
            {
                Name = "Havlu (El)",
                SKU = "LIN-004",
                Description = "Hand towels",
                CategoryId = categories.First(c => c.Name == "Linens").Id,
                Quantity = 180,
                MinimumQuantity = 60,
                ReorderLevel = 80,
                Unit = "adet",
                UnitPrice = 35,
                Location = "Depo A-2",
                Supplier = "Taç Tekstil",
                IsActive = true,
                LastRestocked = DateTime.UtcNow.AddDays(-7),
                CreatedBy = "admin@hotel.com"
            },
            new InventoryItem
            {
                Name = "Yastık Kılıfı",
                SKU = "LIN-005",
                Description = "White pillow cases",
                CategoryId = categories.First(c => c.Name == "Linens").Id,
                Quantity = 250,
                MinimumQuantity = 100,
                ReorderLevel = 120,
                Unit = "adet",
                UnitPrice = 45,
                Location = "Depo A-1",
                Supplier = "Özdilek Tekstil",
                IsActive = true,
                LastRestocked = DateTime.UtcNow.AddDays(-15),
                CreatedBy = "admin@hotel.com"
            },
            // Toiletries
            new InventoryItem
            {
                Name = "Şampuan (Mini)",
                SKU = "TOI-001",
                Description = "Mini shampoo bottles 30ml",
                CategoryId = categories.First(c => c.Name == "Toiletries").Id,
                Quantity = 500,
                MinimumQuantity = 200,
                ReorderLevel = 250,
                Unit = "adet",
                UnitPrice = 8,
                Location = "Depo B-1",
                Supplier = "Evyap",
                IsActive = true,
                LastRestocked = DateTime.UtcNow.AddDays(-5),
                CreatedBy = "admin@hotel.com"
            },
            new InventoryItem
            {
                Name = "Sabun (Bar)",
                SKU = "TOI-002",
                Description = "Guest soap bars",
                CategoryId = categories.First(c => c.Name == "Toiletries").Id,
                Quantity = 450,
                MinimumQuantity = 150,
                ReorderLevel = 200,
                Unit = "adet",
                UnitPrice = 5,
                Location = "Depo B-1",
                Supplier = "Evyap",
                IsActive = true,
                LastRestocked = DateTime.UtcNow.AddDays(-5),
                CreatedBy = "admin@hotel.com"
            },
            new InventoryItem
            {
                Name = "Diş Fırçası Seti",
                SKU = "TOI-003",
                Description = "Toothbrush and toothpaste set",
                CategoryId = categories.First(c => c.Name == "Toiletries").Id,
                Quantity = 300,
                MinimumQuantity = 100,
                ReorderLevel = 150,
                Unit = "set",
                UnitPrice = 12,
                Location = "Depo B-1",
                Supplier = "Colgate",
                IsActive = true,
                LastRestocked = DateTime.UtcNow.AddDays(-12),
                CreatedBy = "admin@hotel.com"
            },
            new InventoryItem
            {
                Name = "Duş Jeli (Mini)",
                SKU = "TOI-004",
                Description = "Mini shower gel bottles 30ml",
                CategoryId = categories.First(c => c.Name == "Toiletries").Id,
                Quantity = 480,
                MinimumQuantity = 200,
                ReorderLevel = 250,
                Unit = "adet",
                UnitPrice = 9,
                Location = "Depo B-1",
                Supplier = "Evyap",
                IsActive = true,
                LastRestocked = DateTime.UtcNow.AddDays(-5),
                CreatedBy = "admin@hotel.com"
            },
            // Cleaning Supplies
            new InventoryItem
            {
                Name = "Çamaşır Deterjanı",
                SKU = "CLN-001",
                Description = "Industrial laundry detergent 20kg",
                CategoryId = categories.First(c => c.Name == "Cleaning Supplies").Id,
                Quantity = 25,
                MinimumQuantity = 10,
                ReorderLevel = 12,
                Unit = "kutu",
                UnitPrice = 450,
                Location = "Depo C-1",
                Supplier = "Henkel",
                IsActive = true,
                LastRestocked = DateTime.UtcNow.AddDays(-8),
                CreatedBy = "admin@hotel.com"
            },
            new InventoryItem
            {
                Name = "Cam Temizleyici",
                SKU = "CLN-002",
                Description = "Glass cleaner spray 1L",
                CategoryId = categories.First(c => c.Name == "Cleaning Supplies").Id,
                Quantity = 45,
                MinimumQuantity = 20,
                ReorderLevel = 25,
                Unit = "şişe",
                UnitPrice = 35,
                Location = "Depo C-1",
                Supplier = "SC Johnson",
                IsActive = true,
                LastRestocked = DateTime.UtcNow.AddDays(-6),
                CreatedBy = "admin@hotel.com"
            },
            new InventoryItem
            {
                Name = "Dezenfektan",
                SKU = "CLN-003",
                Description = "Surface disinfectant 5L",
                CategoryId = categories.First(c => c.Name == "Cleaning Supplies").Id,
                Quantity = 30,
                MinimumQuantity = 15,
                ReorderLevel = 18,
                Unit = "bidon",
                UnitPrice = 180,
                Location = "Depo C-1",
                Supplier = "Cif",
                IsActive = true,
                LastRestocked = DateTime.UtcNow.AddDays(-4),
                CreatedBy = "admin@hotel.com"
            },
            // Food & Beverages
            new InventoryItem
            {
                Name = "Kahve (Espresso)",
                SKU = "FB-001",
                Description = "Espresso coffee beans 1kg",
                CategoryId = categories.First(c => c.Name == "Food & Beverages").Id,
                Quantity = 40,
                MinimumQuantity = 15,
                ReorderLevel = 20,
                Unit = "paket",
                UnitPrice = 280,
                Location = "Mutfak Deposu",
                Supplier = "Tchibo",
                IsActive = true,
                LastRestocked = DateTime.UtcNow.AddDays(-3),
                CreatedBy = "admin@hotel.com"
            },
            new InventoryItem
            {
                Name = "Su (Pet Şişe 0.5L)",
                SKU = "FB-002",
                Description = "Bottled water 500ml",
                CategoryId = categories.First(c => c.Name == "Food & Beverages").Id,
                Quantity = 600,
                MinimumQuantity = 200,
                ReorderLevel = 300,
                Unit = "adet",
                UnitPrice = 4,
                Location = "Minibar Deposu",
                Supplier = "Erikli",
                IsActive = true,
                LastRestocked = DateTime.UtcNow.AddDays(-2),
                CreatedBy = "admin@hotel.com"
            },
            new InventoryItem
            {
                Name = "Çay (Poşet)",
                SKU = "FB-003",
                Description = "Tea bags box of 100",
                CategoryId = categories.First(c => c.Name == "Food & Beverages").Id,
                Quantity = 50,
                MinimumQuantity = 20,
                ReorderLevel = 25,
                Unit = "kutu",
                UnitPrice = 65,
                Location = "Mutfak Deposu",
                Supplier = "Lipton",
                IsActive = true,
                LastRestocked = DateTime.UtcNow.AddDays(-5),
                CreatedBy = "admin@hotel.com"
            },
            // Guest Amenities
            new InventoryItem
            {
                Name = "Terlik (Disposable)",
                SKU = "GA-001",
                Description = "Disposable guest slippers",
                CategoryId = categories.First(c => c.Name == "Guest Amenities").Id,
                Quantity = 400,
                MinimumQuantity = 150,
                ReorderLevel = 200,
                Unit = "çift",
                UnitPrice = 15,
                Location = "Depo B-2",
                Supplier = "Hotel Supplies Ltd",
                IsActive = true,
                LastRestocked = DateTime.UtcNow.AddDays(-10),
                CreatedBy = "admin@hotel.com"
            },
            new InventoryItem
            {
                Name = "Bornoz",
                SKU = "GA-002",
                Description = "Guest bathrobes",
                CategoryId = categories.First(c => c.Name == "Guest Amenities").Id,
                Quantity = 80,
                MinimumQuantity = 30,
                ReorderLevel = 40,
                Unit = "adet",
                UnitPrice = 250,
                Location = "Depo A-3",
                Supplier = "Özdilek Tekstil",
                IsActive = true,
                LastRestocked = DateTime.UtcNow.AddDays(-20),
                CreatedBy = "admin@hotel.com"
            },
            // Low stock item
            new InventoryItem
            {
                Name = "Pil (AA)",
                SKU = "ELC-001",
                Description = "AA batteries for remotes",
                CategoryId = categories.First(c => c.Name == "Electronics").Id,
                Quantity = 8, // Low stock!
                MinimumQuantity = 20,
                ReorderLevel = 30,
                Unit = "adet",
                UnitPrice = 12,
                Location = "Bakım Deposu",
                Supplier = "Duracell",
                IsActive = true,
                LastRestocked = DateTime.UtcNow.AddDays(-30),
                CreatedBy = "admin@hotel.com"
            },
            new InventoryItem
            {
                Name = "TV Kumandası",
                SKU = "ELC-002",
                Description = "Replacement TV remotes",
                CategoryId = categories.First(c => c.Name == "Electronics").Id,
                Quantity = 5, // Low stock!
                MinimumQuantity = 10,
                ReorderLevel = 15,
                Unit = "adet",
                UnitPrice = 85,
                Location = "Bakım Deposu",
                Supplier = "LG Electronics",
                IsActive = true,
                LastRestocked = DateTime.UtcNow.AddDays(-45),
                CreatedBy = "admin@hotel.com"
            }
        };

        await context.InventoryItems.AddRangeAsync(items);
        await context.SaveChangesAsync();
    }

    private static async Task SeedInventoryTransactionsAsync(ApplicationDbContext context)
    {
        // Check removed - handled by CheckSeedStatusAsync

        var items = await context.InventoryItems.ToListAsync();
        if (!items.Any()) return;

        var transactions = new List<InventoryTransaction>();

        foreach (var item in items.Take(10))
        {
            // Initial stock
            transactions.Add(new InventoryTransaction
            {
                ItemId = item.Id,
                Type = TransactionType.In,
                Quantity = item.Quantity + 50,
                PreviousQuantity = 0,
                NewQuantity = item.Quantity + 50,
                UnitPrice = item.UnitPrice,
                TotalPrice = (item.Quantity + 50) * item.UnitPrice,
                Notes = "Initial stock entry",
                Reference = $"INIT-{item.SKU}",
                CreatedBy = "admin@hotel.com",
                TransactionDate = DateTime.UtcNow.AddDays(-60)
            });

            // Usage/Out transaction
            transactions.Add(new InventoryTransaction
            {
                ItemId = item.Id,
                Type = TransactionType.Out,
                Quantity = 50,
                PreviousQuantity = item.Quantity + 50,
                NewQuantity = item.Quantity,
                UnitPrice = item.UnitPrice,
                TotalPrice = 50 * item.UnitPrice,
                Notes = "Monthly consumption",
                Reference = $"USE-{item.SKU}-001",
                CreatedBy = "admin@hotel.com",
                TransactionDate = DateTime.UtcNow.AddDays(-30)
            });
        }

        await context.InventoryTransactions.AddRangeAsync(transactions);
        await context.SaveChangesAsync();
    }

    private static async Task SeedAttendancesAsync(ApplicationDbContext context)
    {
        // Check removed - handled by CheckSeedStatusAsync

        var employees = await context.Employees.Where(e => e.IsActive).ToListAsync();
        if (!employees.Any()) return;

        var attendances = new List<Attendance>();
        var random = new Random(42);

        // Create attendance for last 30 days
        for (int dayOffset = 29; dayOffset >= 0; dayOffset--)
        {
            var date = DateTime.UtcNow.Date.AddDays(-dayOffset);
            
            // Skip weekends
            if (date.DayOfWeek == DayOfWeek.Saturday || date.DayOfWeek == DayOfWeek.Sunday)
                continue;

            foreach (var emp in employees)
            {
                var isPresent = random.NextDouble() > 0.1; // 90% attendance rate
                var checkInHour = 8 + random.Next(0, 2);
                var checkInMinute = random.Next(0, 60);
                var workHours = isPresent ? 8 + random.Next(0, 3) : 0;

                attendances.Add(new Attendance
                {
                    EmployeeId = emp.Id,
                    Date = date,
                    CheckInTime = isPresent ? date.AddHours(checkInHour).AddMinutes(checkInMinute) : null,
                    CheckOutTime = isPresent ? date.AddHours(checkInHour + workHours).AddMinutes(checkInMinute) : null,
                    Status = isPresent ? AttendanceStatus.Present : AttendanceStatus.Absent,
                    Notes = !isPresent ? "Sick leave" : null,
                    CreatedAt = date
                });
            }
        }

        await context.Attendances.AddRangeAsync(attendances);
        await context.SaveChangesAsync();
    }

    private static async Task SeedReservationsAsync(ApplicationDbContext context)
    {
        // Check removed - handled by CheckSeedStatusAsync

        var rooms = await context.Rooms.Take(10).ToListAsync();
        var users = await context.Users.Take(3).ToListAsync();
        
        if (!rooms.Any() || !users.Any()) return;

        // Get user id (use the regular user for sample reservations)
        var userId = users.FirstOrDefault(u => u.Email == "user@hotel.com")?.Id ?? users.First().Id;

        var reservations = new List<Reservation>();

        // Past reservations (checked out)
        reservations.AddRange(new[]
        {
            new Reservation
            {
                UserId = userId,
                RoomId = rooms[0].Id,
                CheckInDate = DateTime.UtcNow.AddDays(-20),
                CheckOutDate = DateTime.UtcNow.AddDays(-17),
                NumberOfGuests = 1,
                TotalPrice = rooms[0].PricePerNight * 3,
                Status = ReservationStatus.CheckedOut,
                SpecialRequests = "Early check-in requested",
                CreatedAt = DateTime.UtcNow.AddDays(-25)
            },
            new Reservation
            {
                UserId = userId,
                RoomId = rooms[1].Id,
                CheckInDate = DateTime.UtcNow.AddDays(-15),
                CheckOutDate = DateTime.UtcNow.AddDays(-10),
                NumberOfGuests = 2,
                TotalPrice = rooms[1].PricePerNight * 5,
                Status = ReservationStatus.CheckedOut,
                CreatedAt = DateTime.UtcNow.AddDays(-20)
            },
            new Reservation
            {
                UserId = userId,
                RoomId = rooms[2].Id,
                CheckInDate = DateTime.UtcNow.AddDays(-10),
                CheckOutDate = DateTime.UtcNow.AddDays(-7),
                NumberOfGuests = 2,
                TotalPrice = rooms[2].PricePerNight * 3,
                Status = ReservationStatus.CheckedOut,
                SpecialRequests = "High floor room preferred",
                CreatedAt = DateTime.UtcNow.AddDays(-15)
            }
        });

        // Current reservations (checked-in)
        reservations.AddRange(new[]
        {
            new Reservation
            {
                UserId = userId,
                RoomId = rooms[3].Id,
                CheckInDate = DateTime.UtcNow.AddDays(-2),
                CheckOutDate = DateTime.UtcNow.AddDays(3),
                NumberOfGuests = 2,
                TotalPrice = rooms[3].PricePerNight * 5,
                Status = ReservationStatus.CheckedIn,
                SpecialRequests = "Quiet room, non-smoking",
                CreatedAt = DateTime.UtcNow.AddDays(-10)
            },
            new Reservation
            {
                UserId = userId,
                RoomId = rooms[4].Id,
                CheckInDate = DateTime.UtcNow.AddDays(-1),
                CheckOutDate = DateTime.UtcNow.AddDays(4),
                NumberOfGuests = 1,
                TotalPrice = rooms[4].PricePerNight * 5,
                Status = ReservationStatus.CheckedIn,
                CreatedAt = DateTime.UtcNow.AddDays(-7)
            }
        });

        // Today's check-ins (confirmed reservations for today)
        if (rooms.Count > 5)
        {
            reservations.AddRange(new[]
            {
                new Reservation
                {
                    UserId = userId,
                    RoomId = rooms[5].Id,
                    CheckInDate = DateTime.UtcNow.Date,
                    CheckOutDate = DateTime.UtcNow.Date.AddDays(2),
                    NumberOfGuests = 2,
                    TotalPrice = rooms[5].PricePerNight * 2,
                    Status = ReservationStatus.Confirmed,
                    SpecialRequests = "Late check-in expected",
                    CreatedAt = DateTime.UtcNow.AddDays(-5)
                },
                new Reservation
                {
                    UserId = userId,
                    RoomId = rooms[6].Id,
                    CheckInDate = DateTime.UtcNow.Date,
                    CheckOutDate = DateTime.UtcNow.Date.AddDays(3),
                    NumberOfGuests = 1,
                    TotalPrice = rooms[6].PricePerNight * 3,
                    Status = ReservationStatus.Pending,
                    SpecialRequests = "Business traveler",
                    CreatedAt = DateTime.UtcNow.AddDays(-2)
                }
            });
        }

        // Future reservations (confirmed)
        reservations.AddRange(new[]
        {
            new Reservation
            {
                UserId = userId,
                RoomId = rooms[5].Id,
                CheckInDate = DateTime.UtcNow.AddDays(5),
                CheckOutDate = DateTime.UtcNow.AddDays(8),
                NumberOfGuests = 2,
                TotalPrice = rooms[5].PricePerNight * 3,
                Status = ReservationStatus.Confirmed,
                SpecialRequests = "Wedding anniversary - flowers in room",
                CreatedAt = DateTime.UtcNow.AddDays(-3)
            },
            new Reservation
            {
                UserId = userId,
                RoomId = rooms[6].Id,
                CheckInDate = DateTime.UtcNow.AddDays(7),
                CheckOutDate = DateTime.UtcNow.AddDays(10),
                NumberOfGuests = 3,
                TotalPrice = rooms[6].PricePerNight * 3,
                Status = ReservationStatus.Confirmed,
                CreatedAt = DateTime.UtcNow.AddDays(-2)
            },
            new Reservation
            {
                UserId = userId,
                RoomId = rooms[7].Id,
                CheckInDate = DateTime.UtcNow.AddDays(10),
                CheckOutDate = DateTime.UtcNow.AddDays(14),
                NumberOfGuests = 2,
                TotalPrice = rooms[7].PricePerNight * 4,
                Status = ReservationStatus.Confirmed,
                SpecialRequests = "Business trip - late check-out needed",
                CreatedAt = DateTime.UtcNow.AddDays(-1)
            },
            new Reservation
            {
                UserId = userId,
                RoomId = rooms[8].Id,
                CheckInDate = DateTime.UtcNow.AddDays(15),
                CheckOutDate = DateTime.UtcNow.AddDays(17),
                NumberOfGuests = 1,
                TotalPrice = rooms[8].PricePerNight * 2,
                Status = ReservationStatus.Pending,
                CreatedAt = DateTime.UtcNow
            }
        });

        // Cancelled reservation
        reservations.Add(new Reservation
        {
            UserId = userId,
            RoomId = rooms[9].Id,
            CheckInDate = DateTime.UtcNow.AddDays(20),
            CheckOutDate = DateTime.UtcNow.AddDays(23),
            NumberOfGuests = 2,
            TotalPrice = rooms[9].PricePerNight * 3,
            Status = ReservationStatus.Cancelled,
            SpecialRequests = "Cancelled due to schedule change",
            CreatedAt = DateTime.UtcNow.AddDays(-5)
        });

        await context.Reservations.AddRangeAsync(reservations);
        await context.SaveChangesAsync();
    }
}
