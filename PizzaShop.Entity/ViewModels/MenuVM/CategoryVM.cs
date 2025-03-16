using System.ComponentModel.DataAnnotations;

namespace PizzaShop.Entity.ViewModels.MenuVM;

public class CategoryVM
{
    public int Id { get; set; }
    
    [Required(ErrorMessage = "Category Name Required.")]
    public string CategoryName { get; set; }

    public string? Description { get; set; }
        
}
