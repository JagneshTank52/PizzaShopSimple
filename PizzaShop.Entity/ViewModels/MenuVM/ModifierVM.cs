using System.ComponentModel.DataAnnotations;

namespace PizzaShop.Entity.ViewModels.MenuVM;

public class ModifierVM
{
    public int Id { get; set; }

    [Required(ErrorMessage = "Modifier Name is required")]
    [StringLength(50,ErrorMessage = "Max limit for Modifier")]
    public string Name { get; set; } = "";

    public string? Description { get; set; }

    // public int Unit {get; set;}
    // public string UnitName { get; set; } = "";
    [Required(ErrorMessage ="Rate is required")]
    public decimal Rate { get; set; }

    [Required(ErrorMessage = "Quntity is required")]
    [Range(1, 100, ErrorMessage = "Quantity must be greater than zero")]
    public int Quantity { get; set; }

    [Required(ErrorMessage = "Modifier group is Required")]
    public int ModifierGroupId { get; set; }


    public bool isGetById {get; set; } = true;
}
