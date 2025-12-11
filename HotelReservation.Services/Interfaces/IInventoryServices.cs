using HotelReservation.Core.DTOs;
using HotelReservation.Core.Models;

namespace HotelReservation.Services.Interfaces;

public interface IInventoryCategoryService
{
    Task<IEnumerable<InventoryCategoryListDto>> GetAllCategoriesAsync();
    Task<IEnumerable<InventoryCategoryListDto>> GetActiveCategoriesAsync();
    Task<InventoryCategoryListDto?> GetCategoryByIdAsync(int id);
    Task<int> CreateCategoryAsync(InventoryCategoryCreateDto dto);
    Task<bool> UpdateCategoryAsync(InventoryCategoryUpdateDto dto);
    Task<bool> DeleteCategoryAsync(int id);
}

public interface IInventoryService
{
    Task<IEnumerable<InventoryItemListDto>> GetAllItemsAsync();
    Task<IEnumerable<InventoryItemListDto>> GetItemsByCategoryAsync(int categoryId);
    Task<IEnumerable<InventoryItemListDto>> GetLowStockItemsAsync();
    Task<InventoryItemDetailsDto?> GetItemByIdAsync(int id);
    Task<int> CreateItemAsync(InventoryItemCreateDto dto, string createdBy);
    Task<bool> UpdateItemAsync(InventoryItemUpdateDto dto);
    Task<bool> DeleteItemAsync(int id);
    Task<InventorySummaryDto> GetInventorySummaryAsync();
    
    // Stock transactions
    Task<bool> UpdateQuantityAsync(int itemId, int quantityChange, TransactionType transactionType, string reference, string notes, string createdBy);
    Task<IEnumerable<InventoryTransactionDto>> GetTransactionHistoryAsync(int itemId);
    Task<IEnumerable<InventoryTransactionListDto>> GetAllTransactionsAsync();
}

public interface IDashboardService
{
    Task<DashboardSummaryDto> GetDashboardSummaryAsync();
    Task<FinancialChartDataDto> GetFinancialChartDataAsync(int year);
    Task<OccupancyChartDataDto> GetOccupancyChartDataAsync(int year);
    Task<IEnumerable<RecentActivityDto>> GetRecentActivitiesAsync(int count = 10);
    Task<IEnumerable<TopRoomDto>> GetTopRoomsAsync(int count = 5);
    Task<IEnumerable<UpcomingCheckInOutDto>> GetUpcomingCheckInsAsync(int days = 7);
    Task<IEnumerable<UpcomingCheckInOutDto>> GetUpcomingCheckOutsAsync(int days = 7);
    Task<IEnumerable<OccupancyReportDto>> GetOccupancyReportAsync(DateTime startDate, DateTime endDate);
}
