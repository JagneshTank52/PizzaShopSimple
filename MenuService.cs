using System.Linq.Expressions;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using PizzaShop.Entity.Models;
using PizzaShop.Entity.ViewModels.HelperVM;
using PizzaShop.Entity.ViewModels.MenuVM;
using PizzaShop.Entity.ViewModels.OrderAppVM;
using PizzaShop.Repository.Interface;
using PizzaShop.Service.Helper;
using PizzaShop.Service.Interface;

namespace PizzaShop.Service.Implementaion;

public class MenuService : IMenuService
{
    private readonly IUnitOfWork _unitOfWork;

    public MenuService(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    #region  CATEGORY

    // CATEGIORY LIST
    public List<CategoryVM> CategoryList()
    {
        var categoryList = _unitOfWork.categoryRepository.GetAll(s => !s.IsDeleated, s => s.Id);

        var categoryVMList = categoryList!
            .Select(s => new CategoryVM
            {
                Id = s.Id,
                CategoryName = s.Name,
                Description = s.Description
            }); 

        return categoryVMList.ToList();
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

        var category = await _unitOfWork.categoryRepository.GetByIdAsync(id);
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

        bool isUnique = _unitOfWork.categoryRepository.Add(newCategory, c => c.Name.ToLower() == categoryModel.CategoryName.ToLower());

        if (!isUnique)
        {
            return (isUnique, "Category with same name already exist");
        }

        bool isAdded = await _unitOfWork.SaveAsync();

        if (!isAdded)
        {
            return (isAdded, "Category not added");
        }

        return (isAdded, "Category Added");
    }

    // EDIT CATEGORY
    public async Task<(bool status, string message)> EditCategory(CategoryVM categoryModel, int createrId)
    {
        Category? category = await _unitOfWork.categoryRepository.GetByIdAsync(categoryModel.Id);

        category.Description = categoryModel.Description;
        category.ModifiedAt = DateTime.Now;
        category.ModifiedBy = createrId;

        bool isUnique;
        if (categoryModel.CategoryName.ToLower() == category.Name.ToLower())
        {
            category!.Name = categoryModel.CategoryName;
            isUnique = _unitOfWork.categoryRepository.Update(category);
        }
        else
        {
            category!.Name = categoryModel.CategoryName;
            isUnique = _unitOfWork.categoryRepository.Update(category, c => c.Name.ToLower() == categoryModel.CategoryName.ToLower());
        }
        if (!isUnique)
        {
            return (isUnique, "Category with same name already exist");
        }

        bool isEdit = await _unitOfWork.SaveAsync();

        if (!isEdit)
        {
            return (false, "Server Error");
        }

        return (true, "Category Updated");
    }

    // DELETE CATEGORY BY ID
    public async Task<(bool status, string? message)> DeleteCategory(int id)
    {
        Category? category = await _unitOfWork.categoryRepository.GetByIdAsync(id);

        if (category == null)
        {
            return (false, "Category does not exsit");
        }

        category.IsDeleated = true;
    
        await _unitOfWork.categoryRepository.DeleteAsync(id);

        var itemList = _unitOfWork.itemRepository.GetItemList(id)!.ToList();

        foreach (var item in itemList)
        {
            item.IsDeleated = true;
            await _unitOfWork.itemRepository.DeleteAsync(id);
        }

        bool isDeleted = await _unitOfWork.SaveAsync();
        if (!isDeleted)
        {
            return (false, "Category not Deleted");
        }

        return (true, "Category Deleted Successfully");
    }
    #endregion

    #region ITEM

    // ITEM LIST
    public NewPaginatedList<ItemListVM> GetItem(PageInfo pageInfo)
    {
        // 1 DEFAULT FLTER
        Expression<Func<Item, bool>> filter = f => !f.IsDeleated && f.CategoryId == pageInfo.GruoupId;

        // 2 SEARCH FLTER
        if (!string.IsNullOrEmpty(pageInfo.SearchString))
        {
            filter = filter.AndAlso(f => f.Name.ToLower().Contains(pageInfo.SearchString.ToLower()));
        }

        // 3 ORDER BY
        Func<IQueryable<Item>, IOrderedQueryable<Item>> orderBy = q => q.OrderBy(o => o.Id);

        var pageResult = _unitOfWork.itemRepository.GetPagedRecords(pageInfo.PageSize, pageInfo.PageIndex, orderBy, filter);

        var ViewModels = pageResult.records.Select(
            item => new ItemListVM
            {
                ItemId = item.Id,
                ItemName = item.Name,
                ItemTypeId = item.ItemType,
                ItemQuantity = item.Quantity,
                ItemRate = item.UnitPrice,
                ProfileImage = item.ItemImage,
                IsAvaiable = item.IsAvaiable.GetValueOrDefault(),
            }
        ).ToList();

        pageInfo.PageIndex = pageResult.pageIndex;
        NewPaginatedList<ItemListVM> itemList = new NewPaginatedList<ItemListVM>(ViewModels, pageResult.totalRecord, pageInfo);
        return itemList;
    }

    // ITEM BY ID
    public async Task<ItemVM> GetAddItem(int id)
    {
        ItemVM itemModel = new ItemVM();

        (itemModel.UnitIdList, itemModel.ItemTypeList, itemModel.CategoryList, itemModel.ModifierGroupList) = LoadDefaultForItem();

        if (id == 0)
        {
            return itemModel;
        }

        Item? item = await _unitOfWork.itemRepository.GetByIdAsync(id);
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

        itemModel.ItemModifierGroupList = _unitOfWork.itemRepository.GetItemModifierGroupList(id);

        return itemModel;
    }

    // ADD ITEM
    public async Task<(bool status, string message)> AddItem(ItemVM itemModel, int createrId)
    {
        string profileImagePath;

        if (itemModel.ProfileImageFile != null && itemModel.ProfileImageFile.Length > 0)
        {
            string uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads", "item");

            if (!Directory.Exists(uploadsFolder))
            {
                Directory.CreateDirectory(uploadsFolder);
            }

            string uniqueFileName = Guid.NewGuid().ToString() + Path.GetExtension(itemModel.ProfileImageFile.FileName);
            string filePath = Path.Combine(uploadsFolder, uniqueFileName);

            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await itemModel.ProfileImageFile.CopyToAsync(stream);
            }

            profileImagePath = "/uploads/item/" + uniqueFileName;
            itemModel.ProfileImage = profileImagePath;
        }

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

        int itemId = _unitOfWork.itemRepository.AddItem(newItem);

        if (itemId == 0)
        {
            return (false, "Item with same name or short code already exist");
        }

        foreach (var modifierGroup in itemModel.ItemModifierGroupList)
        {
            ItemsModifier itemsModifierGroup = new ItemsModifier
            {
                ItemId = itemId,
                ModifierGorupId = modifierGroup.modifierGroupId,
                IsDeleated = false,
                MaxModifier = modifierGroup.maxModifier,
                MinModifier = modifierGroup.minModifier,
                CreatedAt = DateTime.Now,
                CreatedBy = createrId
            };

            bool isUnique = _unitOfWork.itemModifierGroup.Add(itemsModifierGroup);

            if (!isUnique)
            {
                return (isUnique, "Modifier Group with same name exist");
            }

        }

        bool isAdded = await _unitOfWork.SaveAsync();
        if (!isAdded)
        {
            return (false, "Item Not Added");
        }
        return (true, "Item Added Succesfully");
    }

