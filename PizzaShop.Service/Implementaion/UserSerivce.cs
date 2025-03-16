using System.Threading.Tasks;
using PizzaShop.Entity.Models;
using PizzaShop.Entity.ViewModels.UserVM;

using PizzaShop.Repository.Interface;
using PizzaShop.Service.Helper;
using PizzaShop.Service.Interface;

namespace PizzaShop.Service.Implementaion;

public class UserSerivce : IUserService
{

    public readonly IUserRepository _repository;

    public UserSerivce(IUserRepository repository)
    {
        _repository = repository;
    }

    #region GET USER LIST
    public async Task<PaginatedList<UserVM>> GetUsersAsync(string? searchString, string? sorting, int pageIndex, int pageSize)
    {
        IQueryable<User>? users = _repository.getUserList();

        if (!string.IsNullOrEmpty(searchString))
        {
            users = users!.Where(s => s.LastName!.ToLower().Contains(searchString.ToLower()) || s.FirstName!.ToLower().Contains(searchString.ToLower()) || s.UserRole.Name!.ToLower().Contains(searchString.ToLower()));
        }

        users = sorting switch
        {
            "name_asc" => users!.OrderBy(o => o.FirstName),
            "name_desc" => users!.OrderByDescending(o => o.FirstName),
            "role_asc" => users!.OrderBy(o => o.UserRole.Name),
            "role_desc" => users!.OrderByDescending(o => o.UserRole.Name),
            _ => users!.OrderBy(o => o.Id)
        };

        var paginatedUser = users.Select(
            s => new UserVM
            {
                Id = s.Id,
                FirstName = s.FirstName,
                LastName = s.LastName,
                Email = s.Email,
                UserRoleName = s.UserRole.Name,
                PhoneNumber = s.PhoneNumber,
                ProfileImage = s.ProfileImage,
                IsActive = s.IsActive
            }
        );

        PaginatedList<UserVM> userList = await PaginatedList<UserVM>.CreateAsync(paginatedUser, pageIndex, pageSize);

        return userList;
    }
    #endregion

    #region ADD USER
    public UserVM Adduser()
    {
        UserVM user = new UserVM
        {
            CountryList = _repository.getCountryList()!,
            RoleList = _repository.getRoleListByID()!
            // RoleList = _repository.getRoleListByID(modifierId)!
        };

        return user;
    }
    public async Task<(bool status, string? message)> PostAdduser(UserVM userViewModel, int createrId)
    {
        User? isExist = await _repository.GetUserByEmailAsync(userViewModel.Email!);

        if(isExist != null ){
            return (false,"Account already exist with email.");
        }
        
        string profileImagePath = userViewModel.ProfileImage!;

        if (userViewModel.ProfileImageFile != null && userViewModel.ProfileImageFile.Length > 0)
        {
            string uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads");

            if (!Directory.Exists(uploadsFolder))
            {
                Directory.CreateDirectory(uploadsFolder);
            }

            string uniqueFileName = Guid.NewGuid().ToString() + Path.GetExtension(userViewModel.ProfileImageFile.FileName);
            string filePath = Path.Combine(uploadsFolder, uniqueFileName);

            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await userViewModel.ProfileImageFile.CopyToAsync(stream);
            }

            profileImagePath = "/uploads/" + uniqueFileName;
        }
        

        User newUser = new User
        {
            // Id = 3,
            FirstName = userViewModel.FirstName!,
            LastName = userViewModel.LastName,
            UserName = userViewModel.UserName!,
            Email = userViewModel.Email!,
            Password = Hashing.HashPassword(userViewModel.Password!),
            ProfileImage = profileImagePath,

            PhoneNumber = userViewModel.PhoneNumber!,
            CountryId = userViewModel.CountryId,
            StateId = userViewModel.StateId,
            CityId = userViewModel.CityId,
            Address = userViewModel.Address!,
            IsFirstTime = true,
            ZipCode = userViewModel.ZipCode,
            UserRoleId = userViewModel.UserRoleId,
            IsDeleated = false,
            CreatedAt = DateTime.Now,
            CreatedBy = createrId,
            ModifiedAt = null,
            ModifiedBy = null,
            IsActive = true
        };

