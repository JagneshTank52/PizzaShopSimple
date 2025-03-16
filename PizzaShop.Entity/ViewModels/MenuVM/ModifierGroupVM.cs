using System.ComponentModel.DataAnnotations;

namespace PizzaShop.Entity.ViewModels.MenuVM;

public class ModifierGroupVM
{
    public int Id { get; set; }

    [Required(ErrorMessage = "Category Name Required.")]
    public string? ModifierGroupName { get; set; }

    public List<SelectedModifierVM>? SelectedModifiers { get; set; } = new List<SelectedModifierVM>();

    public string? Description { get; set; }
}
