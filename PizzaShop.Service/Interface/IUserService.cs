using PizzaShop.Entity.ViewModels.UserVM;
using PizzaShop.Service.Helper;

namespace PizzaShop.Service.Interface;

public interface IUserService
{
    public Task<PaginatedList<UserVM>> GetUsersAsync(string? searchString, string? sorting, int pageIndex, int pageSize);

    public Task<(bool status, string? message) > DeleteUser(int id);

    public UserVM Adduser();
    UserVM ?  GetCaseCadeDropDown(string type, int value);

    public Task<(bool status, string? message)> PostAdduser(UserVM userViewModel, int createrId);
    Task<(bool status, string? message, UserVM? userVM)> GetUserProfile(int id);
    Task<(bool status, string? message)> EditUserProfile(UserVM editUser, int modifierId);


}

