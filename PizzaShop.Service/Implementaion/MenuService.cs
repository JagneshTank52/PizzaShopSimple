using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using PizzaShop.Entity.Models;
using PizzaShop.Entity.ViewModels.MenuVM;
using PizzaShop.Repository.Interface;
using PizzaShop.Service.Helper;
using PizzaShop.Service.Interface;

namespace PizzaShop.Service.Implementaion;

public class MenuService : IMenuService
{
    private readonly IMenuRepository _repository;

    public MenuService(IMenuRepository repository)
    {
        _repository = repository;
    }

    // ========= CATEGORY ========

    // CATEGIORY LIST
    public async Task<List<CategoryVM>> CategoryList()
    {
        var watch = System.Diagnostics.Stopwatch.StartNew();

        // Asynchronously fetch categories from the repository
        var categoryList = _repository.getCategoryList();

        // Convert to a list of CategoryVM (view model) using LINQ projection
        var categoryVMList = await categoryList!
            .Select(s => new CategoryVM
            {
                Id = s.Id,
                CategoryName = s.Name,
                Description = s.Description
            })
            .ToListAsync();  // Materialize the query (fetch data)

        watch.Stop();
        var time = watch.ElapsedMilliseconds;
        Console.WriteLine("Time to fetch data: " + time);

        return categoryVMList;
    }

    // CATEGORY BY ID
    public async Task<(bool status, string? message, CategoryVM category)> GetCategory(int id)
    {
        CategoryVM categoryVm = new CategoryVM();
        if (id == 0)
        {
            categoryVm.Id = 0;
            return (true, "Category Add", categoryVm);
        }

        var category = await _repository.GetCategoryByIdAsync(id);
        categoryVm.Id = category!.Id;
        categoryVm.CategoryName = category!.Name;
        categoryVm.Description = category!.Description;

        return (true, "Category get", categoryVm);
    }

    // ADD CATEGORY
    public async Task<(bool status, string message)> AddCategory(CategoryVM categoryModel, int createrId)
    {
        Category newCategory = new Category
        {
            Name = categoryModel.CategoryName,
            Description = categoryModel.Description,
            IsDeleated = false,
            CreatedAt = DateTime.Now,
            CreatedBy = createrId,
            ModifiedAt = DateTime.Now,
            ModifiedBy = createrId
        };

        var isAdded = await _repository.AddCategoryAsync(newCategory);

        if (!isAdded)
        {
            return (isAdded, "Category not added");
        }

        return (isAdded, "Category Added");
    }

    // EDIT CATEGORY
    public async Task<(bool status, string message)> EditCategory(CategoryVM categoryModel, int createrId)
    {
        Category? category = await _repository.GetCategoryByIdAsync(categoryModel.Id);

        category!.Name = categoryModel.CategoryName;
        category.Description = categoryModel.Description;
        category.ModifiedAt = DateTime.Now;
        category.ModifiedBy = createrId;

        var isEdit = await _repository.UpdateAsync(category);

        if (!isEdit)
        {
            return (false, "Server Error");
        }

        return (true, "Category Updated");
    }

    // DELETE CATEGORY
    public async Task<(bool status, string? message)> DeleteCategory(int id)
    {
        Category? category = await _repository.GetCategoryByIdAsync(id);

        if (category == null)
        {
            return (false, "Category does not exsit");
        }

        category.IsDeleated = true;

        var watch = System.Diagnostics.Stopwatch.StartNew();

        var itemList = _repository.getItemList(id)!.ToList();

        foreach (var item in itemList)
        {
            item.IsDeleated = true;
        }

        if (!await _repository.UpdateAsync(category))
        {
            return (false, "Category not Deleted");
        }

        watch.Stop();
        var time = watch.ElapsedMilliseconds;
        Console.WriteLine("Time to delete data: " + time);

        return (true, "Category Deleted Successfully");
    }

    // ========= ITEM ========

