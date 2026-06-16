using ImageUpload.Data;
using ImageUpload.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Hosting;
using System.IO;

namespace ImageUpload.Controllers
{
    public class BookController : Controller
    {
        private readonly BookRepository _bookRepository;
        private readonly IWebHostEnvironment _env;
        
        public BookController(BookRepository bookRepository, IWebHostEnvironment env)
        {
            _bookRepository = bookRepository;
            _env = env;
        }

        public IActionResult Index()
        {
            return View(_bookRepository.GetAll());
        }

        public IActionResult Detail(int id)
        {
            var book = _bookRepository.GetById(id);
            if (book == null) return NotFound();
            return View(book);
        }

        [HttpGet]
        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Create(Book book, IFormFile imageFile)
        {
            if (imageFile != null && imageFile.Length > 0)
            {
                // Kiểm tra định dạng (Chỉ cho png, jpg)
                var extension = Path.GetExtension(imageFile.FileName).ToLower();
                if (extension != ".jpg" && extension != ".png")
                {
                    ModelState.AddModelError("ImageFile", "Lỗi: Chỉ cho phép upload file .jpg hoặc .png");
                    return View(book); 
                }

                // Tạo đường dẫn lưu file
                string uploadsFolder = Path.Combine(_env.WebRootPath, "images"); // trỏ vào wwwroot/images
                if (!Directory.Exists(uploadsFolder))
                {
                    Directory.CreateDirectory(uploadsFolder); // Tạo thư mục nếu chưa có
                }

                // Đổi tên file để không bị trùng 
                string uniqueFileName = Guid.NewGuid().ToString() + "_" + imageFile.FileName;
                string filePath = Path.Combine(uploadsFolder, uniqueFileName);

                // Lưu file vật lý vào server
                using (var fileStream = new FileStream(filePath, FileMode.Create))
                {
                    imageFile.CopyTo(fileStream);
                }

                // Gán đường dẫn vào Model để lưu xuống Database
                book.ImageUrl = "/images/" + uniqueFileName;
            }
            else
            {
                ModelState.AddModelError("ImageFile", "Vui lòng chọn hình ảnh!");
                return View(book);
            }

            // Gọi Repository để lưu sách vào DB 
            _bookRepository.Add(book);
            return RedirectToAction("Index");
        }
            

        public IActionResult Edit(int id)
        {
            var book = _bookRepository.GetById(id);
            if (book == null)
            {
                return NotFound();
            }

            return View(book);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Edit(int id, Book book)
        {
            if (id != book.Id)
            {
                return BadRequest();
            }

            if (!ModelState.IsValid)
            {
                return View(book);
            }

            if (!_bookRepository.Update(book))
            {
                return NotFound();
            }

            TempData["SuccessMessage"] = "Cập nhật sách thành công!";
            return RedirectToAction(nameof(Index));
        }

        public IActionResult Delete(int id)
        {
            var book = _bookRepository.GetById(id);
            if (book == null)
            {
                return NotFound();
            }

            return View(book);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public IActionResult DeleteConfirmed(int id)
        {
            if (!_bookRepository.Delete(id))
            {
                return NotFound();
            }

            TempData["SuccessMessage"] = "Xóa sách thành công!";
            return RedirectToAction(nameof(Index));
        }
    }
}
