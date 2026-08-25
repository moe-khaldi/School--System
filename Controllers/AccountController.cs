using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using UniversityCourseEnrollment.Data;
using UniversityCourseEnrollment.Models;
using UniversityCourseEnrollment.Models.ViewModels;
using UniversityCourseEnrollment.Models.Enums;

namespace UniversityCourseEnrollment.Controllers;

public class AccountController : Controller
{
    private readonly ApplicationDbContext _context;

    public AccountController(ApplicationDbContext context)
    {
        _context = context;
    }

    [HttpGet]
    public IActionResult Login()
    {
        return View("~/Views/Home/Login.cshtml");
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Login(LoginViewModel model)
    {
        if (!ModelState.IsValid)
        {
            return View("~/Views/Home/Login.cshtml", model);
        }

        var user = await _context.AppUsers
            .FirstOrDefaultAsync(appUser =>
                appUser.Email == model.Email);

        if (user == null || user.Password != model.Password)
        {
            ModelState.AddModelError(string.Empty, "Invalid email or password.");
            return View("~/Views/Home/Login.cshtml", model);
        }

        var claims = new List<Claim>
        {
            new Claim(ClaimTypes.NameIdentifier, user.AppUserId.ToString()),
            new Claim(ClaimTypes.Name, user.FullName),
            new Claim(ClaimTypes.Email, user.Email),
            new Claim(ClaimTypes.Role, user.Role.ToString())
        };

        var claimsIdentity = new ClaimsIdentity(
            claims,
           authenticationType: "UniversityCookie");
        

        var claimsPrincipal = new ClaimsPrincipal(claimsIdentity);
         await HttpContext.SignInAsync(
            "UniversityCookie",
            claimsPrincipal);

        return user.Role switch
        {
            UserRole.Admin => RedirectToAction("Dashboard", "Admin"),
            UserRole.Teacher => RedirectToAction("Dashboard", "Teacher"),
            UserRole.Student => RedirectToAction("Dashboard", "Student"),
            _ => RedirectToAction("Index", "Home")
        };
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Logout()
    {
        await HttpContext.SignOutAsync(
            "UniversityCookie"
        );

        return RedirectToAction(nameof(Login));
    }

    [HttpGet]
    public IActionResult AccessDenied()
    {
        return View();
    }
}