    // ITEM LIST
    public async Task<PaginatedList<ItemListVM>> GetItemAsync(string? searchString, string? sorting, int pageIndex, int pageSize, int id)
    {
        IQueryable<Item>? Items = _repository.getItemList(id);

        // if (!string.IsNullOrEmpty(searchString))
        // {
        //     users = users!.Where(s => s.Email!.Contains(searchString) ||
        //                     s.FirstName!.Contains(searchString) ||
        //                     s.LastName!.Contains(searchString) ||
        //                     s.UserRole.Name!.Contains(searchString));
        // }

        // users = sorting switch
        // {
        //     "name_asc" => users!.OrderBy(o => o.FirstName),
        //     "name_desc" => users!.OrderByDescending(o => o.FirstName),
        //     "role_asc" => users!.OrderBy(o => o.UserRole.Name),
        //     "role_desc" => users!.OrderByDescending(o => o.UserRole.Name),
        //     _ => users!.OrderBy(o => o.Id)
        // };

        var paginatedItem = Items!.Select(
            item => new ItemListVM
            {
                ItemId = item.Id,
                ItemName = item.Name,
                ItemTypeId = item.ItemType,
                ItemQuantity = item.Quantity,
                ItemRate = item.UnitPrice,
                IsAvaiable = item.IsAvaiable.GetValueOrDefault(),
            }
        );

        PaginatedList<ItemListVM> userList = await PaginatedList<ItemListVM>.CreateAsync(paginatedItem, pageIndex, pageSize);

        return userList;
    }

    // ITEM BY ID
    public async Task<ItemVM> GetAddItem(int id)
    {
        List<SelectListItem> categoryList = _repository.getCategoryList()!.Select(
            s => new SelectListItem
            {
                Value = s.Id.ToString(),  // Assuming 'Id' is an integer, convert it to string
                Text = s.Name
            }).ToList();

        // List<SelectListItem> modifierGroupList = _repository.GetModifierGroupList()!.Select(
        //     s => new SelectListItem
        //     {
        //         Value = s.Id.ToString(),
        //         Text = s.Name
        //     }).ToList();

        List<SelectListItem> modifierGroupList = _repository.GetModifierGroupList()!.Select(
            s => new SelectListItem
            {
                Value = s.Id.ToString(),
                Text = s.Name
            }).ToList();

        List<SelectListItem> itemType = _repository.getItemTypeList();
        List<SelectListItem> measuringUnit = _repository.getMeasuringUnit();

        ItemVM itemModel = new ItemVM
        {
            CategoryList = categoryList,
            ItemTypeList = itemType,
            UnitIdList = measuringUnit,
            ModifierGroupList = modifierGroupList
        };

        if (id == 0)
        {
            return itemModel;
        }

        Item? item = await _repository.GetItemByIdAsync(id);
        itemModel.Id = item!.Id;
        itemModel.CategoryId = item!.CategoryId;
        itemModel.ItemName = item.Name;
        itemModel.ItemTypeId = item.ItemType;
        itemModel.ItemRate = item.UnitPrice;
        itemModel.ItemQuantity = item.Quantity;
        itemModel.UnitId = item.Unit;
        itemModel.TaxPercentage = item.TextPercentage;
        itemModel.IsDefaultTax = item.DefaultTax.GetValueOrDefault();
        itemModel.IsAvaiable = item.IsAvaiable.GetValueOrDefault();
        itemModel.ShortCode = item.ShortCode;
        itemModel.ItemDescription = item.Description;
        itemModel.ProfileImage = item.ItemImage;

        return itemModel;
    }

    // ADD ITEM
    public async Task<(bool status, string message)> AddItem(ItemVM itemModel, int createrId)
    {
        // string profileImagePath = itemModel.ProfileImage!;

        // if (userViewModel.ProfileImageFile != null && userViewModel.ProfileImageFile.Length > 0)
        // {
        //     string uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads");

        //     if (!Directory.Exists(uploadsFolder))
        //     {
        //         Directory.CreateDirectory(uploadsFolder);
        //     }

        //     string uniqueFileName = Guid.NewGuid().ToString() + Path.GetExtension(userViewModel.ProfileImageFile.FileName);
        //     string filePath = Path.Combine(uploadsFolder, uniqueFileName);

        //     using (var stream = new FileStream(filePath, FileMode.Create))
        //     {
        //          itemModel.ProfileImageFile.CopyToAsync(stream);
        //     }

        //     profileImagePath = "/uploads/" + uniqueFileName;
        // }

        Item newItem = new Item
        {
            Name = itemModel.ItemName!,
            ItemType = itemModel.ItemTypeId,
            UnitPrice = itemModel.ItemRate,
            Quantity = itemModel.ItemQuantity,
            Unit = itemModel.UnitId,
            DefaultTax = itemModel.IsDefaultTax,
            Description = itemModel.ItemDescription,
            TextPercentage = itemModel.TaxPercentage,
            IsFavorite = itemModel.IsFavorite,
            ItemImage = itemModel.ProfileImage,
            ShortCode = itemModel.ShortCode!,
            CategoryId = itemModel.CategoryId,
            IsAvaiable = itemModel.IsAvaiable,
            IsDeleated = false,
            CreatedAt = DateTime.Now,
            CreatedBy = createrId,
            ModifiedAt = DateTime.Now,
            ModifiedBy = createrId
        };

        if (!await _repository.AddItemAsync(newItem))
        {
            return (false, "Item Not Added");
        }
        return (true, "Item Added Succesfully");
    }

