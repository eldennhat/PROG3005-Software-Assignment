using Microsoft.AspNetCore.Mvc;
using StudentManagement.Models;

namespace StudentManagement.Controllers;

public class StudentController : Controller {
    // GET
    public IActionResult Info(int id, string name)
    {
        ViewBag.StudentName = name;
        ViewData["Age"] = id;

        StudentViewModel student = new StudentViewModel() {
            Major = "CNTT"
        };
        return View(student);
    }
}