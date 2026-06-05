using Microsoft.AspNetCore.Mvc;
using Validation.Models;

namespace Validation.Controllers;

public class BookController : Controller {
    // GET
    public IActionResult Index()
    {
        //Tạo danh sách sách mẫu
        var books = new List<BookViewModel> {
            new BookViewModel { Id = 1, Name = "Clean Code", Price = 2000 },
            new BookViewModel { Id = 2, Name = "ASP.NET MVC", Price = 1500 },
            new BookViewModel { Id = 3, Name = "Design Pattern", Price = 2500 }
        };
        //Truyền danh sách sang View
        return View(books);
    }

    public IActionResult Detail(int id)
    {
        //Tạo lại ds mẫu
        var books = new List<BookViewModel> {
            new BookViewModel() { Id = 1, Name = "Clean Code", Price = 2000 },
            new BookViewModel() { Id = 2, Name = "ASP.NET MVC", Price = 1500 },
            new BookViewModel() { Id = 3, Name = "Design Pattern", Price = 2500 }
        };

        var book = books.FirstOrDefault(b => b.Id == id);
        if (book == null) {
            return NotFound();
        }

        //Truyền sách sang View
        return View(book);
    }

    public IActionResult Create()
    {
        return View(); //Tìm đến Create.cshtml để chạy
    }

    //POST: Book/Create
    [HttpPost]
    public IActionResult Create(BookViewModel book)
    {
        if (!ModelState.IsValid) {
            //Nếu có lỗi validation, trả về lại view với dữ liệu đã nhập để hiển thị lỗi
            return View(book); //giữ lại phần dữ liệu đã nhập để hiển thị lỗi
        }

        //Thông báo thành công
        TempData["SuccessMessage"] = "Thêm sách \"" + book.Name + "\" thành công!";
        return RedirectToAction("Create"); //Quay lại trang tạo sách sau khi thêm thành công
    }
}