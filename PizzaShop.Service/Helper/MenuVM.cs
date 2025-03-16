using PizzaShop.Entity.ViewModels.MenuVM;

namespace PizzaShop.Service.Helper;

public class MenuVM
{
    public CategoryVM? category {get; set;}
    public List<CategoryVM>? categoies {get; set;}
   
    public PaginatedList<ItemListVM>? items{get; set;}
}
