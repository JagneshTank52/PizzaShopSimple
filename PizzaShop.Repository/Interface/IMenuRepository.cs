using Microsoft.AspNetCore.Mvc.Rendering;
using PizzaShop.Entity.Models;
using PizzaShop.Entity.ViewModels.MenuVM;




namespace PizzaShop.Repository.Interface;

public interface IMenuRepository
{
    //  CRUD OPERATION
    Task<bool> AddCategoryAsync(Category category);
    Task<bool> AddItemAsync(Item item);
    Task<int> AddModiiferGroupAsync(ModifierGroup modifierGroup);
    Task<bool> AddModiiferListAsync(List<Modifier> modifierList);
    Task<bool> UpdateAsync(Category category);
    Task<bool> UpdateModifierGroup(ModifierGroup modifierGroup);
    Task<bool> UpdateItemAsync(Item item);

    // GET LIST
    IQueryable<Category>? getCategoryList();

    IQueryable<ModifierGroup>? GetModifierGroupList();
    IQueryable<Item>? getItemList(int id);
    IQueryable<Modifier>? GetModifierListById(int id);
    IQueryable<Modifier>? GetModifierList(List<SelectedModifierVM> selectedModifier);

    List<SelectListItem> getItemTypeList();
    List<SelectListItem> getMeasuringUnit();


    // GET BY ID
    Task<Item?> GetItemByIdAsync(int id);
    Task<Category?> GetCategoryByIdAsync(int id);
    ModifierGroup? GetModifierGroupById(int id);

    Task<List<Item?>> GetItemsListById(List<int> itemIds);
   
    void updateItem(Item item);
    Task SaveChanges();
    


}
