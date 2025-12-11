namespace HotelReservation.Core.DTOs;

public class DashboardSummaryDto
{
    // Reservation Stats
    public int TotalReservations { get; set; }
    public int PendingReservations { get; set; }
    public int ConfirmedReservations { get; set; }
    public int TodayCheckIns { get; set; }
    public int TodayCheckOuts { get; set; }
    
    // Room Stats
    public int TotalRooms { get; set; }
    public int AvailableRooms { get; set; }
    public int OccupiedRooms { get; set; }
    public decimal OccupancyRate { get; set; }
    
    // Financial Stats
    public decimal TodayRevenue { get; set; }
    public decimal MonthlyRevenue { get; set; }
    public decimal MonthlyIncome { get; set; }
    public decimal MonthlyExpenses { get; set; }
    public decimal MonthlyProfit { get; set; }
    public decimal NetProfit { get; set; }
    
    // Pending counts (for dashboard badges)
    public int PendingExpenses { get; set; }
    public int PendingSalaries { get; set; }
    
    // Staff Stats
    public int TotalEmployees { get; set; }
    public int ActiveEmployees { get; set; }
    public int PresentToday { get; set; }
    public int TodayAttendance { get; set; }
    public int OnLeaveToday { get; set; }
    
    // Inventory Stats
    public int TotalInventoryItems { get; set; }
    public int LowStockItems { get; set; }
    public int OutOfStockItems { get; set; }
    public decimal InventoryValue { get; set; }
    
    // Recent Activity
    public List<RecentActivityDto> RecentActivities { get; set; } = new();
    public List<RecentReservationDto> RecentReservations { get; set; } = new();
    
    // Charts Data
    public List<MonthlyFinancialDto> MonthlyFinancials { get; set; } = new();
    public List<IncomeByTypeDto> IncomeByType { get; set; } = new();
}

public class RecentReservationDto
{
    public int Id { get; set; }
    public string GuestName { get; set; } = string.Empty;
    public string RoomNumber { get; set; } = string.Empty;
    public DateTime CheckInDate { get; set; }
    public DateTime CheckOutDate { get; set; }
    public decimal TotalAmount { get; set; }
    public string Status { get; set; } = string.Empty;
}

public class RecentActivityDto
{
    public string Type { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Icon { get; set; } = string.Empty;
    public string IconColor { get; set; } = string.Empty;
    public string Color { get; set; } = string.Empty;
    public DateTime Time { get; set; }
    public DateTime Timestamp { get; set; }
}

public class OccupancyReportDto
{
    public DateTime Date { get; set; }
    public int TotalRooms { get; set; }
    public int OccupiedRooms { get; set; }
    public int AvailableRooms { get; set; }
    public decimal OccupancyRate { get; set; }
    public decimal Revenue { get; set; }
}

// Chart Data DTOs
public class FinancialChartDataDto
{
    public int Year { get; set; }
    public List<MonthlyChartDataDto> MonthlyData { get; set; } = new();
    public decimal TotalIncome { get; set; }
    public decimal TotalExpenses { get; set; }
    public decimal TotalProfit { get; set; }
}

public class MonthlyChartDataDto
{
    public int Month { get; set; }
    public string MonthName { get; set; } = string.Empty;
    public decimal Income { get; set; }
    public decimal Expenses { get; set; }
    public decimal Profit { get; set; }
}

public class OccupancyChartDataDto
{
    public int Year { get; set; }
    public int TotalRooms { get; set; }
    public List<MonthlyOccupancyDto> MonthlyData { get; set; } = new();
    public decimal AverageOccupancy { get; set; }
}

public class MonthlyOccupancyDto
{
    public int Month { get; set; }
    public string MonthName { get; set; } = string.Empty;
    public decimal OccupancyRate { get; set; }
    public int BookedRoomDays { get; set; }
    public int AvailableRoomDays { get; set; }
}

public class TopRoomDto
{
    public int RoomId { get; set; }
    public string RoomNumber { get; set; } = string.Empty;
    public string RoomType { get; set; } = string.Empty;
    public int TotalBookings { get; set; }
    public decimal TotalRevenue { get; set; }
    public int OccupancyDays { get; set; }
}

public class UpcomingCheckInOutDto
{
    public int ReservationId { get; set; }
    public string GuestName { get; set; } = string.Empty;
    public string RoomNumber { get; set; } = string.Empty;
    public DateTime Date { get; set; }
    public string Type { get; set; } = string.Empty;
}
