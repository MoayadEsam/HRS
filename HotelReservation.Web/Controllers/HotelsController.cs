using HotelReservation.Core.DTOs;
using HotelReservation.Core.Models;
using HotelReservation.Data.Context;
using HotelReservation.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace HotelReservation.Web.Controllers;

[Authorize(Roles = "Admin")]
public class HotelsController : Controller
{
    private readonly IHotelService _hotelService;
    private readonly ILogger<HotelsController> _logger;
    private readonly IWebHostEnvironment _environment;
    private readonly ApplicationDbContext _context;

    public HotelsController(
        IHotelService hotelService, 
        ILogger<HotelsController> logger, 
        IWebHostEnvironment environment,
        ApplicationDbContext context)
    {
        _hotelService = hotelService;
        _logger = logger;
        _environment = environment;
        _context = context;
    }

    [AllowAnonymous]
    public async Task<IActionResult> Index()
    {
        var hotels = await _hotelService.GetAllHotelsAsync();
        return View(hotels);
    }

    [AllowAnonymous]
    public async Task<IActionResult> Details(int id)
    {
        var hotel = await _hotelService.GetHotelByIdAsync(id);
        if (hotel == null)
        {
            return NotFound();
        }
        return View(hotel);
    }

    public IActionResult Create()
    {
        return View();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(HotelCreateDto dto, List<IFormFile>? ImageFiles)
    {
        if (ModelState.IsValid)
        {
            try
            {
                // Create hotel first
                var hotelId = await _hotelService.CreateHotelAsync(dto);
                
                // Handle multiple file uploads
                if (ImageFiles != null && ImageFiles.Any())
                {
                    var uploadsFolder = Path.Combine(_environment.WebRootPath, "uploads", "hotels");
                    if (!Directory.Exists(uploadsFolder))
                    {
                        Directory.CreateDirectory(uploadsFolder);
                    }

                    int displayOrder = 0;
                    foreach (var imageFile in ImageFiles.Where(f => f.Length > 0))
                    {
                        var uniqueFileName = Guid.NewGuid().ToString() + "_" + Path.GetFileName(imageFile.FileName);
                        var filePath = Path.Combine(uploadsFolder, uniqueFileName);

                        using (var fileStream = new FileStream(filePath, FileMode.Create))
                        {
                            await imageFile.CopyToAsync(fileStream);
                        }

                        var hotelImage = new HotelImage
                        {
                            HotelId = hotelId,
                            ImageUrl = "/uploads/hotels/" + uniqueFileName,
                            DisplayOrder = displayOrder,
                            IsPrimary = displayOrder == 0 // First image is primary
                        };
                        _context.HotelImages.Add(hotelImage);
                        displayOrder++;
                    }
                    await _context.SaveChangesAsync();
                }
                // If no files uploaded but URL provided, create a HotelImage from URL
                else if (!string.IsNullOrEmpty(dto.ImageUrl))
                {
                    var hotelImage = new HotelImage
                    {
                        HotelId = hotelId,
                        ImageUrl = dto.ImageUrl,
                        DisplayOrder = 0,
                        IsPrimary = true
                    };
                    _context.HotelImages.Add(hotelImage);
                    await _context.SaveChangesAsync();
                }

                TempData["Success"] = "Hotel created successfully";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating hotel");
                ModelState.AddModelError("", "An error occurred while creating the hotel");
            }
        }
        return View(dto);
    }

    public async Task<IActionResult> Edit(int id)
    {
        var hotel = await _hotelService.GetHotelByIdAsync(id);
        if (hotel == null)
        {
            return NotFound();
        }

        var dto = new HotelUpdateDto
        {
            Id = hotel.Id,
            Name = hotel.Name,
            Address = hotel.Address,
            City = hotel.City,
            Country = hotel.Country,
            PhoneNumber = hotel.PhoneNumber,
            Email = hotel.Email,
            StarRating = hotel.StarRating,
            Description = hotel.Description,
            IsActive = hotel.IsActive
        };

        // Pass existing images to view
        ViewBag.ExistingImages = hotel.ImageUrls;
        ViewBag.HotelId = id;

        return View(dto);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, HotelUpdateDto dto, List<IFormFile>? ImageFiles)
    {
        if (id != dto.Id)
        {
            return NotFound();
        }

        if (ModelState.IsValid)
        {
            try
            {
                var result = await _hotelService.UpdateHotelAsync(dto);
                if (!result)
                {
                    return NotFound();
                }

                // Handle new file uploads
                if (ImageFiles != null && ImageFiles.Any())
                {
                    var uploadsFolder = Path.Combine(_environment.WebRootPath, "uploads", "hotels");
                    if (!Directory.Exists(uploadsFolder))
                    {
                        Directory.CreateDirectory(uploadsFolder);
                    }

                    // Get current max display order
                    var maxOrder = await _context.HotelImages
                        .Where(hi => hi.HotelId == id)
                        .MaxAsync(hi => (int?)hi.DisplayOrder) ?? -1;

                    foreach (var imageFile in ImageFiles.Where(f => f.Length > 0))
                    {
                        maxOrder++;
                        var uniqueFileName = Guid.NewGuid().ToString() + "_" + Path.GetFileName(imageFile.FileName);
                        var filePath = Path.Combine(uploadsFolder, uniqueFileName);

                        using (var fileStream = new FileStream(filePath, FileMode.Create))
                        {
                            await imageFile.CopyToAsync(fileStream);
                        }

                        var hotelImage = new HotelImage
                        {
                            HotelId = id,
                            ImageUrl = "/uploads/hotels/" + uniqueFileName,
                            DisplayOrder = maxOrder,
                            IsPrimary = false
                        };
                        _context.HotelImages.Add(hotelImage);
                    }
                    await _context.SaveChangesAsync();
                }

                TempData["Success"] = "Hotel updated successfully";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating hotel");
                ModelState.AddModelError("", "An error occurred while updating the hotel");
            }
        }
        
        // Reload images for the view if validation fails
        var hotel = await _hotelService.GetHotelByIdAsync(id);
        ViewBag.ExistingImages = hotel?.ImageUrls ?? new List<string>();
        ViewBag.HotelId = id;
        
        return View(dto);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteImage(int hotelId, string imageUrl)
    {
        try
        {
            var image = await _context.HotelImages
                .FirstOrDefaultAsync(hi => hi.HotelId == hotelId && hi.ImageUrl == imageUrl);

            if (image != null)
            {
                // Delete physical file
                var filePath = Path.Combine(_environment.WebRootPath, imageUrl.TrimStart('/').Replace('/', Path.DirectorySeparatorChar));
                if (System.IO.File.Exists(filePath))
                {
                    System.IO.File.Delete(filePath);
                }

                _context.HotelImages.Remove(image);
                
                // If deleted image was primary, make another one primary
                if (image.IsPrimary)
                {
                    var nextImage = await _context.HotelImages
                        .Where(hi => hi.HotelId == hotelId && hi.Id != image.Id)
                        .OrderBy(hi => hi.DisplayOrder)
                        .FirstOrDefaultAsync();
                    if (nextImage != null)
                    {
                        nextImage.IsPrimary = true;
                    }
                }
                
                await _context.SaveChangesAsync();
                TempData["Success"] = "Image deleted successfully";
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting image");
            TempData["Error"] = "An error occurred while deleting the image";
        }

        return RedirectToAction(nameof(Edit), new { id = hotelId });
    }

    public async Task<IActionResult> Delete(int id)
    {
        var hotel = await _hotelService.GetHotelByIdAsync(id);
        if (hotel == null)
        {
            return NotFound();
        }
        return View(hotel);
    }

    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        try
        {
            var result = await _hotelService.DeleteHotelAsync(id);
            if (result)
            {
                TempData["Success"] = "Hotel deleted successfully";
                return RedirectToAction(nameof(Index));
            }
            return NotFound();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting hotel");
            TempData["Error"] = "An error occurred while deleting the hotel";
            return RedirectToAction(nameof(Index));
        }
    }
}