    // EDIT ITEM
    public async Task<(bool status, string message)> EditItem(ItemVM itemModel, int createrId)
    {
        Item? item = await _unitOfWork.itemRepository.GetByIdAsync(itemModel.Id);

        if (item == null)
        {
            return (false, "Item Does not found");
        }

        if (itemModel.ProfileImageFile != null && itemModel.ProfileImageFile.Length > 0)
        {
            string uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads", "item");

            if (!Directory.Exists(uploadsFolder))
            {
                Directory.CreateDirectory(uploadsFolder);
            }

            string uniqueFileName = Guid.NewGuid().ToString() + Path.GetExtension(itemModel.ProfileImageFile.FileName);
            string filePath = Path.Combine(uploadsFolder, uniqueFileName);
            Console.WriteLine(filePath);

            if (File.Exists(filePath))
            {
                // If the file exists, delete it
                File.Delete(filePath);
            }


            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await itemModel.ProfileImageFile.CopyToAsync(stream);
            }

            string profileImagePath;
            profileImagePath = "/uploads/item/" + uniqueFileName;
            item.ItemImage = profileImagePath;
        }

        item.ItemType = itemModel.ItemTypeId;
        item.UnitPrice = itemModel.ItemRate;
        item.Quantity = itemModel.ItemQuantity;
        item.Unit = itemModel.UnitId;
        item.DefaultTax = itemModel.IsDefaultTax;
        item.Description = itemModel.ItemDescription;
        item.TextPercentage = itemModel.TaxPercentage;
        item.IsFavorite = itemModel.IsFavorite;
        item.ShortCode = itemModel.ShortCode!;
        item.CategoryId = itemModel.CategoryId;
        item.IsAvaiable = itemModel.IsAvaiable;
        item.ModifiedAt = DateTime.Now;
        item.ModifiedBy = createrId;

