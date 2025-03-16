using System.Diagnostics;
using System.Security.Claims;
using Microsoft.AspNetCore.Mvc;
using PizzaShop.Entity.ViewModels.HomeVM;
using PizzaShop.Entity.ViewModels.UserVM;
using PizzaShop.Service.Interface;
using PizzaShop.Web.Models;

namespace PizzaShop.Web.Controllers;

public class HomeController : Controller
{
    private readonly ILogger<HomeController> _logger;
    private readonly IHomeService _service;

    public HomeController(ILogger<HomeController> logger,IHomeService service)
    {
        _logger = logger;
        _service = service;
    }

    public IActionResult Index()
    {
        return View();
    }

    #region CHANGE PASSWORD
    [HttpGet]
    public IActionResult ChangePassword()
    {
        return View();
    }

    [HttpPost]
    public async Task<IActionResult> ChangePasswordAsync(ChangePasswordVM model)
    {
        if(!ModelState.IsValid){
            return View(model);
        }
        var userEmail = User.FindFirst(ClaimTypes.Email)!.Value;

        var (status,message) = await _service.ChangePassword(model,userEmail);

        if(!status)
        {
            TempData["error"] = message;
            return RedirectToAction("ChangePassword","Home");
        }

        Response.Cookies.Delete("AuthToken");
        TempData["success"] = message;
        return RedirectToAction("Login","Account");
    }
    #endregion
    
    #region MY PROFILE
    [HttpGet]
    public async Task<IActionResult> MyProfileAsync()
    {
        var userEmail =  User.FindFirst(ClaimTypes.Email)!.Value;   
        var userRole = User.FindFirst(ClaimTypes.Role)!.Value;

        var (status, message, userVM) = await _service.GetMyProfile(userEmail, userRole);

        if (!status)
        {
            TempData["error"] =message;
            return View();
        }

        return View(userVM);
    }

    [HttpPost]
    public JsonResult AjaxMethod(string type, int value)
    {
        
        return Json(_service.GetCaseCadeDropDown(type,value));
    }

    [HttpPost]
    public async Task<IActionResult> MyProfileAsync(UserVM  myProfile)
    {
        if (!ModelState.IsValid)
        {
            return View(myProfile);
        }
        
        var userEmail = User.FindFirst(ClaimTypes.Email)!.Value;

        var (status, message) = await _service.PostMyProfile(myProfile, userEmail);
        
        if(!status)
        {
            TempData["error"] = message;
            return View();
        }

        TempData["success"] = message;
        return RedirectToAction("MyProfile", "Home");
    }
    #endregion

}
