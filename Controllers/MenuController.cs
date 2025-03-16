using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using PizzaShop.Entity.ViewModels.MenuVM;

using PizzaShop.Service.Interface;

namespace PizzaShop.Web.Controllers;

public class MenuController : Controller
{
    private readonly IMenuService _service;

    public MenuController(IMenuService service)
    {
        _service = service; 
    }

    // MENU PAGE
    public IActionResult MenuItem()
    {
        return View();
    }

    //  ====== CATEGORY =======

    // GET - CATEGORY LIST
    [HttpGet]
    public async Task<IActionResult> GetCategoryList()
    {
        var categoryList = await _service.CategoryList();

        return PartialView("_CategoryList", categoryList);
    }

    // GET - CATEGORY BY ID
    [HttpGet]
    public async Task<IActionResult> GetCategoryByIdAsync(int id)
    {
        var (status, message,category) = await _service.GetCategory(id);
        if(id == 0) 
        {
            return PartialView("_AddCategory",category);
        }
        return PartialView("_AddCategory",category);
    }

    // POST - ADD CATEGORY
    [HttpPost]
    public async Task<IActionResult> AddCategory(CategoryVM category)
    {
        if(!ModelState.IsValid)
        {
             return PartialView("_AddCategory",category);
        }
        var createrId = Int32.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);

        var (status, message) = await _service.AddCategory(category, createrId);

        if(!status)
        {
            TempData["error"] = message;
            return Json(new { success = true });
        }

        TempData["success"] = message;
        return Json(new { success = true });
    }

    // POST - EDIT CATEGORY
    [HttpPost]
    public async Task<IActionResult> EditCategory(CategoryVM categoryModel)
    {
         var modifierId = Int32.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);

        if (!ModelState.IsValid)
        {
            return PartialView("_AddCategory",categoryModel);
        }

        var(status,message) = await _service.EditCategory(categoryModel,modifierId);

        if(!status)
        {
            TempData["error"] = message;
            return Json(new { success = true });
        }

        TempData["success"] = message;
        return Json(new { success = true });
    }

    // POST - DELETE CATEGORY

    [HttpGet]
    public async Task<IActionResult> DeleteCategory(int id){
        
        var (status, message) = await _service.DeleteCategory(id);
        
        if(!status)
        {
            TempData["error"] =message;
            return RedirectToAction("MenuItem","Menu");
        }

        TempData["success"] = message;
        return RedirectToAction("MenuItem","Menu");
    }

    //  ====== ITEM =======

    // GET - ITEM LIST
    [HttpGet]
    public async Task<IActionResult> GetItemList(int id,string? searchString, string? sorting, int pageIndex = 1, int pageSize = 5)
    {
       var itemList = await  _service.GetItemAsync(searchString,sorting,pageIndex,pageSize,id);
         return PartialView("_ItemListPartialView", itemList); 
    }

    // GET - ITEM BY ID
    [HttpGet]
    public async Task<IActionResult> AddItemAsync(int id)
    {
        var model = await _service.GetAddItem(id);
        return PartialView("_AddItem",model); 
    }

    // POST - ADD ITEM
    [HttpPost]
    public async Task<IActionResult> AddItemAsync(ItemVM item)
    {
        if (!ModelState.IsValid)
        {
            return PartialView("_AddItem",item);
        }

        var createrId = Int32.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);

        var(status, message) = await _service.AddItem(item, createrId);

        if(!status)
        {
            TempData["error"] = message;
            return Json(new { success = true });
        }

        TempData["success"] = message;
        return Json(new { success = true });
    }

    // POST - EDIT ITEM
    [HttpPost]
    public async Task<IActionResult> EditItem(CategoryVM categoryModel)
    {
         var modifierId = Int32.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);

        if (!ModelState.IsValid)
        {
            return PartialView("_AddCategory",categoryModel);
        }

        var(status,message) = await _service.EditCategory(categoryModel,modifierId);

        if(!status)
        {
            TempData["error"] = message;
            return Json(new { success = true });
        }

        TempData["success"] = message;
        return Json(new { success = true });
    }
    
    // DELETE ITEM
    [HttpGet]
    public async Task<IActionResult> DeleteItem(int id){
        
        var (status, message) = await _service.DeleteItem(id);
        
        if(!status)
        {
            TempData["error"] =message;
            return RedirectToAction("MenuItem","Menu");
        }

        TempData["success"] = message;
        return RedirectToAction("MenuItem","Menu");
    }
    [HttpPost]
    public async Task<IActionResult> DeleteItems([FromBody] List<int> idList)
    {
        var (status, message) = await _service.DeleteItems(idList);

        if(!status)
        {
            TempData["error"] = message;
            return Json(new {success = false});
        }

        TempData["success"] = message;
        return Json(new { success = true });
    }


    //  ====== Modifier Group =======

    // GET -    MODIFIER GROUP LIST
    [HttpGet]
    public IActionResult GetModifierGroupList()
    {
        var modifierGroupList = _service.ModifierGroupList();

        return PartialView("_ModifierGroupList", modifierGroupList);
    }

    // GET - MODIFIER LIST 
    [HttpGet]
    public async Task<IActionResult> GetModifierList(int id,string? searchString,int pageIndex = 1, int pageSize = 5)
    {
       var modifierList = await  _service.GetModifierAsync(searchString,pageIndex,pageSize,id);
       if(modifierList == null){
        return Json(new {success = false,message= "No data found"});
       }
        return PartialView("_ModifierListInModal", modifierList); 
    }

    // GET -    MODIFIER GROUP BY ID
    [HttpGet]
    public IActionResult GetModifierGroupById(int id)
    {
        var (status, message, modifierGroup) = _service.GetModifierGroup(id);
        if (id == 0)
        {
            return PartialView("_AddModifierGroup", modifierGroup);
        }
        return PartialView("_AddModifierGroup", modifierGroup);
    }

    // POST - ADD MODIFIER GROUP
    [HttpPost]
    public async Task<IActionResult> AddModifierGroup([FromBody] ModifierGroupVM modifierGroup)
    {
        if(!ModelState.IsValid)
        {
             return PartialView("_AddModifierGroup",modifierGroup);
        }

        int createrId = Int32.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);
        var (status, message) = await _service.AddModifiergroup(modifierGroup, createrId);

        if(!status)
        {
            TempData["error"] = message;
            return Json(new { success = true });
        }

        TempData["success"] = message;
        return Json(new { success = true });
    }

    // POST - UPDATE MODIFIER GROUP
    [HttpPost]
    public async Task<IActionResult> EditModifierGroup(ModifierGroupVM modifierGroup)
    {
        int modifierId = Int32.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);

        if (!ModelState.IsValid)
        {
            return PartialView("_AddCategory", modifierGroup);
        }

        var (status, message) = await _service.EditModifierGroup(modifierGroup, modifierId);

        if (!status)
        {
            TempData["error"] = message;
            return Json(new { success = true });
        }

        TempData["success"] = message;
        return Json(new { success = true });
    }

    // GET - DELETE CATEGORY

    [HttpGet]
    public IActionResult DeleteModifierGroup(int id)
    {


        var (status, message) = _service.DeleteModifierGroup(id);

        if (!status)
        {
            TempData["error"] = message;
            return RedirectToAction("MenuItem", "Menu");
        }

        TempData["success"] = message;
        return RedirectToAction("MenuItem", "Menu");
    }

    [HttpPost]
    public IActionResult SelectedList([FromBody] List<SelectedModifierVM> selectedModifier)
    {
        return PartialView("_SelectedModifier", selectedModifier);
    }
}
