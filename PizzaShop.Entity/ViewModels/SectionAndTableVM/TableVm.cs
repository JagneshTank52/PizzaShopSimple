using System.ComponentModel.DataAnnotations;

namespace PizzaShop.Entity.ViewModels.SectionAndTableVM;

public class TableVm
{
    public int Id { get; set; }

    [Required(ErrorMessage = "Please Select Section.")]
    public int SectionId { get; set; }

    [Required(ErrorMessage = "Name Required.")]
    [StringLength(50,MinimumLength = 2,ErrorMessage = "Not greate then 50")]
    public string? TableName { get; set;}

    [Required(ErrorMessage = "Enter Capacity.")]
    [MaxLength(10, ErrorMessage = "Max Limit Reached.")]
    public int Capacity { get; set; }

}
