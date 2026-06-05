using FormSubmit.Models;
using Microsoft.AspNetCore.Mvc;

namespace FormSubmit.Controllers;

public class AccountController : Controller {
    // GET
    [HttpGet]
    public IActionResult Login() {
        return View();
    }
    
    //Post: Account/Login
    [HttpPost]
    public IActionResult Login(LoginViewModel model) {
        if (model.username == "admin" && model.password == "123") {
            return RedirectToAction("LoginSuccess");
        }
        ViewBag.ErrorMessage = "Invalid username or password";
        return View(model);
    }
    
    public IActionResult LoginSuccess() {
        return View();
    }
    
}