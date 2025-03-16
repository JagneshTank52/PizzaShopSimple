using System.ComponentModel.DataAnnotations;

namespace PizzaShop.Entity.ViewModels.MenuVM;

public class ModifierListVM
{
    public int Id { get; set; }

    [Required]
    public string Name { get; set; } = "";

    public string? Description { get; set; }

    // public int Unit {get; set;}
    // public string UnitName { get; set; } = "";
    
    public decimal Rate { get; set; }

    [Range(1, double.MaxValue, ErrorMessage = "Quantity must be greater than zero")]
    public int Quantity { get; set; }

    public bool isGetById {get; set; } = true;
    
}
