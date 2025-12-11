using HotelReservation.Core.DTOs;
using HotelReservation.Core.Models;
using HotelReservation.Data.Context;
using HotelReservation.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace HotelReservation.Services;

public class InventoryCategoryService : IInventoryCategoryService
{
    private readonly ApplicationDbContext _context;

    public InventoryCategoryService(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<InventoryCategoryListDto>> GetAllCategoriesAsync()
    {
        var categories = await _context.InventoryCategories
            .Include(c => c.Items)
            .OrderBy(c => c.Name)
            .ToListAsync();

        return categories.Select(c => new InventoryCategoryListDto
        {
            Id = c.Id,
            Name = c.Name,
            Description = c.Description,
            IsActive = c.IsActive,
            ItemCount = c.Items.Count,
            TotalValue = c.Items.Sum(i => i.Quantity * i.UnitPrice)
        });
    }

    public async Task<IEnumerable<InventoryCategoryListDto>> GetActiveCategoriesAsync()
    {
        var categories = await _context.InventoryCategories
            .Where(c => c.IsActive)
            .Include(c => c.Items)
            .OrderBy(c => c.Name)
            .ToListAsync();

        return categories.Select(c => new InventoryCategoryListDto
        {
            Id = c.Id,
            Name = c.Name,
            Description = c.Description,
            IsActive = c.IsActive,
            ItemCount = c.Items.Count,
            TotalValue = c.Items.Sum(i => i.Quantity * i.UnitPrice)
        });
    }

    public async Task<InventoryCategoryListDto?> GetCategoryByIdAsync(int id)
    {
        var category = await _context.InventoryCategories
            .Include(c => c.Items)
            .FirstOrDefaultAsync(c => c.Id == id);

        if (category == null) return null;

        return new InventoryCategoryListDto
        {
            Id = category.Id,
            Name = category.Name,
            Description = category.Description,
            IsActive = category.IsActive,
            ItemCount = category.Items.Count,
            TotalValue = category.Items.Sum(i => i.Quantity * i.UnitPrice)
        };
    }

    public async Task<int> CreateCategoryAsync(InventoryCategoryCreateDto dto)
    {
        var category = new InventoryCategory
        {
            Name = dto.Name,
            Description = dto.Description,
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };

        _context.InventoryCategories.Add(category);
        await _context.SaveChangesAsync();
        return category.Id;
    }

    public async Task<bool> UpdateCategoryAsync(InventoryCategoryUpdateDto dto)
    {
        var category = await _context.InventoryCategories.FindAsync(dto.Id);
        if (category == null) return false;

        category.Name = dto.Name;
        category.Description = dto.Description;
        category.IsActive = dto.IsActive;

        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<bool> DeleteCategoryAsync(int id)
    {
        var category = await _context.InventoryCategories
            .Include(c => c.Items)
            .FirstOrDefaultAsync(c => c.Id == id);

        if (category == null) return false;
        if (category.Items.Any()) return false;

        _context.InventoryCategories.Remove(category);
        await _context.SaveChangesAsync();
        return true;
    }
}

public class InventoryItemService : IInventoryService
{
    private readonly ApplicationDbContext _context;

    public InventoryItemService(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<InventoryItemListDto>> GetAllItemsAsync()
    {
        var items = await _context.InventoryItems
            .Include(i => i.Category)
            .OrderBy(i => i.Name)
            .ToListAsync();

        return items.Select(MapToListDto);
    }

    public async Task<IEnumerable<InventoryItemListDto>> GetItemsByCategoryAsync(int categoryId)
    {
        var items = await _context.InventoryItems
            .Include(i => i.Category)
            .Where(i => i.CategoryId == categoryId)
            .OrderBy(i => i.Name)
            .ToListAsync();

        return items.Select(MapToListDto);
    }

    public async Task<IEnumerable<InventoryItemListDto>> GetLowStockItemsAsync()
    {
        var items = await _context.InventoryItems
            .Include(i => i.Category)
            .Where(i => i.Quantity <= i.ReorderLevel && i.IsActive)
            .OrderBy(i => i.Quantity)
            .ToListAsync();

        return items.Select(MapToListDto);
    }

    public async Task<InventoryItemDetailsDto?> GetItemByIdAsync(int id)
    {
        var item = await _context.InventoryItems
            .Include(i => i.Category)
            .Include(i => i.Transactions.OrderByDescending(t => t.TransactionDate).Take(10))
            .FirstOrDefaultAsync(i => i.Id == id);

        if (item == null) return null;

        return new InventoryItemDetailsDto
        {
            Id = item.Id,
            Name = item.Name,
            Description = item.Description,
            SKU = item.SKU,
            CategoryId = item.CategoryId,
            CategoryName = item.Category.Name,
            Quantity = item.Quantity,
            UnitOfMeasure = item.UnitOfMeasure,
            UnitPrice = item.UnitPrice,
            TotalValue = item.Quantity * item.UnitPrice,
            ReorderLevel = item.ReorderLevel,
            Supplier = item.Supplier,
            Location = item.Location,
            IsActive = item.IsActive,
            IsLowStock = item.Quantity <= item.ReorderLevel,
            LastUpdated = item.UpdatedAt ?? item.CreatedAt,
            RecentTransactions = item.Transactions.Select(t => new InventoryTransactionDto
            {
                Id = t.Id,
                ItemId = t.ItemId,
                ItemName = item.Name,
                Type = t.Type,
                TypeName = t.Type.ToString(),
                Quantity = t.Quantity,
                PreviousQuantity = t.PreviousQuantity,
                NewQuantity = t.NewQuantity,
                UnitPrice = t.UnitPrice,
                TotalPrice = t.TotalPrice,
                Reference = t.Reference,
                Notes = t.Notes,
                TransactionDate = t.TransactionDate,
                CreatedBy = t.CreatedBy
            }).ToList()
        };
    }

    public async Task<int> CreateItemAsync(InventoryItemCreateDto dto, string createdBy)
    {
        var item = new InventoryItem
        {
            Name = dto.Name,
            Description = dto.Description,
            SKU = dto.SKU ?? GenerateSKU(),
            CategoryId = dto.CategoryId,
            Quantity = dto.Quantity,
            UnitOfMeasure = dto.UnitOfMeasure,
            UnitPrice = dto.UnitPrice,
            ReorderLevel = dto.ReorderLevel,
            Supplier = dto.Supplier,
            Location = dto.Location,
            IsActive = true,
            CreatedAt = DateTime.UtcNow,
            CreatedBy = createdBy
        };

        _context.InventoryItems.Add(item);
        await _context.SaveChangesAsync();

        if (dto.Quantity > 0)
        {
            var transaction = new InventoryTransaction
            {
                ItemId = item.Id,
                Type = TransactionType.In,
                Quantity = dto.Quantity,
                PreviousQuantity = 0,
                NewQuantity = dto.Quantity,
                UnitPrice = dto.UnitPrice,
                TotalPrice = dto.Quantity * dto.UnitPrice,
                Reference = "Initial Stock",
                Notes = "Initial inventory entry",
                TransactionDate = DateTime.UtcNow,
                CreatedBy = createdBy
            };

            _context.InventoryTransactions.Add(transaction);
            await _context.SaveChangesAsync();
        }

        return item.Id;
    }

    public async Task<bool> UpdateItemAsync(InventoryItemUpdateDto dto)
    {
        var item = await _context.InventoryItems.FindAsync(dto.Id);
        if (item == null) return false;

        item.Name = dto.Name;
        item.Description = dto.Description;
        item.SKU = dto.SKU;
        item.CategoryId = dto.CategoryId;
        item.UnitOfMeasure = dto.UnitOfMeasure;
        item.UnitPrice = dto.UnitPrice;
        item.ReorderLevel = dto.ReorderLevel;
        item.Supplier = dto.Supplier;
        item.Location = dto.Location;
        item.IsActive = dto.IsActive;
        item.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<bool> DeleteItemAsync(int id)
    {
        var item = await _context.InventoryItems
            .Include(i => i.Transactions)
            .FirstOrDefaultAsync(i => i.Id == id);

        if (item == null) return false;

        _context.InventoryTransactions.RemoveRange(item.Transactions);
        _context.InventoryItems.Remove(item);
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<bool> UpdateQuantityAsync(int itemId, int quantityChange, TransactionType transactionType, string reference, string notes, string createdBy)
    {
        var item = await _context.InventoryItems.FindAsync(itemId);
        if (item == null) return false;

        var previousQuantity = item.Quantity;
        var newQuantity = transactionType switch
        {
            TransactionType.In => previousQuantity + quantityChange,
            TransactionType.Out => previousQuantity - quantityChange,
            TransactionType.Adjustment => quantityChange,
            _ => previousQuantity
        };

        if (newQuantity < 0) return false;

        item.Quantity = newQuantity;
        item.UpdatedAt = DateTime.UtcNow;

        var transaction = new InventoryTransaction
        {
            ItemId = itemId,
            Type = transactionType,
            Quantity = Math.Abs(quantityChange),
            PreviousQuantity = previousQuantity,
            NewQuantity = newQuantity,
            UnitPrice = item.UnitPrice,
            TotalPrice = Math.Abs(quantityChange) * item.UnitPrice,
            Reference = reference,
            Notes = notes,
            TransactionDate = DateTime.UtcNow,
            CreatedBy = createdBy
        };

        _context.InventoryTransactions.Add(transaction);
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<IEnumerable<InventoryTransactionDto>> GetTransactionHistoryAsync(int itemId)
    {
        var transactions = await _context.InventoryTransactions
            .Include(t => t.Item)
            .Where(t => t.ItemId == itemId)
            .OrderByDescending(t => t.TransactionDate)
            .ToListAsync();

        return transactions.Select(t => new InventoryTransactionDto
        {
            Id = t.Id,
            ItemId = t.ItemId,
            ItemName = t.Item.Name,
            Type = t.Type,
            TypeName = t.Type.ToString(),
            Quantity = t.Quantity,
            PreviousQuantity = t.PreviousQuantity,
            NewQuantity = t.NewQuantity,
            UnitPrice = t.UnitPrice,
            TotalPrice = t.TotalPrice,
            Reference = t.Reference,
            Notes = t.Notes,
            TransactionDate = t.TransactionDate,
            CreatedBy = t.CreatedBy
        });
    }

    public async Task<IEnumerable<InventoryTransactionListDto>> GetAllTransactionsAsync()
    {
        var transactions = await _context.InventoryTransactions
            .AsNoTracking()
            .Include(t => t.Item)
                .ThenInclude(i => i.Category)
            .OrderByDescending(t => t.TransactionDate)
            .Take(500)
            .ToListAsync();

        return transactions.Select(t => new InventoryTransactionListDto
        {
            Id = t.Id,
            ItemId = t.ItemId,
            ItemName = t.Item.Name,
            CategoryName = t.Item.Category.Name,
            TypeName = t.Type.ToString(),
            Type = t.Type,
            Quantity = t.Quantity,
            PreviousQuantity = t.PreviousQuantity,
            NewQuantity = t.NewQuantity,
            UnitPrice = t.UnitPrice,
            TransactionDate = t.TransactionDate,
            CreatedAt = t.CreatedAt,
            CreatedBy = t.CreatedBy,
            Notes = t.Notes
        });
    }

    public async Task<InventorySummaryDto> GetInventorySummaryAsync()
    {
        var items = await _context.InventoryItems.ToListAsync();
        var categories = await _context.InventoryCategories.CountAsync(c => c.IsActive);

        return new InventorySummaryDto
        {
            TotalItems = items.Count,
            TotalCategories = categories,
            TotalValue = items.Sum(i => i.Quantity * i.UnitPrice),
            LowStockCount = items.Count(i => i.Quantity <= i.ReorderLevel && i.IsActive),
            OutOfStockCount = items.Count(i => i.Quantity == 0 && i.IsActive)
        };
    }

    private static string GenerateSKU()
    {
        return $"SKU-{DateTime.UtcNow:yyyyMMdd}-{Guid.NewGuid().ToString()[..8].ToUpper()}";
    }

    private static InventoryItemListDto MapToListDto(InventoryItem item) => new()
    {
        Id = item.Id,
        Name = item.Name,
        SKU = item.SKU,
        CategoryName = item.Category.Name,
        Quantity = item.Quantity,
        UnitOfMeasure = item.UnitOfMeasure,
        UnitPrice = item.UnitPrice,
        TotalValue = item.Quantity * item.UnitPrice,
        ReorderLevel = item.ReorderLevel,
        IsActive = item.IsActive,
        IsLowStock = item.Quantity <= item.ReorderLevel
    };
}
