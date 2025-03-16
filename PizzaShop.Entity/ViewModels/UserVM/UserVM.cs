using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Rendering;
namespace PizzaShop.Entity.ViewModels.UserVM;

public class UserVM
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

    [Required(ErrorMessage = "Email is required.")]
    [RegularExpression(@"^[^@\s]+@[^@\s]+\.[^@\s]+$", ErrorMessage = "Please enter a valid email address.")]
    public string? Email { get; set; }

    [Required(ErrorMessage = "Password is required.")]
    [StringLength(12, MinimumLength = 6, ErrorMessage ="Password shoud be in between 6 and 12 digit")]
    public string? Password { get; set; }

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

    public bool? IsFirstTime { get; set; }

    [Required(ErrorMessage = "ZipCode is required.")]
    public string ZipCode { get; set; } = null!;

    [Required(ErrorMessage = "User Role required.")]
    public int UserRoleId { get; set; }

    public string? UserRoleName {get; set;}

    public bool IsDeleated { get; set; }

    public DateTime? CreatedAt { get; set; }

    public int? CreatedBy { get; set; }

    public DateTime? ModifiedAt { get; set; }

    public int? ModifiedBy { get; set; }

    public bool? IsActive { get; set; } = false;

    public List<SelectListItem> CountryList{ get; set; } = new List<SelectListItem>();
    public List<SelectListItem> StateList { get; set; } = new List<SelectListItem>();
    public List<SelectListItem> CityList { get; set; } = new List<SelectListItem>();
    public List<SelectListItem> RoleList { get; set; } = new List<SelectListItem>();
}