        bool isUnique;
        if (itemModel.ItemName.ToLower() == item.Name.ToLower())
        {
            item!.Name = itemModel.ItemName!;
            isUnique = _unitOfWork.itemRepository.Update(item);
        }
        else
        {
            item!.Name = itemModel.ItemName!;
            isUnique = _unitOfWork.itemRepository.Update(item, f => f.Name.ToLower() == itemModel.ItemName!.ToLower());
        }
        if (!isUnique)
        {
            return (false, "Item with same name exist");
        }

        List<int> existingModifiergroup = _unitOfWork.itemModifierGroup.GetModifierGroupPerItem(item.Id);

        // For non Deleting 
        List<ItemModifierGroupVM> nonDeletedModifierGroup = itemModel.ItemModifierGroupList.Where(w => existingModifiergroup.Any(a => a == w.Id)).ToList();
        if (nonDeletedModifierGroup.Any())
        {
            foreach (var itemModifierGroup in nonDeletedModifierGroup)
            {
                ItemsModifier? modifiergroup = await _unitOfWork.itemModifierGroup.GetByIdAsync(itemModifierGroup.Id);
                modifiergroup.MaxModifier = itemModifierGroup.maxModifier;
                modifiergroup.MinModifier = itemModifierGroup.minModifier;
                bool isSame = _unitOfWork.itemModifierGroup.Update(modifiergroup);
            }
        }

        // For Deleting Modifier
        List<int> deleteModifierGroup = existingModifiergroup.Where(w => !itemModel.ItemModifierGroupList.Any(a => a.Id == w)).ToList();
        if (deleteModifierGroup.Any())
        {
            foreach (var itemModifierGroup in deleteModifierGroup)
            {
                ItemsModifier? modifiergroup = await _unitOfWork.itemModifierGroup.GetByIdAsync(itemModifierGroup);
                modifiergroup!.IsDeleated = true;
                await _unitOfWork.itemModifierGroup.DeleteAsync(modifiergroup.Id);
            }
        }

        // For new Added modifier group 
        List<ItemModifierGroupVM> newAddedModifierGroup = itemModel.ItemModifierGroupList.Where(w => w.Id == 0).ToList();
        if (newAddedModifierGroup.Any())
        {
            foreach (var itemModifierGroup in newAddedModifierGroup)
            {
                ItemsModifier newModifierGroup = new ItemsModifier
                {
                    ItemId = item.Id,
                    ModifierGorupId = itemModifierGroup.modifierGroupId,
                    IsDeleated = false,
                    MaxModifier = itemModifierGroup.maxModifier,
                    MinModifier = itemModifierGroup.minModifier,
                    CreatedAt = DateTime.Now,
                    CreatedBy = createrId
                };
                bool isSame = _unitOfWork.itemModifierGroup.Add(newModifierGroup);
            }
        }

