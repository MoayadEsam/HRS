# Hotel Reservation System - ASP.NET Core MVC

A comprehensive hotel reservation system built with ASP.NET Core MVC, Entity Framework Core, SQLite, and following SOLID principles with a layered architecture.

## 🎯 Project Overview

This application provides a complete hotel management and reservation system with role-based access control, room availability checking, search/filtering capabilities, and export functionality.

## 📋 Features Implemented

### ✅ Completed Features

1. **Project Structure & Architecture**
   - Multi-project solution with 5 projects
   - Layered architecture (Web, Core, Data, Services, Tests)
   - SOLID principles implementation
   - Dependency Injection
   - Repository Pattern with Unit of Work

2. **Database & Models**
   - SQLite database
   - Entity Framework Core 9.0
   - Complete domain models:
     - Hotel
     - Room  
     - Amenity
     - Reservation
     - ApplicationUser
     - RoomAmenity (many-to-many)
     - AuditLog
   - Fluent API configurations
   - Database seeding with sample data

3. **Authentication & Authorization**
   - ASP.NET Core Identity
   - Three user roles: Admin, Staff, User
   - Seeded default users:
     - Admin: admin@example.com / Admin123!
     - Staff: staff@example.com / Staff123!
     - User: user@example.com / User123!
   - Cookie-based authentication

4. **Business Logic**
   - Complete service layer with AutoMapper
   - Room availability algorithm
   - Price calculation
   - Reservation overlap detection
   - Validation logic

5. **Data Access**
   - Generic Repository Pattern
   - Specific repositories for each entity
   - Unit of Work pattern
   - Async/await throughout

## 🏗️ Architecture

```
HotelReservation/
├── HotelReservation.Web/          # MVC Web Application
│   ├── Controllers/
│   ├── Views/
│   ├── wwwroot/
│   └── Program.cs
├── HotelReservation.Core/         # Domain Models & DTOs
│   ├── Models/
│   ├── DTOs/
│   └── Enums/
├── HotelReservation.Data/         # Data Access Layer
│   ├── Context/
│   ├── Repositories/
│   ├── Configurations/
│   └── Seed/
├── HotelReservation.Services/     # Business Logic Layer
│   ├── Interfaces/
│   ├── Mappings/
│   └── Services/
└── HotelReservation.Tests/        # Unit Tests
    └── xUnit Test Project
```

## 🔧 Technologies Used

- **Framework**: ASP.NET Core 9.0 MVC
- **ORM**: Entity Framework Core 9.0
- **Database**: SQLite
- **Authentication**: ASP.NET Core Identity
- **Mapping**: AutoMapper 13.0.1
- **PDF Generation**: QuestPDF 2024.10.3
- **Testing**: xUnit, Moq
- **UI Framework**: Bootstrap 5

## 📦 NuGet Packages Installed

### Web Project
- Microsoft.EntityFrameworkCore.Design 9.0.0
- Microsoft.AspNetCore.Identity.EntityFrameworkCore 9.0.0
- QuestPDF 2024.10.3

### Data Project
- Microsoft.EntityFrameworkCore.Sqlite 9.0.0
- Microsoft.AspNetCore.Identity.EntityFrameworkCore 9.0.0

### Core Project
- Microsoft.Extensions.Identity.Stores 9.0.0

### Services Project
- AutoMapper 13.0.1

### Tests Project
- xUnit
- Moq 4.20.72

## 🚀 Getting Started

### Prerequisites
- .NET 9.0 SDK or later
- Visual Studio 2022 / VS Code / Rider

### Installation Steps

1. **Clone or navigate to the project directory**
```powershell
cd C:\Users\Administrator\HRS
```

2. **Restore NuGet packages**
```powershell
dotnet restore HotelReservation.sln
```

3. **Build the solution**
```powershell
dotnet build HotelReservation.sln
```

4. **Create and run database migrations**
```powershell
cd HotelReservation.Web
dotnet ef migrations add InitialCreate --project ..\HotelReservation.Data\HotelReservation.Data.csproj
dotnet ef database update
```

5. **Run the application**
```powershell
dotnet run --project HotelReservation.Web
```

6. **Access the application**
   - Open browser and navigate to: `https://localhost:5001` or `http://localhost:5000`

### Default Login Credentials

- **Admin User**
  - Email: admin@example.com
  - Password: Admin123!

- **Staff User**
  - Email: staff@example.com
  - Password: Staff123!

- **Regular User**
  - Email: user@example.com
  - Password: User123!

## 📊 Database Schema

### Main Entities

**Hotels (1-N with Rooms)**
- Id, Name, Address, City, Country
- PhoneNumber, Email, StarRating
- Description, IsActive, CreatedAt

**Rooms (N-1 with Hotels, N-N with Amenities, 1-N with Reservations)**
- Id, RoomNumber, Type, Capacity
- PricePerNight, Description, FloorNumber
- IsAvailable, HotelId