        if (!await _repository.AddUserAsync(newUser))
        {
            return (false, "User Not Added");
        }
        return (true, "User added succesfully.");

    }
    #endregion

    #region EDIT USER

    public UserVM? GetCaseCadeDropDown(string type, int value)
    {
        UserVM userVM = new UserVM();

        switch (type)
        {
            case "country":
                userVM.StateList = _repository.getStateListByCountryID(value)!;
                break;
            case "state":
                userVM.CityList = _repository.getCityListByStateID(value)!;
                break;
        }

        return userVM;
    }

    public async Task<(bool status, string? message, UserVM? userVM)> GetUserProfile(int id)
    {
        User? user = await _repository.GetUserByIdAsync(id);

        if (user == null)
        {
            return (false, "User Does not found", null);
        }

        UserVM userModel = new UserVM
        {
            Id = id,
            FirstName = user!.FirstName,
            LastName = user!.LastName!,
            UserName = user!.UserName,
            Email = user.Email,
            ProfileImage = user.ProfileImage,
            PhoneNumber = user!.PhoneNumber,
            CityId = user!.CityId,
            StateId = user!.StateId,
            CountryId = user!.CountryId,
            Address = user!.Address,
            ZipCode = user!.ZipCode,
            UserRoleId = user.UserRoleId,
            CountryList = _repository.getCountryList()!,
            StateList = _repository.getStateListByCountryID(user.CountryId)!,
            CityList = _repository.getCityListByStateID(user.StateId)!,
            RoleList = _repository.getRoleListByID()!,
            IsActive = user.IsActive,
        };

        return (true, "User Found", userModel);
    }

    public async Task<(bool status, string? message)> EditUserProfile(UserVM editUser, int modifierId)
    {
        string profileImagePath ;

        editUser.CountryList = _repository.getCountryList()!;
        editUser.StateList = _repository.getStateListByCountryID(editUser.CountryId)!;
        editUser.CityList = _repository.getCityListByStateID(editUser.StateId)!;
        editUser.RoleList = _repository.getRoleListByID()!;

        User? user = await _repository.GetUserByIdAsync(editUser.Id);

        if (user == null)
        {
            return (false, "User Does not found");
        }

        if (editUser.ProfileImageFile != null && editUser.ProfileImageFile.Length > 0)
        {
            string uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads");

            if (!Directory.Exists(uploadsFolder))
            {
                Directory.CreateDirectory(uploadsFolder);
            }

            string uniqueFileName = Guid.NewGuid().ToString() + Path.GetExtension(editUser.ProfileImageFile.FileName);
            string filePath = Path.Combine(uploadsFolder, uniqueFileName);

            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await editUser.ProfileImageFile.CopyToAsync(stream);
            }

            profileImagePath = "/uploads/" + uniqueFileName;
            user.ProfileImage = profileImagePath;
        }

        user!.FirstName = editUser.FirstName!;
        user.LastName = editUser.LastName;
        user.UserName = editUser.UserName!;
        user.PhoneNumber = editUser.PhoneNumber!;
        user.CountryId = editUser.CountryId;
        user.StateId = editUser.StateId;
        user.CityId = editUser.CityId;
        user.Address = editUser.Address!;
        user.UserRoleId = editUser.UserRoleId;
        user.IsActive = editUser.IsActive;
        user.ZipCode = editUser.ZipCode;
        user.ModifiedAt = DateTime.Now;
        user.ModifiedBy = modifierId;

        if (!await _repository.UpdateUserAsync(user))
        {
            return (false, "user Does Not Update");
        }

        return (true, "User Edited Successfully");
    }

    #endregion

    #region DELETE USER
    public async Task<(bool status, string? message)> DeleteUser(int id)
    {
        var user = await _repository.GetUserByIdAsync(id);
        user!.IsDeleated = true;
        if (!await _repository.UpdateUserAsync(user))
        {
            return (false, "User not Deleted");
        }

        return (true, "User Deleted Successfully");

    }
    #endregion

}