        bool isEdit = await _unitOfWork.SaveAsync();

        if (!isEdit)
        {
            return (false, "Server Error");
        }

        return (true, "Item Updated");
    }

    // DELETE ITEM
    public async Task<(bool status, string? message)> DeleteItem(int id)
    {
        Item? item = await _unitOfWork.itemRepository.GetByIdAsync(id);

        item!.IsDeleated = true;
        await _unitOfWork.itemRepository.DeleteAsync(id);

        bool isDeleted = await _unitOfWork.SaveAsync();

        if (!isDeleted)
        {
            return (false, "Item not Deleted");
        }

        return (true, "Item Deleted Successfully");
    }

    // DELETE ITEM MASS
    public async Task<(bool status, string message)> DeleteItems(List<int> itemIds)
    {
        var items = await _unitOfWork.itemRepository.GetItemsListById(itemIds);

        if (items == null)
        {
            return (false, "No item is selected");
        }

        foreach (var item in items)
        {
            item!.IsDeleated = true;
            await _unitOfWork.itemRepository.DeleteAsync(item.Id);
        }

        bool isDeleted = await _unitOfWork.SaveAsync();

        if (!isDeleted)
        {
            return (false, "Item not Deleted");
        }

        return (true, "Item Deleted");
    }

    // LOAD DEFAULT ITEM
    public (List<SelectListItem> measuringUnit, List<SelectListItem> itemType, List<SelectListItem> categoryList, List<SelectListItem> modifierGroupList) LoadDefaultForItem()
    {
        var measuringUnit = _unitOfWork.modifierRepository.GetMeasuringUnit();
        var itemType = _unitOfWork.itemRepository.GetItemTypeList();
        var categoryList = _unitOfWork.categoryRepository.GetAll(s => !s.IsDeleated, s => s.Id)!.Select(s => new SelectListItem
        {
            Value = s.Id.ToString(),
            Text = s.Name,
        }).ToList();
        var modifierGroupList = _unitOfWork.modifierGroupRepository.GetModifierGroupList()!.Select(s => new SelectListItem
        {
            Value = s.Id.ToString(),
            Text = s.Name,
        }).ToList();

        return (measuringUnit, itemType, categoryList, modifierGroupList);
    }

    // MENU ITEM FOR ORDER APP MENU
    public List<MenuItemVm> GetMenuItem(int categoryId)
    {
        IEnumerable<Item>? itemList;

        if(categoryId == 0){
            itemList= _unitOfWork.itemRepository.GetAll(f => !f.IsDeleated, o => o.Id, q => q.Include(i => i.ItemTypeNavigation));
        }
        else if(categoryId == 100000){
            itemList = _unitOfWork.itemRepository.GetAll(f => !f.IsDeleated && f.IsFavorite, o => o.Id,q => q.Include(i => i.ItemTypeNavigation));
        }
        else {
            itemList = _unitOfWork.itemRepository.GetAll(f => !f.IsDeleated && f.CategoryId == categoryId, o => o.Id,q => q.Include(i => i.ItemTypeNavigation));
        }
        
        List<MenuItemVm> menuItems = itemList.Select(
            s => new MenuItemVm{
                ItemId = s.Id,
                ItemName = s.Name,
                ItemImage = s.ItemImage ?? "/images/dining-menu.png",
                IsFavorite = s.IsFavorite,
                FoodType = s.ItemType,
                FoodTypeName = s.ItemTypeNavigation.Name,
                ItemPrice = s.UnitPrice

            }).ToList();

        return menuItems;
    }

    // TOGGLE FAVORITE
    public async Task<(bool status, string message)> ToggleFavorite(int itemId)
    {
        var item = await _unitOfWork.itemRepository.GetByIdAsync(itemId);
        if(item == null){
            return (false, "Item not found");
        }
        item.IsFavorite = !item.IsFavorite;
        bool isUpdated = _unitOfWork.itemRepository.Update(item);
        if(!isUpdated){
            return (false, "Failed to toggle favorite");
        }
        return (true, "Favorite toggled");
    }
    #endregion

    #region MODIFIER GROUP

    // MODIFIER GROUP LIST
    public List<ModifierGroupVM> ModifierGroupList()
    {
        var modifierGroups = _unitOfWork.modifierGroupRepository.GetModifierGroupList();

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

    // GET MODIFIER GROUP BY ID
    public async Task<ModifierGroupVM> GetModifierGroup(int id)
    {
        var modifierGroupVm = new ModifierGroupVM();
        if (id == 0)
        {
            modifierGroupVm.Id = 0;
            return modifierGroupVm;
        }

        var modifierGroup = await _unitOfWork.modifierGroupRepository.GetByIdAsync(id);
        modifierGroupVm.Id = modifierGroup!.Id;
        modifierGroupVm.ModifierGroupName = modifierGroup.Name;
        modifierGroupVm.Description = modifierGroup!.Description;

        var modifierList = _unitOfWork.modifierRepository.GetModifierListById(id)!;
        List<SelectedModifierVM> selectedModifiers = modifierList.Select(
            s => new SelectedModifierVM
            {
                Id = s.Id,
                Name = s.Name
            }
            ).ToList();

        modifierGroupVm.SelectedModifiers = selectedModifiers;

        return modifierGroupVm;
    }

    //  ADD MODIFIER GROUP
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

        bool isUnique = _unitOfWork.modifierGroupRepository.Add(newModifierGroup, f => f.Name.ToLower() == modifierGroupModel.ModifierGroupName!.ToLower());

        if (!isUnique)
        {
            return (false, "ModifierGruoup with same name exist");
        }

        int modifierGroupId = _unitOfWork.modifierGroupRepository.AddModifierGroup(newModifierGroup);

        if (modifierGroupId == 0)
        {
            return (false, "Modifier group not added");
        }

        List<Modifier>? oldModifierList = await _unitOfWork.modifierGroupRepository.GetModifierList(modifierGroupModel.SelectedModifiers!)!.ToListAsync();

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

        _unitOfWork.modifierRepository.AddModiiferListAsync(newModifierList);

        var isAdded = await _unitOfWork.SaveAsync();

        if (!isAdded)
        {
            return (false, "Server Error");
        }

        return (true, "Modifier group Added");
    }

    // EDIT MODIFIER GROUP
    public async Task<(bool status, string message)> EditModifierGroup(ModifierGroupVM modifierGroupModel, int createrId)
    {

        // For adding new modifier group
        ModifierGroup? modifierGroup = await _unitOfWork.modifierGroupRepository.GetByIdAsync(modifierGroupModel.Id);

        modifierGroup.Description = modifierGroupModel.Description;
        modifierGroup.ModifiedAt = DateTime.Now;
        modifierGroup.ModifiedBy = createrId;

        bool isUnique;

        if (modifierGroupModel.ModifierGroupName.ToLower() == modifierGroup!.Name.ToLower())
        {
            modifierGroup!.Name = modifierGroupModel.ModifierGroupName!;
            isUnique = _unitOfWork.modifierGroupRepository.Update(modifierGroup);
        }
        else
        {
            modifierGroup!.Name = modifierGroupModel.ModifierGroupName!;
            isUnique = _unitOfWork.modifierGroupRepository.Update(modifierGroup, f => f.Name == modifierGroupModel.ModifierGroupName);
        }

        if (!isUnique)
        {
            return (false, "ModifierGruoup with same name exist");
        }

        if (modifierGroupModel.SelectedModifiers != null)
        {

            IQueryable<Modifier>? existingModifier = _unitOfWork.modifierRepository.GetModifierListById(modifierGroup.Id);

            // For Deleteing deleted modifier
            List<Modifier> deleteModifier = existingModifier!.Where(w => !modifierGroupModel.SelectedModifiers!.Select(m => m.Id).Contains(w.Id)).ToList();
            if (deleteModifier.Any())
            {
                foreach (var modifier in deleteModifier)
                {
                    modifier.IsDeleated = true;
                    await _unitOfWork.modifierRepository.DeleteAsync(modifier.Id);
                }
            }

            // For Adding new Modifier
            List<Modifier> selectedModifierFromDb = _unitOfWork.modifierGroupRepository.GetModifierList(modifierGroupModel.SelectedModifiers!)!.ToList();
            List<Modifier>? willAddModifier = selectedModifierFromDb.Where(w => !existingModifier!.Any(a => a.Id == w.Id)).ToList();
            if (willAddModifier!.Any())
            {
                List<Modifier> newModifierList = new List<Modifier>();

                foreach (var modifier in willAddModifier!)
                {

                    Modifier newModifier = new Modifier
                    {
                        Name = modifier.Name!,
                        Description = modifier.Description,
                        Quantity = modifier.Quantity,
                        UnitPrice = modifier.UnitPrice,
                        ModifierGroupId = modifierGroup.Id,
                        IsDeleated = false,
                        CreatedAt = DateTime.Now,
                        CreatedBy = createrId,
                        ModifiedAt = DateTime.Now,
                        ModifiedBy = createrId
                    };
                    newModifierList.Add(newModifier);
                }

                _unitOfWork.modifierRepository.AddModiiferListAsync(newModifierList);
            }
        }
        var isAdded = await _unitOfWork.SaveAsync();

        if (!isAdded)
        {
            return (false, "Server Error");
        }
        return (true, "Modifier group Updated");
    }

    // DELETE MODIFIER GROUP
    public async Task<(bool status, string? message)> DeleteModifierGroup(int id)
    {
        var modifierGroup = await _unitOfWork.modifierGroupRepository.GetByIdAsync(id);

        if (modifierGroup == null)
        {
            return (false, "Modifier group does not exist");
        }

        modifierGroup.IsDeleated = true;
        await _unitOfWork.modifierGroupRepository.DeleteAsync(id);

        var modifierList = _unitOfWork.modifierRepository.GetModifierListById(id)!.ToList();

        foreach (var modifier in modifierList)
        {
            modifier.IsDeleated = true;
            await _unitOfWork.modifierRepository.DeleteAsync(modifier.Id);
        }

        bool isDeleted = await _unitOfWork.SaveAsync();

        if (!isDeleted)
        {
            return (false, "ModifierGroup not Deleted");
        }

        return (true, "Modifiergroup Deleted Successfully");
    }

    #endregion

    #region MODIFIER

    // MODIFIER LIST
    public async Task<PaginatedList<ModifierListVM>> GetModifierAsync(string? searchString, int pageIndex, int pageSize, int id)
    {
        IQueryable<Modifier>? modifiers = _unitOfWork.modifierRepository.GetModifierListById(id);

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


        if (paginatedModifier.Count() != 0 && paginatedModifier.Count() % pageSize == 0 && pageIndex > paginatedModifier.Count() / pageSize)
        {
            pageIndex--;
        }

        PaginatedList<ModifierListVM> modifierList = await PaginatedList<ModifierListVM>.CreateAsync(paginatedModifier, pageIndex, pageSize);

        return modifierList;
    }

    // GET MODIFIER BY ID
    public async Task<ModifierVM> GetModifier(int id)
    {
        ModifierVM modifierVM = new ModifierVM();

        (modifierVM.ModifierGroupList, modifierVM.UnitList) = LoadDefaultForModifier();

        if (id == 0)
        {
            modifierVM.Id = 0;
            return modifierVM;
        }

        Modifier? modifier = await _unitOfWork.modifierRepository.GetByIdAsync(id);

        modifierVM.Id = modifier!.Id;
        modifierVM.Name = modifier!.Name;
        modifierVM.ModifierGroupId = modifier.ModifierGroupId;
        modifierVM.Rate = modifier.UnitPrice;
        modifierVM.Quantity = modifier.Quantity;
        modifierVM.UnitId = modifier.MeasuringUnitId;
        modifierVM.IsFavorite = false;
        modifierVM.Description = modifier.Description;

        return modifierVM;
    }

    // ADD MODIFIER
    public async Task<(bool status, string message)> AddModifier(ModifierVM modifierVM, int createrId)
    {
        Modifier newModifier = new Modifier
        {
            Name = modifierVM.Name!,
            Description = modifierVM.Description,
            Quantity = modifierVM.Quantity,
            UnitPrice = modifierVM.Rate,
            ModifierGroupId = modifierVM.ModifierGroupId,
            MeasuringUnitId = modifierVM.UnitId,
            IsDeleated = false,
            CreatedAt = DateTime.Now,
            CreatedBy = createrId,
            ModifiedAt = DateTime.Now,
            ModifiedBy = createrId,
        };

        // bool isUnique = _unitOfWork.modifierRepository.Add(newModifier, f => f.Name == modifierVM.Name && f.ModifierGroupId == modifierVM.ModifierGroupId);
        bool isUnique = _unitOfWork.modifierRepository.Add(newModifier);

        if (!isUnique)
        {
            return (false, "Modifer with same name exist in same modifier group.");
        }

        bool isAdded = await _unitOfWork.SaveAsync();

        if (!isAdded)
        {
            return (isAdded, "Modifier not created");
        }
        return (isAdded, "Modifier created successfully");
    }

    // EDIT MODIFIER
    public async Task<(bool status, string message)> EditModifier(ModifierVM modifierVM, int modifierId)
    {
        Modifier? modifier = await _unitOfWork.modifierRepository.GetByIdAsync(modifierVM.Id);


        modifier!.Description = modifierVM.Description;
        modifier.Quantity = modifierVM.Quantity;
        modifier.UnitPrice = modifierVM.Rate;
        modifier.ModifierGroupId = modifierVM.ModifierGroupId;
        modifier.MeasuringUnitId = modifierVM.UnitId;
        modifier.ModifiedAt = DateTime.Now;
        modifier.ModifiedBy = modifierId;

        bool isUnique;

        // if (modifierVM.Name.ToLower() == modifier.Name.ToLower())
        // {
            modifier!.Name = modifierVM.Name!;
            isUnique = _unitOfWork.modifierRepository.Update(modifier);
        // }
        // else
        // {
        //     modifier!.Name = modifierVM.Name!;
        //     isUnique = _unitOfWork.modifierRepository.Update(modifier, f => f.Name.ToLower() == modifierVM.Name!.ToLower() && f.ModifierGroupId == modifierVM.ModifierGroupId);
        // }

        if (!isUnique)
        {
            return (false, "Modifer with same name exist in same modifier group.");
        }

        bool isUpdated = await _unitOfWork.SaveAsync();
        if (!isUpdated)
        {
            return (false, "Modifier not updated");
        }
        return (true, "Modifier updated successfully");
    }

    // DELETE MODIFIERS
    public async Task<(bool status, string message)> DeleteModifiers(List<int> selectedModifier)
    {
        var modifiers = await _unitOfWork.modifierRepository.GetModifiers(selectedModifier);

        if (modifiers == null)
        {
            return (false, "No modifier is selected");
        }

        foreach (var modifier in modifiers)
        {
            modifier!.IsDeleated = true;
            await _unitOfWork.modifierRepository.DeleteAsync(modifier.Id);
        }

        bool isDeleted = await _unitOfWork.SaveAsync();
        if (!isDeleted)
        {
            return (false, "Modifier not deleted");
        }

        return (true, "Modifier deleted successfully");
    }

    // LOAD DEFAUT LIST FOR MODIFIER
    public (List<SelectListItem> modifierGroupList, List<SelectListItem> unitList) LoadDefaultForModifier()
    {
        var modifierGroupList = _unitOfWork.modifierGroupRepository.GetModifierGroupList()!.Select(
            s => new SelectListItem
            {
                Value = s.Id.ToString(),
                Text = s.Name
            }).ToList();

        var unitList = _unitOfWork.modifierRepository.GetMeasuringUnit();

        return (modifierGroupList, unitList);
    }
    #endregion
}



