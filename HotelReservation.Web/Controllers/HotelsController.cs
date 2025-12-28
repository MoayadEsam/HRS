using HotelReservation.Core.DTOs;
using HotelReservation.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HotelReservation.Web.Controllers;

[Authorize(Roles = "Admin")]
public class HotelsController : Controller
{
    private readonly IHotelService _hotelService;
    private readonly ILogger<HotelsController> _logger;
    private readonly IWebHostEnvironment _environment;

    public HotelsController(IHotelService hotelService, ILogger<HotelsController> logger, IWebHostEnvironment environment)
    {
        _hotelService = hotelService;
        _logger = logger;
        _environment = environment;
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
    public async Task<IActionResult> Create(HotelCreateDto dto, IFormFile? ImageFile)
    {
        if (ModelState.IsValid)
        {
            try
            {
                // Handle file upload
                if (ImageFile != null && ImageFile.Length > 0)
                {
                    var uploadsFolder = Path.Combine(_environment.WebRootPath, "uploads", "hotels");
                    if (!Directory.Exists(uploadsFolder))
                    {
                        Directory.CreateDirectory(uploadsFolder);
                    }

                    var uniqueFileName = Guid.NewGuid().ToString() + "_" + Path.GetFileName(ImageFile.FileName);
                    var filePath = Path.Combine(uploadsFolder, uniqueFileName);

                    using (var fileStream = new FileStream(filePath, FileMode.Create))
                    {
                        await ImageFile.CopyToAsync(fileStream);
                    }

                    dto.ImageUrl = "/uploads/hotels/" + uniqueFileName;
                }

                await _hotelService.CreateHotelAsync(dto);
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

        return View(dto);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, HotelUpdateDto dto)
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
                if (result)
                {
                    TempData["Success"] = "Hotel updated successfully";
                    return RedirectToAction(nameof(Index));
                }
                return NotFound();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating hotel");
                ModelState.AddModelError("", "An error occurred while updating the hotel");
            }
        }
        return View(dto);
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
