using Microsoft.AspNetCore.Mvc;
using PizzaShop.Entity.Models;
using PizzaShop.Entity.ViewModels.AccountVM;


namespace PizzaShop.Service.Interface;

public interface IAuthenticationService
{
    Task<(bool success, string? token,bool isFirstTime, User? user)> LoginUser(LoginVM model);

    Task<(bool success, string? token, User? user)> ForgotUser(ForgetPasswordVM model);

    (bool status, string? message, ResetPasswordVM? model) ValidateResetLink(string token);

    Task<(bool status,string? message)> ResetPassword(ResetPasswordVM model);
}
