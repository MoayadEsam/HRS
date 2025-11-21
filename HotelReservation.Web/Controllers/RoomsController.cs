using HotelReservation.Core.DTOs;
using HotelReservation.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace HotelReservation.Web.Controllers;

public class RoomsController : Controller
{
    private readonly IRoomService _roomService;
    private readonly IHotelService _hotelService;
    private readonly IAmenityService _amenityService;
    private readonly ILogger<RoomsController> _logger;

    public RoomsController(
        IRoomService roomService,
        IHotelService hotelService,
        IAmenityService amenityService,
        ILogger<RoomsController> logger)
    {
        _roomService = roomService;
        _hotelService = hotelService;
        _amenityService = amenityService;
        _logger = logger;
    }

    public async Task<IActionResult> Index()
    {
        var rooms = await _roomService.GetAllRoomsAsync();
        return View(rooms);
    }

    public async Task<IActionResult> Search()
    {
        ViewBag.Hotels = new SelectList(await _hotelService.GetActiveHotelsAsync(), "Id", "Name");
        ViewBag.Amenities = await _amenityService.GetActiveAmenitiesAsync();
        return View();
    }

    [HttpPost]
    public async Task<IActionResult> Search(RoomSearchDto searchDto)
    {
        var rooms = await _roomService.SearchRoomsAsync(searchDto);
        ViewBag.Hotels = new SelectList(await _hotelService.GetActiveHotelsAsync(), "Id", "Name");
        ViewBag.Amenities = await _amenityService.GetActiveAmenitiesAsync();
        ViewBag.SearchPerformed = true;
        return View(rooms);
    }

    public async Task<IActionResult> Details(int id)
    {
        var room = await _roomService.GetRoomByIdAsync(id);
        if (room == null)
        {
            return NotFound();
        }
        return View(room);
    }

    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Create()
    {
        await LoadViewData();
        return View();
    }

    [HttpPost]
    [Authorize(Roles = "Admin")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(RoomCreateDto dto)
    {
        if (ModelState.IsValid)
        {
            try
            {
                await _roomService.CreateRoomAsync(dto);
                TempData["Success"] = "Room created successfully";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating room");
                ModelState.AddModelError("", "An error occurred while creating the room");
            }
        }

        await LoadViewData();
        return View(dto);
    }

    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Edit(int id)
    {
        var room = await _roomService.GetRoomByIdAsync(id);
        if (room == null)
        {
            return NotFound();
        }

        var dto = new RoomUpdateDto
        {
            Id = room.Id,
            RoomNumber = room.RoomNumber,
            Type = room.Type,
            Capacity = room.Capacity,
            PricePerNight = room.PricePerNight,
            Description = room.Description,
            FloorNumber = room.FloorNumber,
            IsAvailable = room.IsAvailable,
            HotelId = room.HotelId,
            AmenityIds = room.Amenities.Select(a => a.Id).ToList()
        };

        await LoadViewData();
        return View(dto);
    }

    [HttpPost]
    [Authorize(Roles = "Admin")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, RoomUpdateDto dto)
    {
        if (id != dto.Id)
        {
            return NotFound();
        }

        if (ModelState.IsValid)
        {
            try
            {
                var result = await _roomService.UpdateRoomAsync(dto);
                if (result)
                {
                    TempData["Success"] = "Room updated successfully";
                    return RedirectToAction(nameof(Index));
                }
                return NotFound();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating room");
                ModelState.AddModelError("", "An error occurred while updating the room");
            }
        }

        await LoadViewData();
        return View(dto);
    }

    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Delete(int id)
    {
        var room = await _roomService.GetRoomByIdAsync(id);
        if (room == null)
        {
            return NotFound();
        }
        return View(room);
    }

    [HttpPost, ActionName("Delete")]
    [Authorize(Roles = "Admin")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        try
        {
            var result = await _roomService.DeleteRoomAsync(id);
            if (result)
            {
                TempData["Success"] = "Room deleted successfully";
                return RedirectToAction(nameof(Index));
            }
            return NotFound();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting room");
            TempData["Error"] = "Cannot delete room with existing reservations";
            return RedirectToAction(nameof(Index));
        }
    }

    private async Task LoadViewData()
    {
        ViewBag.Hotels = new SelectList(await _hotelService.GetAllHotelsAsync(), "Id", "Name");
        ViewBag.Amenities = await _amenityService.GetActiveAmenitiesAsync();
    }
}
