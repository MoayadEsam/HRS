using HotelReservation.Core.DTOs;
using HotelReservation.Core.Models;
using HotelReservation.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Extensions.Localization;

namespace HotelReservation.Web.Controllers;

[Authorize(Roles = "Admin,Staff")]
public class InventoryController : Controller
{
    private readonly IInventoryCategoryService _categoryService;
    private readonly IInventoryService _itemService;
    private readonly IStringLocalizer<SharedResource> _localizer;

    public InventoryController(
        IInventoryCategoryService categoryService,
        IInventoryService itemService,
        IStringLocalizer<SharedResource> localizer)
    {
        _categoryService = categoryService;
        _itemService = itemService;
        _localizer = localizer;
    }

    #region Overview

    public async Task<IActionResult> Index()
    {
        var summary = await _itemService.GetInventorySummaryAsync();
        var lowStockItems = await _itemService.GetLowStockItemsAsync();

        ViewBag.LowStockItems = lowStockItems;
        return View(summary);
    }

    #endregion

    #region Categories

    public async Task<IActionResult> Categories()
    {
        var categories = await _categoryService.GetAllCategoriesAsync();
        return View(categories);
    }

    public IActionResult CreateCategory()
    {
        return View();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> CreateCategory(InventoryCategoryCreateDto dto)
    {
        if (!ModelState.IsValid)
            return View(dto);

        await _categoryService.CreateCategoryAsync(dto);
        TempData["Success"] = _localizer["CategoryCreated"].Value;
        return RedirectToAction(nameof(Categories));
    }

    public async Task<IActionResult> EditCategory(int id)
    {
        var category = await _categoryService.GetCategoryByIdAsync(id);
        if (category == null)
            return NotFound();

        var dto = new InventoryCategoryUpdateDto
        {
            Id = category.Id,
            Name = category.Name,
            Description = category.Description,
            IsActive = category.IsActive
        };
        return View(dto);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> EditCategory(InventoryCategoryUpdateDto dto)
    {
        if (!ModelState.IsValid)
            return View(dto);

        var result = await _categoryService.UpdateCategoryAsync(dto);
        if (!result)
        {
            TempData["Error"] = _localizer["UpdateFailed"].Value;
            return View(dto);
        }

        TempData["Success"] = _localizer["CategoryUpdated"].Value;
        return RedirectToAction(nameof(Categories));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteCategory(int id)
    {
        var result = await _categoryService.DeleteCategoryAsync(id);
        if (!result)
            TempData["Error"] = _localizer["DeleteFailedHasItems"].Value;
        else
            TempData["Success"] = _localizer["CategoryDeleted"].Value;

        return RedirectToAction(nameof(Categories));
    }

    #endregion

    #region Items

    public async Task<IActionResult> Items(int? categoryId, bool? lowStock)
    {
        IEnumerable<InventoryItemListDto> items;

        if (lowStock == true)
            items = await _itemService.GetLowStockItemsAsync();
        else if (categoryId.HasValue)
            items = await _itemService.GetItemsByCategoryAsync(categoryId.Value);
        else
            items = await _itemService.GetAllItemsAsync();

        var categories = await _categoryService.GetActiveCategoriesAsync();
        ViewBag.Categories = new SelectList(categories, "Id", "Name", categoryId);
        ViewBag.SelectedCategory = categoryId;
        ViewBag.LowStockFilter = lowStock;

        return View(items);
    }

    public async Task<IActionResult> ItemDetails(int id)
    {
        var item = await _itemService.GetItemByIdAsync(id);
        if (item == null)
            return NotFound();

        return View(item);
    }

    public async Task<IActionResult> CreateItem()
    {
        var categories = await _categoryService.GetActiveCategoriesAsync();
        ViewBag.Categories = new SelectList(categories, "Id", "Name");

        return View(new InventoryItemCreateDto
        {
            Quantity = 0,
            ReorderLevel = 10,
            UnitOfMeasure = "pcs"
        });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> CreateItem(InventoryItemCreateDto dto)
    {
        if (!ModelState.IsValid)
        {
            var categories = await _categoryService.GetActiveCategoriesAsync();
            ViewBag.Categories = new SelectList(categories, "Id", "Name", dto.CategoryId);
            return View(dto);
        }

        await _itemService.CreateItemAsync(dto, User.Identity?.Name ?? "System");
        TempData["Success"] = _localizer["ItemCreated"].Value;
        return RedirectToAction(nameof(Items));
    }

    public async Task<IActionResult> EditItem(int id)
    {
        var item = await _itemService.GetItemByIdAsync(id);
        if (item == null)
            return NotFound();

        var categories = await _categoryService.GetActiveCategoriesAsync();
        ViewBag.Categories = new SelectList(categories, "Id", "Name", item.CategoryId);

        var dto = new InventoryItemUpdateDto
        {
            Id = item.Id,
            Name = item.Name,
            Description = item.Description,
            SKU = item.SKU,
            CategoryId = item.CategoryId,
            Quantity = item.Quantity,
            ReorderLevel = item.ReorderLevel,
            UnitOfMeasure = item.UnitOfMeasure,
            UnitPrice = item.UnitPrice,
            Supplier = item.Supplier,
            Location = item.Location,
            IsActive = item.IsActive
        };
        return View(dto);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> EditItem(InventoryItemUpdateDto dto)
    {
        if (!ModelState.IsValid)
        {
            var categories = await _categoryService.GetActiveCategoriesAsync();
            ViewBag.Categories = new SelectList(categories, "Id", "Name", dto.CategoryId);
            return View(dto);
        }

        var result = await _itemService.UpdateItemAsync(dto);
        if (!result)
        {
            TempData["Error"] = _localizer["UpdateFailed"].Value;
            var categories = await _categoryService.GetActiveCategoriesAsync();
            ViewBag.Categories = new SelectList(categories, "Id", "Name", dto.CategoryId);
            return View(dto);
        }

        TempData["Success"] = _localizer["ItemUpdated"].Value;
        return RedirectToAction(nameof(Items));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteItem(int id)
    {
        var result = await _itemService.DeleteItemAsync(id);
        if (!result)
            TempData["Error"] = _localizer["DeleteFailed"].Value;
        else
            TempData["Success"] = _localizer["ItemDeleted"].Value;

        return RedirectToAction(nameof(Items));
    }

    #endregion

    #region Stock Transactions

    public async Task<IActionResult> Transactions(int? itemId, string? transactionType, DateTime? startDate, DateTime? endDate)
    {
        var transactions = await _itemService.GetAllTransactionsAsync();
        
        // Apply filters
        if (itemId.HasValue)
            transactions = transactions.Where(t => t.ItemId == itemId.Value);
        
        if (!string.IsNullOrEmpty(transactionType))
            transactions = transactions.Where(t => t.TransactionType == transactionType);
        
        if (startDate.HasValue)
            transactions = transactions.Where(t => t.TransactionDate >= startDate.Value);
        
        if (endDate.HasValue)
            transactions = transactions.Where(t => t.TransactionDate <= endDate.Value.AddDays(1));

        // Populate ViewBag for filters
        var items = await _itemService.GetAllItemsAsync();
        ViewBag.Items = new SelectList(items, "Id", "Name", itemId);
        ViewBag.SelectedItemId = itemId;
        ViewBag.TransactionType = transactionType;
        ViewBag.StartDate = startDate;
        ViewBag.EndDate = endDate;

        return View(transactions.OrderByDescending(t => t.TransactionDate));
    }

    public async Task<IActionResult> StockIn(int id)
    {
        var item = await _itemService.GetItemByIdAsync(id);
        if (item == null)
            return NotFound();

        ViewBag.Item = item;
        return View(new StockTransactionViewModel { ItemId = id, TransactionType = TransactionType.In });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> StockIn(StockTransactionViewModel model)
    {
        if (!ModelState.IsValid)
        {
            var item = await _itemService.GetItemByIdAsync(model.ItemId);
            ViewBag.Item = item;
            return View(model);
        }

        var result = await _itemService.UpdateQuantityAsync(
            model.ItemId,
            model.Quantity,
            TransactionType.In,
            model.Reference ?? "",
            model.Notes ?? "",
            User.Identity?.Name ?? "System");

        if (!result)
        {
            TempData["Error"] = _localizer["OperationFailed"].Value;
            var item = await _itemService.GetItemByIdAsync(model.ItemId);
            ViewBag.Item = item;
            return View(model);
        }

        TempData["Success"] = _localizer["StockUpdated"].Value;
        return RedirectToAction(nameof(ItemDetails), new { id = model.ItemId });
    }

    public async Task<IActionResult> StockOut(int id)
    {
        var item = await _itemService.GetItemByIdAsync(id);
        if (item == null)
            return NotFound();

        ViewBag.Item = item;
        return View(new StockTransactionViewModel { ItemId = id, TransactionType = TransactionType.Out });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> StockOut(StockTransactionViewModel model)
    {
        if (!ModelState.IsValid)
        {
            var item = await _itemService.GetItemByIdAsync(model.ItemId);
            ViewBag.Item = item;
            return View(model);
        }

        var result = await _itemService.UpdateQuantityAsync(
            model.ItemId,
            model.Quantity,
            TransactionType.Out,
            model.Reference ?? "",
            model.Notes ?? "",
            User.Identity?.Name ?? "System");

        if (!result)
        {
            TempData["Error"] = _localizer["InsufficientStock"].Value;
            var item = await _itemService.GetItemByIdAsync(model.ItemId);
            ViewBag.Item = item;
            return View(model);
        }

        TempData["Success"] = _localizer["StockUpdated"].Value;
        return RedirectToAction(nameof(ItemDetails), new { id = model.ItemId });
    }

    public async Task<IActionResult> StockAdjust(int id)
    {
        var item = await _itemService.GetItemByIdAsync(id);
        if (item == null)
            return NotFound();

        ViewBag.Item = item;
        return View(new StockTransactionViewModel
        {
            ItemId = id,
            TransactionType = TransactionType.Adjustment,
            Quantity = item.Quantity
        });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> StockAdjust(StockTransactionViewModel model)
    {
        if (!ModelState.IsValid)
        {
            var item = await _itemService.GetItemByIdAsync(model.ItemId);
            ViewBag.Item = item;
            return View(model);
        }

        var result = await _itemService.UpdateQuantityAsync(
            model.ItemId,
            model.Quantity,
            TransactionType.Adjustment,
            model.Reference ?? "Manual Adjustment",
            model.Notes ?? "",
            User.Identity?.Name ?? "System");

        if (!result)
        {
            TempData["Error"] = _localizer["OperationFailed"].Value;
            var item = await _itemService.GetItemByIdAsync(model.ItemId);
            ViewBag.Item = item;
            return View(model);
        }

        TempData["Success"] = _localizer["StockAdjusted"].Value;
        return RedirectToAction(nameof(ItemDetails), new { id = model.ItemId });
    }

    public async Task<IActionResult> TransactionHistory(int id)
    {
        var item = await _itemService.GetItemByIdAsync(id);
        if (item == null)
            return NotFound();

        var transactions = await _itemService.GetTransactionHistoryAsync(id);

        ViewBag.Item = item;
        return View(transactions);
    }

    #endregion
}

public class StockTransactionViewModel
{
    public int ItemId { get; set; }
    public TransactionType TransactionType { get; set; }
    public int Quantity { get; set; }
    public string? Reference { get; set; }
    public string? Notes { get; set; }
}