**Amenities (N-N with Rooms)**
- Id, Name, Description
- IconClass, IsActive

**Reservations (N-1 with Users, N-1 with Rooms)**
- Id, CheckInDate, CheckOutDate
- NumberOfGuests, TotalPrice, Status
- SpecialRequests, UserId, RoomId

**ApplicationUser (Identity User + Extensions)**
- FirstName, LastName, CreatedAt
- + All Identity properties

## 🔑 Features by Role

### Admin
- Full CRUD on Hotels
- Full CRUD on Rooms
- Full CRUD on Amenities
- View all reservations
- Manage users

### Staff
- View hotels and rooms
- Manage all reservations
- Check-in/Check-out guests

### User
- Browse hotels and rooms
- Search and filter rooms
- Create reservations
- View own reservations
- Cancel own reservations

## 📝 Remaining Tasks to Complete

### 1. Complete Controllers
Create the following controllers in `HotelReservation.Web/Controllers/`:

- **RoomsController**: CRUD operations, search, filtering
- **ReservationsController**: User reservations, Admin/Staff management
- **AmenitiesController**: CRUD for amenities (Admin only)
- **AccountController**: Register, Login, Logout
- **ExportController**: CSV and PDF export functionality

### 2. Create Razor Views
Create views for all controllers with Bootstrap 5 styling:

- **Hotels**: Index, Create, Edit, Delete, Details
- **Rooms**: Index, Search, Details, Create, Edit, Delete
- **Reservations**: Index, Create, Details, MyReservations
- **Account**: Register, Login
- **Shared**: _Layout.cshtml, _LoginPartial.cshtml, Error.cshtml

### 3. Implement Export Functionality
- CSV export for reservations
- PDF export using QuestPDF for reservation receipts/reports

### 4. Create Custom Attributes
- Custom validation attributes for date ranges
- Authorization policy attributes

### 5. Global Error Handling
- Create middleware for exception handling
- User-friendly error pages

### 6. Client-Side Validation
- jQuery validation
- Date picker integration
- Dynamic form validation

### 7. Unit Tests
Create tests in `HotelReservation.Tests/`:
- Room availability tests
- Price calculation tests
- Reservation overlap tests
- Service layer tests with Moq

### 8. UI Enhancements
- Add custom CSS
- Implement search filters
- Add date range pickers
- Responsive design improvements

## 🛠️ Development Commands

### Entity Framework Migrations
```powershell
# Create a new migration
dotnet ef migrations add MigrationName --project HotelReservation.Data

# Update database
dotnet ef database update --project HotelReservation.Web

# Remove last migration
dotnet ef migrations remove --project HotelReservation.Data

# Drop database
dotnet ef database drop --project HotelReservation.Web
```

### Build & Run
```powershell
# Build solution
dotnet build

# Run web application
dotnet run --project HotelReservation.Web

# Run tests
dotnet test

# Clean solution
dotnet clean
```

## 🔍 Project Highlights

### Clean Architecture
- Clear separation of concerns
- Dependency Inversion Principle
- Interface-based design

### Repository Pattern
- Generic repository for common operations
- Specific repositories for complex queries
- Unit of Work for transaction management

### Service Layer
- Business logic separation
- AutoMapper for DTO mapping
- Input validation
- Room availability algorithm

### Data Seeding
- Automatic role creation
- Default users
- Sample hotels, rooms, and amenities

## 📖 Key Implementation Details

### Room Availability Algorithm
```csharp
// Checks if a room is available for a date range
// Returns false if ANY reservation overlaps
// Overlap condition: NOT (existing.End <= checkIn OR existing.Start >= checkOut)
```

### Price Calculation
```csharp
TotalPrice = Room.PricePerNight × NumberOfNights
```

### Authorization
- Controller-level authorization with `[Authorize(Roles = "Admin")]`
- Action-level `[AllowAnonymous]` for public endpoints
- Cookie-based authentication with 7-day expiration

## 🎨 UI Framework
- Bootstrap 5.3
- Font Awesome icons for amenities
- Responsive design
- Toast notifications for success/error messages

## 📈 Future Enhancements
- Payment integration
- Email notifications
- Review and rating system
- Advanced reporting
- Loyalty program
- Multi-language support

## 🐛 Troubleshooting

### Database Issues
If you encounter database errors:
```powershell
dotnet ef database drop --project HotelReservation.Web --force
dotnet ef database update --project HotelReservation.Web
```

### Port Conflicts
Modify `launchSettings.json` to change ports if needed.

### Build Errors
```powershell
dotnet clean
dotnet restore
dotnet build
```

## 📄 License
This project is for educational purposes.

## 👥 Contributors
Developed as a comprehensive ASP.NET Core MVC demonstration project.

---

**Note**: This README documents the current state of the project. Complete the remaining tasks listed above to finalize the application with full CRUD operations, UI views, and export functionality.