using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using HotelReservation.Web.Models;
using HotelReservation.Services.Interfaces;

namespace HotelReservation.Web.Controllers;

public class HomeController : Controller
{
    private readonly ILogger<HomeController> _logger;
    private readonly IHotelService _hotelService;
    private readonly IRoomService _roomService;

    public HomeController(ILogger<HomeController> logger, IHotelService hotelService, IRoomService roomService)
    {
        _logger = logger;
        _hotelService = hotelService;
        _roomService = roomService;
    }

    public async Task<IActionResult> Index()
    {
        var hotels = await _hotelService.GetActiveHotelsAsync();
        return View(hotels);
    }

    public IActionResult Privacy()
    {
        return View();
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}
