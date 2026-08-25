using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using UniversityCourseEnrollment.Models;
using Microsoft.EntityFrameworkCore;
using UniversityCourseEnrollment.Data;
using UniversityCourseEnrollment.Models.ViewModels;
using Microsoft.AspNetCore.Diagnostics;

namespace UniversityCourseEnrollment.Controllers;

public class HomeController : Controller
{
    private readonly ApplicationDbContext _context;
    private readonly ILogger<HomeController> _logger;

    public HomeController(ApplicationDbContext context, ILogger<HomeController> logger)
    {
        _context = context;
        _logger = logger;
    }
    
    public IActionResult Index()
    {
        return View("Login");
    }
    public IActionResult Privacy()
    {
        return View();
    }

    [HttpGet]
    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        var exceptionFeature = HttpContext.Features.Get<IExceptionHandlerPathFeature>();
        if (exceptionFeature?.Error is not null)
        {
            _logger.LogError(exceptionFeature.Error, "Unhandled exception while processing {Path}", exceptionFeature.Path);
        }

        return View(new ErrorViewModel
        {
            RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier
        });
    }

    [HttpGet]
    public IActionResult Login()
    {
        return View();
    }

   
}

  
