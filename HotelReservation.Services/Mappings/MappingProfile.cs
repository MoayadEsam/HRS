using AutoMapper;
using HotelReservation.Core.DTOs;
using HotelReservation.Core.Models;

namespace HotelReservation.Services.Mappings;

public class MappingProfile : Profile
{
    public MappingProfile()
    {
        // Hotel mappings
        CreateMap<Hotel, HotelListDto>()
            .ForMember(dest => dest.RoomCount, opt => opt.MapFrom(src => src.Rooms.Count))
            .ForMember(dest => dest.ImageUrls, opt => opt.MapFrom(src => 
                src.Images.OrderBy(i => i.DisplayOrder).Select(i => i.ImageUrl).ToList()))
            .ForMember(dest => dest.ImageUrl, opt => opt.MapFrom(src => 
                src.Images.Any() 
                    ? (src.Images.FirstOrDefault(i => i.IsPrimary) ?? src.Images.OrderBy(i => i.DisplayOrder).First()).ImageUrl 
                    : src.ImageUrl));
        CreateMap<Hotel, HotelDetailsDto>()
            .ForMember(dest => dest.ImageUrls, opt => opt.MapFrom(src => 
                src.Images.OrderBy(i => i.DisplayOrder).Select(i => i.ImageUrl).ToList()))
            .ForMember(dest => dest.ImageUrl, opt => opt.MapFrom(src => 
                src.Images.Any() 
                    ? (src.Images.FirstOrDefault(i => i.IsPrimary) ?? src.Images.OrderBy(i => i.DisplayOrder).First()).ImageUrl 
                    : src.ImageUrl));
        CreateMap<HotelCreateDto, Hotel>();
        CreateMap<HotelUpdateDto, Hotel>();

        // Room mappings
        CreateMap<Room, RoomListDto>()
            .ForMember(dest => dest.HotelName, opt => opt.MapFrom(src => src.Hotel.Name));
        CreateMap<Room, RoomDetailsDto>()
            .ForMember(dest => dest.HotelName, opt => opt.MapFrom(src => src.Hotel.Name))
            .ForMember(dest => dest.Amenities, opt => opt.MapFrom(src => src.RoomAmenities.Select(ra => ra.Amenity)));
        CreateMap<RoomCreateDto, Room>();
        CreateMap<RoomUpdateDto, Room>();

        // Amenity mappings
        CreateMap<Amenity, AmenityListDto>();
        CreateMap<AmenityCreateDto, Amenity>();
        CreateMap<AmenityUpdateDto, Amenity>();

        // Reservation mappings
        CreateMap<Reservation, ReservationListDto>()
            .ForMember(dest => dest.RoomNumber, opt => opt.MapFrom(src => src.Room.RoomNumber))
            .ForMember(dest => dest.HotelName, opt => opt.MapFrom(src => src.Room.Hotel.Name))
            .ForMember(dest => dest.UserName, opt => opt.MapFrom(src => src.User.FirstName + " " + src.User.LastName));
        
        CreateMap<Reservation, ReservationDetailsDto>()
            .ForMember(dest => dest.NumberOfNights, opt => opt.MapFrom(src => (src.CheckOutDate - src.CheckInDate).Days))
            .ForMember(dest => dest.RoomNumber, opt => opt.MapFrom(src => src.Room.RoomNumber))
            .ForMember(dest => dest.RoomType, opt => opt.MapFrom(src => src.Room.Type.ToString()))
            .ForMember(dest => dest.PricePerNight, opt => opt.MapFrom(src => src.Room.PricePerNight))
            .ForMember(dest => dest.HotelId, opt => opt.MapFrom(src => src.Room.HotelId))
            .ForMember(dest => dest.HotelName, opt => opt.MapFrom(src => src.Room.Hotel.Name))
            .ForMember(dest => dest.HotelAddress, opt => opt.MapFrom(src => src.Room.Hotel.Address))
            .ForMember(dest => dest.UserName, opt => opt.MapFrom(src => src.User.FirstName + " " + src.User.LastName))
            .ForMember(dest => dest.UserEmail, opt => opt.MapFrom(src => src.User.Email));
        
        CreateMap<ReservationCreateDto, Reservation>();
        CreateMap<ReservationUpdateDto, Reservation>();

        // User mappings
        CreateMap<ApplicationUser, UserDto>()
            .ForMember(dest => dest.Roles, opt => opt.Ignore());

        // Department mappings
        CreateMap<Department, DepartmentListDto>()
            .ForMember(dest => dest.EmployeeCount, opt => opt.MapFrom(src => src.Employees.Count));
        CreateMap<DepartmentCreateDto, Department>();
        CreateMap<DepartmentUpdateDto, Department>();

        // Employee mappings
        CreateMap<Employee, EmployeeListDto>()
            .ForMember(dest => dest.FullName, opt => opt.MapFrom(src => $"{src.FirstName} {src.LastName}"))
            .ForMember(dest => dest.DepartmentName, opt => opt.MapFrom(src => src.Department.Name));
        CreateMap<Employee, EmployeeDetailsDto>()
            .ForMember(dest => dest.FullName, opt => opt.MapFrom(src => $"{src.FirstName} {src.LastName}"))
            .ForMember(dest => dest.DepartmentName, opt => opt.MapFrom(src => src.Department.Name));
        CreateMap<EmployeeCreateDto, Employee>();
        CreateMap<EmployeeUpdateDto, Employee>();

        // Salary mappings
        CreateMap<Salary, SalaryListDto>()
            .ForMember(dest => dest.EmployeeName, opt => opt.MapFrom(src => $"{src.Employee.FirstName} {src.Employee.LastName}"))
            .ForMember(dest => dest.DepartmentName, opt => opt.MapFrom(src => src.Employee.Department.Name))
            .ForMember(dest => dest.StatusName, opt => opt.MapFrom(src => src.Status.ToString()));
        CreateMap<SalaryCreateDto, Salary>();
        CreateMap<SalaryUpdateDto, Salary>();

        // Attendance mappings
        CreateMap<Attendance, AttendanceListDto>()
            .ForMember(dest => dest.EmployeeName, opt => opt.MapFrom(src => $"{src.Employee.FirstName} {src.Employee.LastName}"))
            .ForMember(dest => dest.StatusName, opt => opt.MapFrom(src => src.Status.ToString()));
        CreateMap<AttendanceCreateDto, Attendance>();

        // Expense Category mappings
        CreateMap<ExpenseCategory, ExpenseCategoryListDto>()
            .ForMember(dest => dest.ExpenseCount, opt => opt.MapFrom(src => src.Expenses.Count))
            .ForMember(dest => dest.TotalAmount, opt => opt.MapFrom(src => src.Expenses.Sum(e => e.Amount)));
        CreateMap<ExpenseCategoryCreateDto, ExpenseCategory>();
        CreateMap<ExpenseCategoryUpdateDto, ExpenseCategory>();

        // Expense mappings
        CreateMap<Expense, ExpenseListDto>()
            .ForMember(dest => dest.CategoryName, opt => opt.MapFrom(src => src.Category.Name))
            .ForMember(dest => dest.PaymentMethodName, opt => opt.MapFrom(src => src.PaymentMethod.ToString()))
            .ForMember(dest => dest.StatusName, opt => opt.MapFrom(src => src.Status.ToString()));
        CreateMap<Expense, ExpenseDetailsDto>()
            .ForMember(dest => dest.CategoryName, opt => opt.MapFrom(src => src.Category.Name))
            .ForMember(dest => dest.PaymentMethodName, opt => opt.MapFrom(src => src.PaymentMethod.ToString()))
            .ForMember(dest => dest.StatusName, opt => opt.MapFrom(src => src.Status.ToString()));
        CreateMap<ExpenseCreateDto, Expense>();
        CreateMap<ExpenseUpdateDto, Expense>();

        // Income mappings
        CreateMap<Income, IncomeListDto>()
            .ForMember(dest => dest.TypeName, opt => opt.MapFrom(src => src.Type.ToString()))
            .ForMember(dest => dest.PaymentMethodName, opt => opt.MapFrom(src => src.PaymentMethod.ToString()));
        CreateMap<Income, IncomeDetailsDto>()
            .ForMember(dest => dest.TypeName, opt => opt.MapFrom(src => src.Type.ToString()))
            .ForMember(dest => dest.PaymentMethodName, opt => opt.MapFrom(src => src.PaymentMethod.ToString()));
        CreateMap<IncomeCreateDto, Income>();
        CreateMap<IncomeUpdateDto, Income>();

        // Inventory Category mappings
        CreateMap<InventoryCategory, InventoryCategoryListDto>()
            .ForMember(dest => dest.ItemCount, opt => opt.MapFrom(src => src.Items.Count))
            .ForMember(dest => dest.TotalValue, opt => opt.MapFrom(src => src.Items.Sum(i => i.Quantity * i.UnitPrice)));
        CreateMap<InventoryCategoryCreateDto, InventoryCategory>();
        CreateMap<InventoryCategoryUpdateDto, InventoryCategory>();

        // Inventory Item mappings
        CreateMap<InventoryItem, InventoryItemListDto>()
            .ForMember(dest => dest.CategoryName, opt => opt.MapFrom(src => src.Category.Name))
            .ForMember(dest => dest.TotalValue, opt => opt.MapFrom(src => src.Quantity * src.UnitPrice))
            .ForMember(dest => dest.IsLowStock, opt => opt.MapFrom(src => src.Quantity <= src.ReorderLevel));
        CreateMap<InventoryItem, InventoryItemDetailsDto>()
            .ForMember(dest => dest.CategoryName, opt => opt.MapFrom(src => src.Category.Name))
            .ForMember(dest => dest.TotalValue, opt => opt.MapFrom(src => src.Quantity * src.UnitPrice))
            .ForMember(dest => dest.IsLowStock, opt => opt.MapFrom(src => src.Quantity <= src.ReorderLevel))
            .ForMember(dest => dest.LastUpdated, opt => opt.MapFrom(src => src.UpdatedAt ?? src.CreatedAt));
        CreateMap<InventoryItemCreateDto, InventoryItem>();
        CreateMap<InventoryItemUpdateDto, InventoryItem>();

        // Inventory Transaction mappings
        CreateMap<InventoryTransaction, InventoryTransactionDto>()
            .ForMember(dest => dest.ItemName, opt => opt.MapFrom(src => src.Item.Name))
            .ForMember(dest => dest.TypeName, opt => opt.MapFrom(src => src.Type.ToString()));
        CreateMap<InventoryTransaction, InventoryTransactionListDto>()
            .ForMember(dest => dest.ItemName, opt => opt.MapFrom(src => src.Item.Name))
            .ForMember(dest => dest.TypeName, opt => opt.MapFrom(src => src.Type.ToString()));
    }
}
