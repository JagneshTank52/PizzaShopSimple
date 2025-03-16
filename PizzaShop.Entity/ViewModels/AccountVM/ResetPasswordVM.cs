using System.ComponentModel.DataAnnotations;

namespace PizzaShop.Entity.ViewModels.AccountVM;

public class ResetPasswordVM
{
    
    [RegularExpression(@"^[^@\s]+@[^@\s]+\.[^@\s]+$", ErrorMessage = "Please enter a valid email address.")]
    public string Email {get; set;}

    [Required(ErrorMessage = "New Password is Required")]
    [StringLength(12, MinimumLength =6, ErrorMessage ="Password should be in between 6 to 12")]
    [DataType(DataType.Password)]

    public string? NewPassword {get; set;}

    [Required(ErrorMessage = "Confirm Password is Required")]
    [DataType(DataType.Password)]
    [Compare("NewPassword", ErrorMessage = "Password doesn't match.")]
    // [CompareAttribute("NewPassword", ErrorMessage = "Password doesn't match.")] 
    public string? ConfirmPassword {get; set;}
}
