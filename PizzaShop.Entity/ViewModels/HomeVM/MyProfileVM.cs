using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace PizzaShop.Entity.ViewModels.HomeVM;

public class MyProfileVM
{
    public int Id { get; set; }

    [Required(ErrorMessage = "First Name Reqired")]
    [StringLength(50)]
    public  string? FirstName { get; set; }

    [StringLength(50)]
    public string? LastName { get; set; }

    [Required(ErrorMessage = "User Name Reqired")]
    [StringLength(50)]
    public string? UserName { get; set; }
    
    public string? Email { get; set; }

    public string? ProfileImage { get; set; }

   public IFormFile? ProfileImageFile {get; set;}

    [Required(ErrorMessage = "PhoneNumber is required.")]
    public string? PhoneNumber { get; set; } 

    [Required(ErrorMessage = "Please Select Country.")]
    public int CountryId { get; set; }

    [Required(ErrorMessage = "Please Select State.")]
    public int StateId { get; set; }

    [Required(ErrorMessage = "Please Select City.")]
    public int CityId { get; set; }

    [Required(ErrorMessage = "Address is required.")]
    public string? Address { get; set; }

    [Required(ErrorMessage = "ZipCode is required.")]
    public string ZipCode { get; set; } = null!;

    public string? UserRoleName {get; set;}

    public List<SelectListItem> CountryList{ get; set; } = new List<SelectListItem>();
    public List<SelectListItem> StateList { get; set; } = new List<SelectListItem>();
    public List<SelectListItem> CityList { get; set; } = new List<SelectListItem>();
}
