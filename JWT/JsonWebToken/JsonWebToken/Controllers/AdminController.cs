using JsonWebToken.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace JsonWebToken.Controllers;

public class AdminController : Controller
{
    private readonly ApplicationDbContext _context;

    public AdminController(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<IActionResult> Index()
    {
        if (HttpContext.Session.GetString("UserRole") != "Admin")
        {
            return RedirectToAction("Login", "Account");
        }

        ViewBag.ActiveTab = "scheduling";

        var movies = await _context.Movies
            .Include(m => m.MovieGenres)
            .ThenInclude(mg => mg.Genre)
            .OrderByDescending(m => m.ReleaseDate)
            .ToListAsync();

        return View("~/Views/Staff/Index.cshtml", movies);
    }
}
