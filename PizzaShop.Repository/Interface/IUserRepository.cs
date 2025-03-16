using Microsoft.AspNetCore.Mvc.Rendering;
using PizzaShop.Entity.Models;



namespace PizzaShop.Repository.Interface;

public interface IUserRepository
{
    Task<User?> GetUserByEmailAsync(string email);
    Task<User?> GetUserByIdAsync(int id);
    // comman
    Task<bool> UpdateUserAsync(User user);
    Task<bool> AddUserAsync(User user);
    List<SelectListItem>? getCountryList();
    List<SelectListItem>? getStateListByCountryID(int Id);
    List<SelectListItem>? getCityListByStateID(int Id);
    List<SelectListItem>? getRoleListByID();
    // List<SelectListItem>? getRoleListByID(int id);
    IQueryable<User>? getUserList();



    

    

}
