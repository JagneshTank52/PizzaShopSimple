using PizzaShop.Entity.ViewModels.HomeVM;
using PizzaShop.Entity.ViewModels.UserVM;

namespace PizzaShop.Service.Interface;

public interface IHomeService
{
    Task<(bool status,string? message)> ChangePassword(ChangePasswordVM model, string email);

    Task<(bool status, string? message, UserVM ? userVM)> GetMyProfile(string email, string role);

    UserVM ?  GetCaseCadeDropDown(string type, int value);

    Task<(bool status, string message)> PostMyProfile(UserVM  myProfile, string userEmail);
}
