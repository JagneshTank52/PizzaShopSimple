using System.ComponentModel.DataAnnotations;

namespace PizzaShop.Entity.ViewModels.SectionAndTableVM;

public class SectionVM
{
    public int Id { get; set; }

    [Required(ErrorMessage = "Section Name Required.")]
    [StringLength(maximumLength: 50, ErrorMessage ="Max Limit")]
    public string SectionName { get; set; }

    public string? Description { get; set; }
}
