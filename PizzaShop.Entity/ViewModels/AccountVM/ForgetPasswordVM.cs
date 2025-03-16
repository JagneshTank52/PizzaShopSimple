using System.ComponentModel.DataAnnotations;

namespace PizzaShop.Entity.ViewModels.AccountVM;

public class ForgetPasswordVM
{
    [Required(ErrorMessage ="Please Enter your email address.")]
    [RegularExpression(@"^[a-zA-Z0-9._%+-]+@[a-zA-Z0-9.-]+\.[a-zA-Z]{2,}$", ErrorMessage = "Please enter a valid email address.")]
    public string Email {get; set;}
}


