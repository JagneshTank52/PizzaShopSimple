using System.ComponentModel.DataAnnotations;

namespace PizzaShop.Entity.ViewModels.MenuVM;

public class ItemModifierVM
{
    public int Id { get; set;}
    public int ModifierGroupId { get; set;}
    public string? ModifierGroupName { get; set;}

    public List<SelectedModifierVM> Modifier = new List<SelectedModifierVM>();

    [Required(ErrorMessage = "Minimum Modifier is required.")]
    [Range(0, 10, ErrorMessage = "Min Modifier must be between 0 and 32,767.")]
    public int MinModifier { get; set; }

    [Required(ErrorMessage = "Maximum Modifier is required.")]
    [Range(0, 10, ErrorMessage = "Max Modifier must be between 0 and 32,767.")]
    public int MaxModifier { get; set; }

    // Custom Validation to ensure MinModifier <= MaxModifier
    public bool IsValid() => MinModifier <= MaxModifier;
}