    // EDIT CATEGORY
    public async Task<(bool status, string message)> EditItem(ItemVM itemModel, int createrId)
    {
        Item? item = await _repository.GetItemByIdAsync(itemModel.Id);

        item!.Name = itemModel.ItemName!;
        item.ItemType = itemModel.ItemTypeId;
        item.UnitPrice = itemModel.ItemRate;
        item.Quantity = itemModel.ItemQuantity;
        item.Unit = itemModel.UnitId;
        item.DefaultTax = itemModel.IsDefaultTax;
        item.Description = itemModel.ItemDescription;
        item.TextPercentage = itemModel.TaxPercentage;
        item.IsFavorite = itemModel.IsFavorite;
        item.ItemImage = itemModel.ProfileImage;
        item.ShortCode = itemModel.ShortCode!;
        item.CategoryId = itemModel.CategoryId;
        item.IsAvaiable = itemModel.IsAvaiable;
        item.ModifiedAt = DateTime.Now;
        item.ModifiedBy = createrId;

        var isEdit = await _repository.UpdateItemAsync(item);

        if (!isEdit)
        {
            return (false, "Server Error");
        }

        return (true, "Category Updated");
    }
    // DELETE ITEM

    public async Task<(bool status, string? message)> DeleteItem(int id)
    {
        var item = await _repository.GetItemByIdAsync(id);
        item!.IsDeleated = true;
        if (!await _repository.UpdateItemAsync(item))
        {
            return (false, "Category not Deleted");
        }

        return (true, "Category Deleted Successfully");
    }

    // for mass delete Items
    public async Task<(bool status, string message)> DeleteItems(List<int> itemIds)
    {
        var items = await _repository.GetItemsListById(itemIds);

        if (items == null)
        {
            return (false, "No item is selected");
        }

        foreach (var item in items)
        {
            item!.IsDeleated = true;
            _repository.updateItem(item);
        }

        await _repository.SaveChanges();

        return (true, "Item Deleted");
    }


    // ========= MODIFIER GROUP ========

    // MODIFIER GROUP LIST
    public List<ModifierGroupVM> ModifierGroupList()
    {
        var modifierGroups = _repository.GetModifierGroupList();

        var modifierGroupList = modifierGroups!.Select(
             s => new ModifierGroupVM
             {
                 Id = s.Id,
                 ModifierGroupName = s.Name,
                 Description = s.Description
             }
         ).ToList();

        return modifierGroupList;
    }

    // Get MidifierGroup By id
    public (bool status, string? message, ModifierGroupVM modifierGroup) GetModifierGroup(int id)
    {
        var modifierGroupVm = new ModifierGroupVM();
        if (id == 0)
        {
            modifierGroupVm.Id = 0;
            return (status: true, "New modifierGroup", modifierGroupVm);
        }

        var modifierGroup = _repository.GetModifierGroupById(id);
        modifierGroupVm.Id = modifierGroup!.Id;
        modifierGroupVm.ModifierGroupName = modifierGroup!.Name;
        modifierGroupVm.Description = modifierGroup!.Description;

        var modifierList = _repository.GetModifierListById(id)!;
        List<SelectedModifierVM> selectedModifiers = modifierList.Select(
            s => new SelectedModifierVM{
                Id = s.Id,
                Name = s.Name
            }
            ).ToList();
            
        modifierGroupVm.SelectedModifiers = selectedModifiers;

        return (true, "Category get", modifierGroupVm);
    }

    //  POST - ADD MODIFIER GROUP
    public async Task<(bool status, string message)> AddModifiergroup(ModifierGroupVM modifierGroupModel, int createrId)
    {
        ModifierGroup newModifierGroup = new ModifierGroup
        {
            Name = modifierGroupModel.ModifierGroupName!,
            Description = modifierGroupModel.Description,
            IsDeleated = false,
            CreatedAt = DateTime.Now,
            CreatedBy = createrId,
            ModifiedAt = DateTime.Now,
            ModifiedBy = createrId
        };

        int modifierGroupId = await _repository.AddModiiferGroupAsync(newModifierGroup);

        if (modifierGroupId == 0)
        {
            return (false, "Modifier group not added");
        }

        List<Modifier>? oldModifierList = await _repository.GetModifierList(modifierGroupModel.SelectedModifiers!)!.ToListAsync();

        if (oldModifierList == null)
        {
            return (false, "Selected modifier does not exist");
        }

        List<Modifier> newModifierList = new List<Modifier>();

        foreach (var modifier in oldModifierList)
        {
            Modifier newModifier = new Modifier
            {
                Name = modifier.Name,
                Description = modifier.Description,
                Quantity = modifier.Quantity,
                UnitPrice = modifier.UnitPrice,
                ModifierGroupId = modifierGroupId,
                IsDeleated = false,
                CreatedAt = DateTime.Now,
                CreatedBy = createrId,
                ModifiedAt = DateTime.Now,
                ModifiedBy = createrId
            };
            newModifierList.Add(newModifier);
        }

        var isAdded = await _repository.AddModiiferListAsync(newModifierList);

        if(!isAdded){
            return(false, "Server Error");
        }

        return (true, "Modifier group Added");
    }

