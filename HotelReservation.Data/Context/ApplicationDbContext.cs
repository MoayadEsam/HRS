using HotelReservation.Core.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace HotelReservation.Data.Context;

public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }

    public DbSet<Hotel> Hotels { get; set; }
    public DbSet<Room> Rooms { get; set; }
    public DbSet<Amenity> Amenities { get; set; }
    public DbSet<RoomAmenity> RoomAmenities { get; set; }
    public DbSet<Reservation> Reservations { get; set; }
    public DbSet<AuditLog> AuditLogs { get; set; }
    
    // Staff Management
    public DbSet<Department> Departments { get; set; }
    public DbSet<Employee> Employees { get; set; }
    public DbSet<Salary> Salaries { get; set; }
    public DbSet<Attendance> Attendances { get; set; }
    
    // Financial Management
    public DbSet<ExpenseCategory> ExpenseCategories { get; set; }
    public DbSet<Expense> Expenses { get; set; }
    public DbSet<Income> Incomes { get; set; }
    public DbSet<FinancialReport> FinancialReports { get; set; }
    
    // Inventory Management
    public DbSet<InventoryCategory> InventoryCategories { get; set; }
    public DbSet<InventoryItem> InventoryItems { get; set; }
    public DbSet<InventoryTransaction> InventoryTransactions { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Hotel Configuration
        modelBuilder.Entity<Hotel>(entity =>
        {
            entity.HasKey(h => h.Id);
            entity.HasIndex(h => h.Name);
            entity.Property(h => h.Name).IsRequired().HasMaxLength(200);
            entity.Property(h => h.Address).IsRequired().HasMaxLength(500);
            entity.Property(h => h.City).IsRequired().HasMaxLength(100);
            entity.Property(h => h.Country).IsRequired().HasMaxLength(100);
        });

        // Room Configuration
        modelBuilder.Entity<Room>(entity =>
        {
            entity.HasKey(r => r.Id);
            entity.HasIndex(r => new { r.HotelId, r.RoomNumber }).IsUnique();
            entity.Property(r => r.RoomNumber).IsRequired().HasMaxLength(50);
            entity.Property(r => r.PricePerNight).HasColumnType("decimal(18,2)");

            entity.HasOne(r => r.Hotel)
                .WithMany(h => h.Rooms)
                .HasForeignKey(r => r.HotelId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        // Amenity Configuration
        modelBuilder.Entity<Amenity>(entity =>
        {
            entity.HasKey(a => a.Id);
            entity.HasIndex(a => a.Name).IsUnique();
            entity.Property(a => a.Name).IsRequired().HasMaxLength(100);
        });

        // RoomAmenity Configuration (Many-to-Many)
        modelBuilder.Entity<RoomAmenity>(entity =>
        {
            entity.HasKey(ra => new { ra.RoomId, ra.AmenityId });

            entity.HasOne(ra => ra.Room)
                .WithMany(r => r.RoomAmenities)
                .HasForeignKey(ra => ra.RoomId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(ra => ra.Amenity)
                .WithMany(a => a.RoomAmenities)
                .HasForeignKey(ra => ra.AmenityId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        // Reservation Configuration
        modelBuilder.Entity<Reservation>(entity =>
        {
            entity.HasKey(r => r.Id);
            entity.HasIndex(r => r.UserId);
            entity.HasIndex(r => r.RoomId);
            entity.HasIndex(r => new { r.CheckInDate, r.CheckOutDate });
            entity.HasIndex(r => r.CheckInDate);
            entity.HasIndex(r => r.CheckOutDate);
            entity.HasIndex(r => r.Status);
            entity.Property(r => r.TotalPrice).HasColumnType("decimal(18,2)");
            entity.Property(r => r.UserId).IsRequired();

            entity.HasOne(r => r.User)
                .WithMany(u => u.Reservations)
                .HasForeignKey(r => r.UserId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(r => r.Room)
                .WithMany(rm => rm.Reservations)
                .HasForeignKey(r => r.RoomId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        // ApplicationUser Configuration
        modelBuilder.Entity<ApplicationUser>(entity =>
        {
            entity.Property(u => u.FirstName).IsRequired().HasMaxLength(50);
            entity.Property(u => u.LastName).IsRequired().HasMaxLength(50);
        });

        // AuditLog Configuration
        modelBuilder.Entity<AuditLog>(entity =>
        {
            entity.HasKey(a => a.Id);
            entity.HasIndex(a => a.Timestamp);
            entity.Property(a => a.Action).IsRequired().HasMaxLength(100);
            entity.Property(a => a.EntityName).IsRequired().HasMaxLength(100);
        });

        // Department Configuration
        modelBuilder.Entity<Department>(entity =>
        {
            entity.HasKey(d => d.Id);
            entity.HasIndex(d => d.Name).IsUnique();
            entity.Property(d => d.Name).IsRequired().HasMaxLength(100);
        });

        // Employee Configuration
        modelBuilder.Entity<Employee>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => e.Email).IsUnique();
            entity.HasIndex(e => e.EmployeeCode).IsUnique();
            entity.Property(e => e.FirstName).IsRequired().HasMaxLength(100);
            entity.Property(e => e.LastName).IsRequired().HasMaxLength(100);
            entity.Property(e => e.Email).IsRequired().HasMaxLength(256);
            entity.Property(e => e.BaseSalary).HasColumnType("decimal(18,2)");

            entity.HasOne(e => e.Department)
                .WithMany(d => d.Employees)
                .HasForeignKey(e => e.DepartmentId)
                .OnDelete(DeleteBehavior.Restrict);
                
            entity.HasOne(e => e.User)
                .WithOne(u => u.Employee)
                .HasForeignKey<Employee>(e => e.UserId)
                .OnDelete(DeleteBehavior.SetNull);
        });

        // Salary Configuration
        modelBuilder.Entity<Salary>(entity =>
        {
            entity.HasKey(s => s.Id);
            entity.HasIndex(s => new { s.EmployeeId, s.Month, s.Year }).IsUnique();
            entity.HasIndex(s => new { s.Month, s.Year });
            entity.HasIndex(s => s.Status);
            entity.Property(s => s.BaseSalary).HasColumnType("decimal(18,2)");
            entity.Property(s => s.Bonus).HasColumnType("decimal(18,2)");
            entity.Property(s => s.Deductions).HasColumnType("decimal(18,2)");
            entity.Property(s => s.Overtime).HasColumnType("decimal(18,2)");
            entity.Property(s => s.NetSalary).HasColumnType("decimal(18,2)");

            entity.HasOne(s => s.Employee)
                .WithMany(e => e.Salaries)
                .HasForeignKey(s => s.EmployeeId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        // Attendance Configuration
        modelBuilder.Entity<Attendance>(entity =>
        {
            entity.HasKey(a => a.Id);
            entity.HasIndex(a => new { a.EmployeeId, a.Date }).IsUnique();
            entity.HasIndex(a => a.Date);
            entity.HasIndex(a => a.Status);

            entity.HasOne(a => a.Employee)
                .WithMany(e => e.Attendances)
                .HasForeignKey(a => a.EmployeeId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        // ExpenseCategory Configuration
        modelBuilder.Entity<ExpenseCategory>(entity =>
        {
            entity.HasKey(ec => ec.Id);
            entity.HasIndex(ec => ec.Name).IsUnique();
            entity.Property(ec => ec.Name).IsRequired().HasMaxLength(100);
        });

        // Expense Configuration
        modelBuilder.Entity<Expense>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => e.ExpenseDate);
            entity.HasIndex(e => e.Status);
            entity.HasIndex(e => new { e.ExpenseDate, e.Status });
            entity.Property(e => e.Title).IsRequired().HasMaxLength(200);
            entity.Property(e => e.Amount).HasColumnType("decimal(18,2)");

            entity.HasOne(e => e.Category)
                .WithMany(c => c.Expenses)
                .HasForeignKey(e => e.CategoryId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        // Income Configuration
        modelBuilder.Entity<Income>(entity =>
        {
            entity.HasKey(i => i.Id);
            entity.HasIndex(i => i.IncomeDate);
            entity.HasIndex(i => i.Type);
            entity.HasIndex(i => new { i.IncomeDate, i.Type });
            entity.Property(i => i.Title).IsRequired().HasMaxLength(200);
            entity.Property(i => i.Amount).HasColumnType("decimal(18,2)");

            entity.HasOne(i => i.Reservation)
                .WithMany()
                .HasForeignKey(i => i.ReservationId)
                .OnDelete(DeleteBehavior.SetNull);
        });

        // FinancialReport Configuration
        modelBuilder.Entity<FinancialReport>(entity =>
        {
            entity.HasKey(fr => fr.Id);
            entity.HasIndex(fr => new { fr.Type, fr.Month, fr.Year });
            entity.Property(fr => fr.TotalIncome).HasColumnType("decimal(18,2)");
            entity.Property(fr => fr.TotalExpenses).HasColumnType("decimal(18,2)");
            entity.Property(fr => fr.TotalSalaries).HasColumnType("decimal(18,2)");
            entity.Property(fr => fr.NetProfit).HasColumnType("decimal(18,2)");
            entity.Property(fr => fr.OccupancyRate).HasColumnType("decimal(5,2)");
        });

        // InventoryCategory Configuration
        modelBuilder.Entity<InventoryCategory>(entity =>
        {
            entity.HasKey(ic => ic.Id);
            entity.HasIndex(ic => ic.Name).IsUnique();
            entity.Property(ic => ic.Name).IsRequired().HasMaxLength(100);
        });

        // InventoryItem Configuration
        modelBuilder.Entity<InventoryItem>(entity =>
        {
            entity.HasKey(ii => ii.Id);
            entity.HasIndex(ii => ii.SKU).IsUnique();
            entity.Property(ii => ii.Name).IsRequired().HasMaxLength(100);
            entity.Property(ii => ii.UnitPrice).HasColumnType("decimal(18,2)");

            entity.HasOne(ii => ii.Category)
                .WithMany(c => c.Items)
                .HasForeignKey(ii => ii.CategoryId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        // InventoryTransaction Configuration
        modelBuilder.Entity<InventoryTransaction>(entity =>
        {
            entity.HasKey(it => it.Id);
            entity.HasIndex(it => it.CreatedAt);
            entity.Property(it => it.UnitPrice).HasColumnType("decimal(18,2)");

            entity.HasOne(it => it.Item)
                .WithMany(i => i.Transactions)
                .HasForeignKey(it => it.ItemId)
                .OnDelete(DeleteBehavior.Cascade);
        });
    }
}
