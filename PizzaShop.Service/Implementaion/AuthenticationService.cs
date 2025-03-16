using System;
using Microsoft.AspNetCore.Cryptography.KeyDerivation;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using PizzaShop.Entity.Models;
using PizzaShop.Entity.ViewModels.AccountVM;

using PizzaShop.Repository.Interface;
using PizzaShop.Service.Helper;
using PizzaShop.Service.Interface;

namespace PizzaShop.Service.Implementaion;

public class AuthenticationService : IAuthenticationService
{
    public readonly IUserRepository _repository;
    public readonly ITokenService _token;

    public AuthenticationService (IUserRepository repository, ITokenService token){
        _repository = repository;
        _token = token;
    }

    // LOGIN
    public async  Task<(bool success, string? token,bool isFirstTime, User? user)> LoginUser(LoginVM model)
    {
        var user = await _repository.GetUserByEmailAsync(model.Email);

        string token = "";
        if (user == null || !Hashing.VerifyPassword(model.Password,user.Password))
        {
            return (false, null, false, null);
        }

        if(user!.IsFirstTime.GetValueOrDefault()){
            token = _token.GenerateResetToken(user.Email);
            return(true,token,true,user);
        }
        
        var timeSpan = TimeSpan.FromHours(24); // FOR NORMAL LOGIN
        if (model.RememberMe)
        {
            timeSpan = TimeSpan.FromDays(30); // FOR REMEMBER ME FUNCTIONALITY
        }

        token = _token.GenerateAuthToken(user!, timeSpan);

        return (true, token, false ,user);
    }

    // FORGET
    public async Task<(bool success, string? token, User? user)> ForgotUser(ForgetPasswordVM model)
    {
        var user = await _repository.GetUserByEmailAsync(model.Email);

        if(user == null) 
        {
            return (false, null, null);
        }

        var token = _token.GenerateResetToken(model.Email);

        return (true,token,user);
    }

    // VALIDATE RESET LINK
    public (bool status, string? message, ResetPasswordVM? model) ValidateResetLink(string token)
    {
        var email = _token.ValidateResetToken(token);

        if (email == null)
        {
            return (false, "Invalid Or Expire Token", null);
        }

        ResetPasswordVM model = new ResetPasswordVM { Email = email };
        return (true, "Password is Reset", model);
    }

    // RESET PASSWORD
    public async Task<(bool status, string? message)> ResetPassword(ResetPasswordVM model)
    {
        User? user = await _repository.GetUserByEmailAsync(model.Email);

        if (user == null)
        {
            return (false, "User is not valid");
        }

        if(Hashing.VerifyPassword(model.NewPassword!,user.Password))
        {
            return(false, "You can not set same password");
        }
        
        user.Password = Hashing.HashPassword(model.NewPassword!);
        user.IsFirstTime = false;
        bool status =  await _repository.UpdateUserAsync(user);
        if(!status)
        {
            return(false,"Server Error");
        }

        return(true,"Password Changed");
    }
}

