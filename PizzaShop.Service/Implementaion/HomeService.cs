using PizzaShop.Entity.Models;
using PizzaShop.Entity.ViewModels.HomeVM;
using PizzaShop.Entity.ViewModels.UserVM;

using PizzaShop.Repository.Interface;
using PizzaShop.Service.Helper;
using PizzaShop.Service.Interface;

namespace PizzaShop.Service.Implementaion;

public class HomeService : IHomeService
{
    public readonly IUserRepository _repository;

    public HomeService(IUserRepository repository)
    {
        _repository = repository;
    }

    public async Task<(bool status, string? message)> ChangePassword(ChangePasswordVM model, string email)
    {
        User? user = await _repository.GetUserByEmailAsync(email);

        if (user == null)
        {
            return (false, "User is not valid");
        }

        if (!Hashing.VerifyPassword(model.CurrentPassword!, user.Password))
        {
            return (false, "your current passeord does not match.");
        }

        if (Hashing.VerifyPassword(model.NewPassword!, user.Password))
        {
            return (false, "You can not set same password");
        }

        user.Password = Hashing.HashPassword(model.NewPassword!);
        bool status = await _repository.UpdateUserAsync(user);
        if (!status)
        {
            return (false, "Server Error");
        }

        return (true, "Password Changed");
    }

    #region MY PROFILE
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


    public async Task<(bool status, string? message, UserVM? userVM)> GetMyProfile(string email, string role)
    {
        User? user = await _repository.GetUserByEmailAsync(email);

        if (user == null)
        {
            return (false, "user does not found", null);
        }

        UserVM userVM = new UserVM
        {
            FirstName = user!.FirstName,
            Id = user.Id,
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
            UserRoleName = role,
            CountryList = _repository.getCountryList()!,
            StateList = _repository.getStateListByCountryID(user.CountryId)!,
            CityList = _repository.getCityListByStateID(user.StateId)!
        };

        return (true, "User data fetched.", userVM);
    }

    public async Task<(bool status, string message)> PostMyProfile(UserVM  myProfile, string userEmail)
    {
        string profileImagePath ;

        myProfile.CountryList = _repository.getCountryList()!;

        myProfile.StateList = _repository.getStateListByCountryID(myProfile.CountryId)!;

        myProfile.CityList = _repository.getCityListByStateID(myProfile.StateId)!;

        User? user = await _repository.GetUserByEmailAsync(userEmail);

        if (user == null)
        {
            return (false, "User Does not Match");
        }


        if (myProfile.ProfileImageFile != null && myProfile.ProfileImageFile.Length > 0)
        {
            string uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads");

            if (!Directory.Exists(uploadsFolder))
            {
                Directory.CreateDirectory(uploadsFolder);
            }

            string uniqueFileName = Guid.NewGuid().ToString() + Path.GetExtension(myProfile.ProfileImageFile.FileName);
            string filePath = Path.Combine(uploadsFolder, uniqueFileName);

            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await myProfile.ProfileImageFile.CopyToAsync(stream);
            }

            profileImagePath = "/uploads/" + uniqueFileName;
            user.ProfileImage = profileImagePath;
        }

        user.FirstName = myProfile.FirstName!;
        user.LastName = myProfile.LastName;
        user.UserName = myProfile.UserName!;
        user.PhoneNumber = myProfile.PhoneNumber!;
        user.CountryId = myProfile.CountryId;
        user.StateId = myProfile.StateId;
        user.CityId = myProfile.CityId;
        user.Address = myProfile.Address!;
        user.ZipCode = myProfile.ZipCode;
        user.ModifiedAt = DateTime.Now;

        bool isUpdate = await _repository.UpdateUserAsync(user);

        if (!isUpdate)
        {
            return (false, "User is not Updated");
        }

        return (true, "Your Profile Updated Successfully.");

    }
    #endregion
}
