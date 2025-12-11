using HotelReservation.Core.DTOs;
using HotelReservation.Core.Models;
using HotelReservation.Data.Context;
using HotelReservation.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace HotelReservation.Web.Controllers;

public class AccountController : Controller
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly SignInManager<ApplicationUser> _signInManager;
    private readonly ApplicationDbContext _context;
    private readonly ILogger<AccountController> _logger;

    public AccountController(
        UserManager<ApplicationUser> userManager,
        SignInManager<ApplicationUser> signInManager,
        ApplicationDbContext context,
        ILogger<AccountController> logger)
    {
        _userManager = userManager;
        _signInManager = signInManager;
        _context = context;
        _logger = logger;
    }

    [HttpGet]
    public IActionResult Register()
    {
        if (User.Identity?.IsAuthenticated ?? false)
        {
            return RedirectToAction("Index", "Home");
        }
        return View();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Register(RegisterDto model)
    {
        if (ModelState.IsValid)
        {
            var user = new ApplicationUser
            {
                UserName = model.Email,
                Email = model.Email,
                FirstName = model.FirstName,
                LastName = model.LastName
            };

            var result = await _userManager.CreateAsync(user, model.Password);

            if (result.Succeeded)
            {
                await _userManager.AddToRoleAsync(user, "User");
                await _signInManager.SignInAsync(user, isPersistent: false);
                _logger.LogInformation("User created a new account with password.");
                TempData["Success"] = "Registration successful!";
                return RedirectToAction("Index", "Home");
            }

            foreach (var error in result.Errors)
            {
                ModelState.AddModelError(string.Empty, error.Description);
            }
        }

        return View(model);
    }

    [HttpGet]
    public IActionResult Login(string? returnUrl = null)
    {
        if (User.Identity?.IsAuthenticated ?? false)
        {
            return RedirectToAction("Index", "Home");
        }
        
        ViewData["ReturnUrl"] = returnUrl;
        return View();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Login(LoginDto model, string? returnUrl = null)
    {
        returnUrl ??= Url.Content("~/");

        if (ModelState.IsValid)
        {
            var result = await _signInManager.PasswordSignInAsync(
                model.Email, 
                model.Password, 
                model.RememberMe, 
                lockoutOnFailure: false);

            if (result.Succeeded)
            {
                _logger.LogInformation("User logged in.");
                return LocalRedirect(returnUrl);
            }
            else
            {
                ModelState.AddModelError(string.Empty, "Invalid login attempt.");
            }
        }

        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Logout()
    {
        await _signInManager.SignOutAsync();
        _logger.LogInformation("User logged out.");
        return RedirectToAction("Index", "Home");
    }

    [HttpGet]
    public IActionResult AccessDenied()
    {
        return View();
    }

    [HttpGet]
    public IActionResult StaffRegister()
    {
        if (User.Identity?.IsAuthenticated ?? false)
        {
            return RedirectToAction("Index", "Home");
        }
        return View();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> StaffRegister(StaffRegistrationDto model)
    {
        if (ModelState.IsValid)
        {
            // Find employee by code and email
            var employee = await _context.Employees
                .FirstOrDefaultAsync(e => e.EmployeeCode == model.EmployeeCode && 
                                          e.Email.ToLower() == model.Email.ToLower() &&
                                          e.IsActive);

            if (employee == null)
            {
                ModelState.AddModelError(string.Empty, "Invalid employee code or email. Please contact your administrator.");
                return View(model);
            }

            if (!string.IsNullOrEmpty(employee.UserId))
            {
                ModelState.AddModelError(string.Empty, "This employee already has an account. Please login instead.");
                return View(model);
            }

            // Create user account
            var user = new ApplicationUser
            {
                UserName = employee.Email,
                Email = employee.Email,
                FirstName = employee.FirstName,
                LastName = employee.LastName,
                EmailConfirmed = true
            };

            var result = await _userManager.CreateAsync(user, model.Password);

            if (result.Succeeded)
            {
                // Assign Staff role
                await _userManager.AddToRoleAsync(user, "Staff");

                // Link employee to user
                employee.UserId = user.Id;
                await _context.SaveChangesAsync();

                await _signInManager.SignInAsync(user, isPersistent: false);
                _logger.LogInformation("Staff member registered and logged in: {Email}", user.Email);
                TempData["Success"] = "Registration successful! Welcome to the team.";
                return RedirectToAction("Index", "StaffProfile");
            }

            foreach (var error in result.Errors)
            {
                ModelState.AddModelError(string.Empty, error.Description);
            }
        }

        return View(model);
    }
}
