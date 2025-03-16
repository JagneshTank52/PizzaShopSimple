using PizzaShop.Entity.ViewModels.HelperVM;
using PizzaShop.Entity.ViewModels.MenuVM;

using PizzaShop.Service.Helper;

namespace PizzaShop.Service.Interface;

public interface IMenuService
{
    // CATEGORY
    // public List<CategoryVM> CategoryList ();
    public Task<List<CategoryVM>> CategoryList ();
    public Task<(bool status, string? message, CategoryVM category) > GetCategory(int id);
    public Task<(bool status, string message)> AddCategory(CategoryVM categoryModel,int createrId);
    public Task<(bool status, string message)> EditCategory(CategoryVM categoryModel,int createrId);
    public Task<(bool status, string? message) > DeleteCategory(int id);
    public Task<(bool status, string? message) > DeleteItem(int id);
    public (bool status, string? message) DeleteModifierGroup(int id);

    // ITEM
    public Task<PaginatedList<ItemListVM>> GetItemAsync(string? searchString, string? sorting, int pageIndex, int pageSize, int id);
    public Task<ItemVM> GetAddItem(int id);

    public Task<(bool status, string message)> DeleteItems(List<int> itemIds);
    public Task<(bool status, string message)> AddItem(ItemVM itemModel,int createrId);
    public Task<(bool status, string message)> EditItem(ItemVM itemModel,int createrId);

    // MODIFIER GROUP

    public Task<PaginatedList<ModifierListVM>> GetModifierAsync(string? searchString,int pageIndex, int pageSize, int id);
    public List<ModifierGroupVM> ModifierGroupList ();
    public (bool status, string? message, ModifierGroupVM? modifierGroup) GetModifierGroup(int id);
    public Task<(bool status, string message)> AddModifiergroup(ModifierGroupVM modifierGroupModel,int createrId);
    public Task<(bool status, string message)> EditModifierGroup(ModifierGroupVM modifierGroupModel,int createrId);

    // MODIFIER 

    



    

    
 


}
