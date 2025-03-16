using System.ComponentModel.DataAnnotations;

namespace PizzaShop.Entity.ViewModels.HomeVM;

public class ChangePasswordVM
{
    [Required(ErrorMessage = "New Password is Required")]
    [StringLength(12, MinimumLength =6, ErrorMessage ="Password should be in between 6 to 12")]
    [DataType(DataType.Password)]

    public string? NewPassword {get; set;}

    [Required(ErrorMessage = "Confirm Password is Required")]
    [DataType(DataType.Password)]
    [Compare("NewPassword", ErrorMessage = "Password doesn't match.")]
    // [CompareAttribute("NewPassword", ErrorMessage = "Password doesn't match.")] 
    public string? ConfirmPassword {get; set;}

    [Required(ErrorMessage = "Current Password is Required")]
    [StringLength(12, MinimumLength =6, ErrorMessage ="Password should be in between 6 to 12")]
    [DataType(DataType.Password)]

    public string? CurrentPassword {get; set;}
}
