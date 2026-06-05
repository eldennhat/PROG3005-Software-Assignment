using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using StudentManagement.Models;

namespace StudentManagement.Controllers;

public class HomeController : Controller {
    private readonly ILogger<HomeController> _logger;

    public HomeController(ILogger<HomeController> logger)
    {
        _logger = logger;
    }

    public IActionResult Index()
    {
        ViewData["Message"] = "Welcome to ASP.NET Core MVC";
        return View();
    }

    public IActionResult About()
    {
        ViewBag.StudentName = "Nhat Minh Quang";
        return View();
    }
    
    public IActionResult Contact()
    {
        ViewBag.Mail = "BIT24@st.cmcu.edu.vn";
        return View();
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