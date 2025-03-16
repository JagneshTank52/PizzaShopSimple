using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace PizzaShop.Entity.ViewModels.MenuVM;

public class ItemVM
{
    public int Id { get; set; }

    [Required(ErrorMessage = "Please Select Country.")]
    public int CategoryId { get; set; }

    public List<SelectListItem> CategoryList{ get; set; } = new List<SelectListItem>();
    public List<SelectListItem> ModifierGroupList{ get; set; } = new List<SelectListItem>();

    [Required(ErrorMessage = "Name Required.")]
    [StringLength(50,MinimumLength = 2,ErrorMessage = "Not greate then 50")]
    public string? ItemName { get; set;}

    [Required(ErrorMessage = "Please Select Item Type.")]
    public int ItemTypeId { get; set; }

    public List<SelectListItem> ItemTypeList{ get; set; } = new List<SelectListItem>();

    [Required (ErrorMessage = "Please enter item rate.")]
    [Range(1,50000,ErrorMessage = "Rate must be grater than zero")]
    public decimal ItemRate { get; set; }

    [Required (ErrorMessage = "Please enter quantity.")]
    [Range(1, 200, ErrorMessage = "Quantity must be greater than zero.")]
    public int ItemQuantity { get; set; }

    [Required(ErrorMessage = "Please Select Unit.")]
    public int UnitId { get; set; }

    public List<SelectListItem> UnitIdList{ get; set; } = new List<SelectListItem>();

    public bool IsAvaiable { get; set;}

    public bool IsDefaultTax { get; set;}
    public bool IsFavorite { get; set;}

    
    [Range(1,100,ErrorMessage = "Percentage must be less then 100")]
    public decimal? TaxPercentage { get; set; }

    [MinLength(1,ErrorMessage ="Always greater then 1")]
    [MaxLength(5,ErrorMessage = "Always less then 5")]
    
    public string? ShortCode { get; set;}

    public string? ItemDescription { get; set; }

    public string? ProfileImage { get; set; }

    public IFormFile? ProfileImageFile {get; set;} 

    public List<ItemModifierVM> ItemModifierGroupList = new List<ItemModifierVM>();

}
