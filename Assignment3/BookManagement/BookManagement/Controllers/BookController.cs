using BookManagement.Models;
using Microsoft.AspNetCore.Mvc;

namespace BookManagement.Controllers;

public class BookController : Controller {
    // GET
    public IActionResult Index()
    {
        var books = new List<BookViewModel> {
            new BookViewModel { Id = 1, Name = "Clean Code", Price = "2000" },
            new BookViewModel { Id = 2, Name = "ASP.NET MVC", Price = "1500" },
            new BookViewModel { Id = 3, Name = "Design Pattern", Price = "2500" }
        };
        return View(books);
    }

    public IActionResult Details(int id)
    {
        var books = new List<BookViewModel> {
            new BookViewModel { Id = 1, Name = "Clean Code", Price = "2000" },
            new BookViewModel { Id = 2, Name = "ASP.NET MVC", Price = "1500" },
            new BookViewModel { Id = 3, Name = "Design Pattern", Price = "2500" }
        };
        
        var book = books.FirstOrDefault(b => b.Id == id);
        if (book == null) return NotFound();
        return View(book);
    }
    
    public IActionResult Create()
    {
        return View(); //run by Create.cshtml
    }
    
    //Post: Book/Create
    [HttpPost]
    public IActionResult Create(BookViewModel book)
    {
        TempData["SuccessMessage"] = "Book" + book.Name + "added successfully!";
        return RedirectToAction("Index");
    }
}