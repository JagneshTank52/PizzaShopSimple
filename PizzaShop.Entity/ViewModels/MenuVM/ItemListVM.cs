using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;

namespace PizzaShop.Entity.ViewModels.MenuVM;

public class ItemListViewModel
{
    public List<ItemListVM> Items { get; set; } = new List<ItemListVM>();

    public List<int> SelectedItemIds { get; set; } = new List<int>();
}

public class ItemListVM
{
    public int ItemId { get; set; }

    [Required]
    public string ItemName { get; set; } = "";
    public string? ProfileImage { get; set; }

  public IFormFile? ProfileImageFile {get; set;}

    public int ItemTypeId { get; set; }
    
    public decimal ItemRate { get; set; }
    [Range(1, double.MaxValue, ErrorMessage = "Quantity must be greater than zero")]


    public int ItemQuantity { get; set; }
    public bool IsAvaiable{get; set;}

}