    //  POST - Edit MODIFIER GROUP
    public async Task<(bool status, string message)> EditModifierGroup(ModifierGroupVM modifierGroupModel, int createrId)
    {
        var watch = System.Diagnostics.Stopwatch.StartNew();

        ModifierGroup? modifierGroup = _repository.GetModifierGroupById(modifierGroupModel.Id);

        modifierGroup!.Name = modifierGroupModel.ModifierGroupName;
        modifierGroup.Description = modifierGroupModel.Description;
        modifierGroup.ModifiedAt = DateTime.Now;
        modifierGroup.ModifiedBy = createrId;

        var isEdit = await _repository.UpdateModifierGroup(modifierGroup);

        watch.Stop();
        var time = watch.ElapsedMilliseconds;
        Console.WriteLine("Time to eDIT data: " + time);

        if (!isEdit)
        {
            return (false, "Server Error");
        }

        return (true, "Category Updated");
    }

    // DELETE MODIFIER GROUP
    public (bool status, string? message) DeleteModifierGroup(int id)
    {
        var modifierGroup = _repository.GetModifierGroupById(id);

        if (modifierGroup == null)
        {
            return (false, "Modifier group does not exist");
        }

        modifierGroup.IsDeleated = true;

        var modifierList = _repository.getItemList(id)!.ToList();

        // foreach (var item in itemList)
        // {
        //     item.IsDeleated = true;
        // }

        // if (!await _repository.UpdateAsync(category))
        // {
        //     return (false, "Category not Deleted");
        // }

        return (true, "Category Deleted Successfully");
    }

    // ========= MODIFIER ========

    public async Task<PaginatedList<ModifierListVM>> GetModifierAsync(string? searchString, int pageIndex, int pageSize, int id)
    {
        IQueryable<Modifier>? modifiers = _repository.GetModifierListById(id);

        if (!string.IsNullOrEmpty(searchString))
        {
            modifiers = modifiers!.Where(s => s.Name.ToLower().Contains(searchString.ToLower()));
        }

        var paginatedModifier = modifiers!.Select(
            s => new ModifierListVM
            {
                Id = s.Id,
                Name = s.Name,
                Rate = s.UnitPrice,
                Quantity = s.Quantity,
                isGetById = id == 0 ? false : true
            }
        );

        PaginatedList<ModifierListVM> modifierList = await PaginatedList<ModifierListVM>.CreateAsync(paginatedModifier, pageIndex, pageSize);

        return modifierList;
    }

    

    // public async Task<(bool status, string message)> AddModifierList(List<SelectedModifierVM> selectedModifiers, int createrId, int modifierGroupId)
    // {
    //     List<Modifier>? oldModifierList = await _repository.GetModifierList(selectedModifiers)!.ToListAsync();

    //     if(oldModifierList == null){
    //         return(false,"Selected modifier does not exist");
    //     }

    //     List<Modifier> newModifierList = new List<Modifier>();

    //     foreach(var modifier in oldModifierList){
    //         Modifier newModifier = new Modifier{
    //             Id = modifier.Id,
    //             Name = modifier.Name,
    //             Description = modifier.Description,
    //             Quantity = modifier.Quantity,
    //             UnitPrice = modifier.UnitPrice,
    //             ModifierGroupId = modifierGroupId,
    //             IsDeleated = false,
    //             CreatedAt = DateTime.Now,
    //             CreatedBy = createrId,
    //             ModifiedAt = DateTime.Now,
    //             ModifiedBy = createrId
    //         };
    //         newModifierList.Add(newModifier);
    //     }

    //     var isAdded = await _repository.AddModiiferListAsync(newModifierList);

    //     if (!isAdded)
    //     {
    //         return (isAdded, "Modifier group not added");
    //     }

    //     return (isAdded, "Modifier group Added");
    // }


}



