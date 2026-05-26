using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using StudentManagement.Models;

namespace StudentManagement.Controllers;

public class ProductController : Controller {
    
    public IActionResult Detail(int id)
    {
        ViewBag.ProductID = id ;
        return View("ProductDetail");
    }
    
    public IActionResult Category(string name)
    {
        ViewBag.CategoryName = name;
        return View();
    }


}