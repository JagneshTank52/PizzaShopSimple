using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using PizzaShop.Entity.Data;
using PizzaShop.Entity.Models;
using PizzaShop.Entity.ViewModels.MenuVM;


// using PizzaShop.Entiy.Models;

using PizzaShop.Repository.Interface;

namespace PizzaShop.Repository.Implementaion;

public class MenuRepository : IMenuRepository
{
    private readonly PizzaShopContext _context;

    public MenuRepository(PizzaShopContext context)
    {
        _context = context;
    }

    // CRUID OPERATION
    public async Task<bool> AddCategoryAsync(Category category)
    {
        try
        {
            _context.Add(category);
            await _context.SaveChangesAsync();
            return true;
        }
        catch
        {
            return false;
        }
    }
    public async Task<bool> AddItemAsync(Item item)
    {
        try
        {
            _context.Add(item);
            await _context.SaveChangesAsync();
            return true;
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            return false;
        }
    }

    public async Task<int> AddModiiferGroupAsync(ModifierGroup modifierGroup)
    {
        try
        {
            _context.Add(modifierGroup);
            await _context.SaveChangesAsync();
            return modifierGroup.Id;
        }
        catch
        {
            return 0;
        }
    }

    public async Task<bool> AddModiiferListAsync(List<Modifier> modifierList)
    {
        try
        {
            foreach(var modifier in modifierList){
                _context.Add(modifier);
            }   
            await _context.SaveChangesAsync();
            return true;
        }
        catch
        {
            return false;
        }
    }

    public async Task<bool> UpdateAsync(Category category)
    {
        try
        {
            _context.Update(category);
            await _context.SaveChangesAsync();
            return true;
        }
        catch
        {
            return false;
        }
    }
    public async Task<bool> UpdateItemAsync(Item item)
    {
        try
        {
            await _context.SaveChangesAsync();
            return true;
        }
        catch
        {
            return false;
        }
    }
    public async Task<bool> UpdateModifierGroup(ModifierGroup modifierGroup)
    {
        try
        {
            _context.Update(modifierGroup);
            await _context.SaveChangesAsync();
            return true;
        }
        catch
        {
            return false;
        }
    }


    // GET BY ID
    public async Task<Category?> GetCategoryByIdAsync(int id)
    {
        return await _context.Categories.AsNoTracking().FirstOrDefaultAsync(c => c.Id == id);
    }

    public async Task<Item?> GetItemByIdAsync(int id)
    {
        return await _context.Items.FirstOrDefaultAsync(c => c.Id == id);
    }


    public ModifierGroup? GetModifierGroupById(int id)
    {
        return _context.ModifierGroups.FirstOrDefault(c => c.Id == id);
    }


    // GET LIST
    public IQueryable<Category>? getCategoryList()
    {
        var categoryList = _context.Categories.Where(x => !x.IsDeleated).OrderBy(o => o.Id).AsQueryable();

        return categoryList;
    }

    public IQueryable<ModifierGroup>? GetModifierGroupList()
    {
        var modfierGroupList = _context.ModifierGroups.Where(x => !x.IsDeleated).OrderBy(o => o.Id).AsQueryable();
        return modfierGroupList;
    }

    public IQueryable<Item>? getItemList(int id)
    {
        var itemList = _context.Items.Where(x => !x.IsDeleated && x.CategoryId == id).OrderBy(o => o.Id).AsQueryable();
        return itemList;
    }

    public List<SelectListItem> getItemTypeList()
    {
        return _context.FoodTypes.Select(s => new SelectListItem
        {
            Value = s.Id.ToString(),
            Text = s.Name
        }).ToList();
    }

    public List<SelectListItem> getMeasuringUnit()
    {
        return _context.MeasuringUnits.Select(s => new SelectListItem
        {
            Value = s.Id.ToString(),
            Text = s.Name
        }).ToList();
    }

    public async Task<List<Item?>> GetItemsListById(List<int> itemIds)
    {
        var items = await _context.Items
        .Where(item => itemIds.Contains(item.Id))
        .ToListAsync();

        return items!;
    }

    public void updateItem(Item item)
    {
        try
        {
            _context.Items.Update(item);
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
        }
    }

    public async Task SaveChanges()
    {
        await _context.SaveChangesAsync();
    }

    public IQueryable<Modifier>? GetModifierListById(int id)
    {
        IQueryable<Modifier> modifierList;

        if (id == 0)
        {
            modifierList = _context.Modifiers.Where(x => !x.IsDeleated).OrderBy(o => o.Id).AsQueryable();
            return modifierList;
        }

        modifierList = _context.Modifiers.Where(x => !x.IsDeleated && x.ModifierGroupId == id).OrderBy(o => o.Id).AsQueryable();
        return modifierList;
    }

    public IQueryable<Modifier>? GetModifierList(List<SelectedModifierVM> selectedModifier)
    {
        IQueryable<Modifier> modifierList;

        modifierList = _context.Modifiers.Where(w => selectedModifier.Select(s => s.Id).Contains(w.Id));

        return modifierList;
    }
}
