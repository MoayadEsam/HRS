using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HotelReservation.Web.Controllers;

[Authorize(Roles = "Admin")]
public class ManagementController : Controller
{
    public IActionResult Index()
    {
        return View();
    }
}
