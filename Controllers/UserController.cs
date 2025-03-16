using System.Security.Claims;
using Microsoft.AspNetCore.Mvc;
using PizzaShop.Entity.ViewModels.UserVM;
using PizzaShop.Service.Interface;

namespace PizzaShop.Web.Controllers;

public class UserController : Controller
{
    private readonly IUserService _service;
    private readonly IEmailService _emailService;

    public UserController(IUserService service, IEmailService emailService )
    {
        _service = service;
        _emailService = emailService;
    }

    #region SHOW USER LIST
    [HttpGet]
    public async Task<IActionResult> UserList(string? searchString, string? sorting, int pageIndex = 1, int pageSize = 5)
    {
       var userList = await  _service.GetUsersAsync(searchString,sorting,pageIndex,pageSize);

        if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
        {
            return PartialView("_UserPartialView", userList); 
        }   
        // return PartialView("_UserPartialView", userList);
        ViewData["userList"] = userList;
        return View();
    }
    #endregion

    #region ADD USER

    [HttpGet]
    public IActionResult AddUser()
    {
        var user = _service.Adduser(); 
        return View(user);
    }

    [HttpPost]
    public async Task<IActionResult> AddUser(UserVM user){

        if(!ModelState.IsValid){
            return View(user);
        }
        var createrId = Int32.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);

        var(status, message) = await _service.PostAdduser(user, createrId);

        if(!status)
        {
            TempData["error"] = message;
            return RedirectToAction("AddUser");
        }

        string emailBodyStr = System.IO.File.ReadAllText("wwwroot/html/newUserMail.html");
        string emailBody = string.Format(emailBodyStr,user.UserName,user.Password);

        bool isSent = await _emailService.SendEmailAsync(user.Email!, "Reset Password", emailBody);

        if (isSent)
        {
            TempData["success"] = "Reset email is sent";
            return RedirectToAction("UserList", "User");
        }

        TempData["success"] = message;
        return RedirectToAction("UserList","User");
    }
    #endregion

    #region EDIT USER
    [HttpGet]
    public async Task<IActionResult> EditUserAsync(int id)
    {

        var(status, message, userModel) = await _service.GetUserProfile(id);

        if(!status) 
        {
            return View();
        }
        
        return View(userModel);
    }

    [HttpPost]
    public JsonResult AjaxMethod(string type, int value)
    {
        
        return Json(_service.GetCaseCadeDropDown(type,value));
    }

    [HttpPost]
    public async Task<IActionResult> EditUserAsync(UserVM editUser)
    {
        var modifierId = Int32.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value); 

        if(!ModelState.IsValid)
        {
            return View(editUser);
        }
        
        var(status, message) = await _service.EditUserProfile(editUser, modifierId);
        if (!status)
        {
            TempData["error"] = message;
            return View(editUser);
        }

        TempData["success"] = message;
        return RedirectToAction("UserList","User");
    }
    #endregion

    #region DELETE USER
    [HttpGet]
    public async Task<IActionResult> DeleteUser(int id){
        
        var (status, message) = await _service.DeleteUser(id);
        
        if(!status)
        {
            TempData["error"] =message;
            return RedirectToAction("UserList","User");
        }
        TempData["success"] = message;
        return RedirectToAction("UserList","User");
    }
    #endregion
   
}
