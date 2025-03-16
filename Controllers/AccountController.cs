using System.Security.Claims;
using Microsoft.AspNetCore.Mvc;
using PizzaShop.Entity.ViewModels.AccountVM;
using PizzaShop.Service.Interface;

namespace PizzaShop.Web.Controllers;

public class AccountController : Controller
{

    public readonly IConfiguration _configuration;
    public readonly IAuthenticationService _service;
    public readonly IEmailService _emailService;

    public AccountController(IConfiguration configuration, IAuthenticationService service, IEmailService emailService)
    {
        _configuration = configuration;
        _service = service;
        _emailService = emailService;
    }

    // GET - LOGIN
    [HttpGet]
    public IActionResult Login()
    {
        if (!HttpContext.Request.Cookies.ContainsKey("AuthToken")){
            return View();
        }

        var userRole = User.FindFirst(ClaimTypes.Role)!.Value;

        // return userRole switch
        // {
        //     "Admin" => RedirectToAction("Admindashboard","Home"),
        //     "Account Manager" => RedirectToAction("AccountManagerDashBoard","Home"),
        //     "Chef" => RedirectToAction("ChefDashBoard","Home"),
        //     _ => View(),
        // }; 

        return RedirectToAction("Index", "Home");
    }

    // POST - LOGIN
    [HttpPost]
    public async Task<IActionResult> Login(LoginVM model)
    {

        if (!ModelState.IsValid)
        {
           return View(model);
        }

        var (success, token, isFirstTime,user) = await _service.LoginUser(model);

        if(isFirstTime){
            return  RedirectToAction("Reset","Account", new {token = token});
        }

        if (!success || user == null)
        {
            TempData["error"] = "Invalid Email or Password";
            return RedirectToAction("Login", "Account");
        }

        if (token != null)
        {
            Response.Cookies.Append("AuthToken", token, new CookieOptions
            {
                HttpOnly = false,
                Secure = true,
                SameSite = SameSiteMode.Strict,
                Expires = DateTime.UtcNow.AddMinutes(120)
            }
            );
        }

        return RedirectToAction("Index", "Home");
    }

    // GET - FORGET
    [HttpGet]
    public IActionResult Forget(string? email)
    {
        return View();
    }

    // POST - FORGET
    [HttpPost]
    public async Task<IActionResult> Forget(ForgetPasswordVM model)
    {
         if (!ModelState.IsValid)
        {
            return View(model);
        }

        var (success, token, user) = await _service.ForgotUser(model);

        if (user == null || !success)
        {
            TempData["error"] = "User Does Not Found";
            return RedirectToAction("Forget", "Account");
        }

        var resetLink = Url.Action("Reset", "Account", new { token = token }, Request.Scheme);

        string emailBody = System.IO.File.ReadAllText("wwwroot/html/emailHtml.html");
        emailBody = emailBody.Replace("{resetLink}", resetLink);

        bool isSent = await _emailService.SendEmailAsync(user.Email, "Reset Password", emailBody);

        if (isSent)
        {
            TempData["success"] = "Reset email is sent";
            return RedirectToAction("Login", "Account");
        }

        TempData["success"] = "Failed to send reset email. Please try again.";
        return View();
    }

    [HttpGet]
    public IActionResult LogOut()
    {
        Response.Cookies.Delete("AuthToken");
        return RedirectToAction("Login","Account");
    }

    [HttpGet]
    public IActionResult Reset(string token){
        var (status, message, model) = _service.ValidateResetLink(token);

        if(status == false){
            TempData["error"] = message;
            return RedirectToAction("Login","Account");
        }

        return View(model);
    }

     [HttpPost]
    public async Task<IActionResult> Reset(ResetPasswordVM model)
    {
        if(!ModelState.IsValid){
            return View(model);
        }

        var (status,message) = await _service.ResetPassword(model);


        if(status){
            TempData["success"] = message;
            return RedirectToAction("Login","Account");
        }

        TempData["error"] = message;
        return View(model);

        
    }
    
}
