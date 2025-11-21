using HotelReservation.Core.DTOs;
using HotelReservation.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HotelReservation.Web.Controllers;

[Authorize(Roles = "Admin")]
public class AmenitiesController : Controller
{
    private readonly IAmenityService _amenityService;
    private readonly ILogger<AmenitiesController> _logger;

    public AmenitiesController(IAmenityService amenityService, ILogger<AmenitiesController> logger)
    {
        _amenityService = amenityService;
        _logger = logger;
    }

    public async Task<IActionResult> Index()
    {
        var amenities = await _amenityService.GetAllAmenitiesAsync();
        return View(amenities);
    }

    public IActionResult Create()
    {
        return View();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(AmenityCreateDto dto)
    {
        if (ModelState.IsValid)
        {
            try
            {
                await _amenityService.CreateAmenityAsync(dto);
                TempData["Success"] = "Amenity created successfully";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating amenity");
                ModelState.AddModelError("", "An error occurred while creating the amenity");
            }
        }
        return View(dto);
    }

    public async Task<IActionResult> Edit(int id)
    {
        var amenity = await _amenityService.GetAmenityByIdAsync(id);
        if (amenity == null)
        {
            return NotFound();
        }

        var dto = new AmenityUpdateDto
        {
            Id = amenity.Id,
            Name = amenity.Name,
            Description = amenity.Description,
            IconClass = amenity.IconClass,
            IsActive = amenity.IsActive
        };

        return View(dto);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, AmenityUpdateDto dto)
    {
        if (id != dto.Id)
        {
            return NotFound();
        }

        if (ModelState.IsValid)
        {
            try
            {
                var result = await _amenityService.UpdateAmenityAsync(dto);
                if (result)
                {
                    TempData["Success"] = "Amenity updated successfully";
                    return RedirectToAction(nameof(Index));
                }
                return NotFound();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating amenity");
                ModelState.AddModelError("", "An error occurred while updating the amenity");
            }
        }
        return View(dto);
    }

    public async Task<IActionResult> Delete(int id)
    {
        var amenity = await _amenityService.GetAmenityByIdAsync(id);
        if (amenity == null)
        {
            return NotFound();
        }
        return View(amenity);
    }

    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        try
        {
            var result = await _amenityService.DeleteAmenityAsync(id);
            if (result)
            {
                TempData["Success"] = "Amenity deleted successfully";
                return RedirectToAction(nameof(Index));
            }
            return NotFound();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting amenity");
            TempData["Error"] = "Cannot delete amenity currently in use";
            return RedirectToAction(nameof(Index));
        }
    }
}
