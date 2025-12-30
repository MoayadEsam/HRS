using HotelReservation.Core.DTOs;
using HotelReservation.Core.Models;
using HotelReservation.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace HotelReservation.Web.Controllers;

[Authorize]
public class ReservationsController : Controller
{
    private readonly IReservationService _reservationService;
    private readonly IRoomService _roomService;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly ILogger<ReservationsController> _logger;

    public ReservationsController(
        IReservationService reservationService,
        IRoomService roomService,
        UserManager<ApplicationUser> userManager,
        ILogger<ReservationsController> logger)
    {
        _reservationService = reservationService;
        _roomService = roomService;
        _userManager = userManager;
        _logger = logger;
    }

    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Index()
    {
        var reservations = await _reservationService.GetAllReservationsAsync();
        return View(reservations);
    }

    public async Task<IActionResult> MyReservations()
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(userId))
        {
            return RedirectToAction("Login", "Account");
        }

        var reservations = await _reservationService.GetReservationsByUserAsync(userId);
        return View(reservations);
    }

    public async Task<IActionResult> Details(int id)
    {
        var reservation = await _reservationService.GetReservationByIdAsync(id);
        if (reservation == null)
        {
            return NotFound();
        }

        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        var isAdmin = User.IsInRole("Admin");

        if (!isAdmin && reservation.UserId != userId)
        {
            return Forbid();
        }

        return View(reservation);
    }

    public async Task<IActionResult> Create(int roomId)
    {
        var room = await _roomService.GetRoomByIdAsync(roomId);
        if (room == null)
        {
            return NotFound();
        }

        ViewBag.Room = room;
        ViewBag.RoomNumber = room.RoomNumber;
        ViewBag.HotelName = room.HotelName;
        ViewBag.PricePerNight = room.PricePerNight;
        
        var dto = new ReservationCreateDto
        {
            RoomId = roomId,
            CheckInDate = DateTime.Today.AddDays(1),
            CheckOutDate = DateTime.Today.AddDays(2),
            NumberOfGuests = 1
        };

        return View(dto);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(ReservationCreateDto dto)
    {
        if (ModelState.IsValid)
        {
            try
            {
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                if (string.IsNullOrEmpty(userId))
                {
                    return RedirectToAction("Login", "Account");
                }

                var reservationId = await _reservationService.CreateReservationAsync(dto, userId);
                TempData["Success"] = "Reservation created successfully";
                return RedirectToAction(nameof(Details), new { id = reservationId });
            }
            catch (ArgumentException ex)
            {
                ModelState.AddModelError("", ex.Message);
            }
            catch (InvalidOperationException ex)
            {
                ModelState.AddModelError("", ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating reservation");
                ModelState.AddModelError("", "An error occurred while creating the reservation");
            }
        }

        var room = await _roomService.GetRoomByIdAsync(dto.RoomId);
        ViewBag.Room = room;
        if (room != null)
        {
            ViewBag.RoomNumber = room.RoomNumber;
            ViewBag.HotelName = room.HotelName;
            ViewBag.PricePerNight = room.PricePerNight;
        }
        return View(dto);
    }

    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Edit(int id)
    {
        var reservation = await _reservationService.GetReservationByIdAsync(id);
        if (reservation == null)
        {
            return NotFound();
        }

        var dto = new ReservationUpdateDto
        {
            Id = reservation.Id,
            CheckInDate = reservation.CheckInDate,
            CheckOutDate = reservation.CheckOutDate,
            NumberOfGuests = reservation.NumberOfGuests,
            SpecialRequests = reservation.SpecialRequests,
            Status = reservation.Status
        };

        return View(dto);
    }

    [HttpPost]
    [Authorize(Roles = "Admin")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, ReservationUpdateDto dto)
    {
        if (id != dto.Id)
        {
            return NotFound();
        }

        if (ModelState.IsValid)
        {
            try
            {
                var result = await _reservationService.UpdateReservationAsync(dto);
                if (result)
                {
                    TempData["Success"] = "Reservation updated successfully";
                    return RedirectToAction(nameof(Details), new { id });
                }
                return NotFound();
            }
            catch (ArgumentException ex)
            {
                ModelState.AddModelError("", ex.Message);
            }
            catch (InvalidOperationException ex)
            {
                ModelState.AddModelError("", ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating reservation");
                ModelState.AddModelError("", "An error occurred while updating the reservation");
            }
        }

        return View(dto);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Cancel(int id)
    {
        var reservation = await _reservationService.GetReservationByIdAsync(id);
        if (reservation == null)
        {
            return NotFound();
        }

        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        var isAdmin = User.IsInRole("Admin");

        if (!isAdmin && reservation.UserId != userId)
        {
            return Forbid();
        }

        try
        {
            var result = await _reservationService.CancelReservationAsync(id);
            if (result)
            {
                TempData["Success"] = "Reservation cancelled successfully";
                return RedirectToAction(nameof(MyReservations));
            }
            return NotFound();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error cancelling reservation");
            TempData["Error"] = "An error occurred while cancelling the reservation";
            return RedirectToAction(nameof(MyReservations));
        }
    }

    [HttpGet]
    public async Task<IActionResult> CalculatePrice(int roomId, DateTime checkIn, DateTime checkOut)
    {
        try
        {
            var price = await _reservationService.CalculateTotalPriceAsync(roomId, checkIn, checkOut);
            return Json(new { success = true, totalPrice = price });
        }
        catch (Exception ex)
        {
            return Json(new { success = false, message = ex.Message });
        }
    }
}
