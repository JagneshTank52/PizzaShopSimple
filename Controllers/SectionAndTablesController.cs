using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using PizzaShop.Entity.ViewModels.SectionAndTableVM;
using PizzaShop.Service.Helper;
using PizzaShop.Service.Interface;

namespace PizzaShop.Web.Controllers;

public class SectionAndTablesController : Controller
{
    private readonly ISectionAndTableService _service;
    public SectionAndTablesController(ISectionAndTableService service)
    {
        _service = service;
    }
    public IActionResult SectionAndTable()
    {
        return View();
    }

    //  ====== SECTION =======

    // GET - SECTION LIST
    [HttpGet]
    public async Task<IActionResult> GetSectionList()
    {
        List<SectionVM> sectionList = await _service.SectionList();

        return PartialView("_SectionList", sectionList);
    }

    // GET - SECTION BY ID
    [HttpGet]
    public IActionResult GetSectionById(int id)
    {
        SectionVM section = _service.GetSection(id);
    
        return PartialView("_AddSection",section);
    }

    // POST - ADD SECTION
    [HttpPost]
    public IActionResult AddSection(SectionVM sectionVM)
    {
        if (!ModelState.IsValid)
        {
            return PartialView("_AddSection",sectionVM);
        }

        int createrId = Int32.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);

        var(status, message) = _service.AddSection(sectionVM, createrId);

        if(!status)
        {
            return Json(new { success = false, msg = message });
        }

        return Json(new { success = true, msg = message });
    }

    // POST - EDIT SECTION
    [HttpPost]
    public IActionResult EditSection(SectionVM sectionModel)
    {
        int modifierId = Int32.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);

        if (!ModelState.IsValid)
        {
            return PartialView("_AddSection",sectionModel);
        }

        var(status,message) = _service.EditSection(sectionModel,modifierId);

        if(!status)
        {
            return Json(new { success = false,msg = message });
        }

        return Json(new { success = true,msg = message  });
    }

    // GET - DELETE SECTION
    [HttpGet]
    public async Task<IActionResult> DeleteSection(int id)
    {
        var (status, message) = await _service.DeleteSection(id);

        if (!status)
        {
            return Json(new { success = false, msg = message });
        }
        
        return Json(new { success = true, msg = message });
    }


    // ========= TABLE LIST ==========

    // GET - TABLE LIST
    [HttpGet]
    public async Task<IActionResult> GetTableList(int id,string? searchString, string? sorting, int pageIndex = 1, int pageSize = 5)
    {
        PaginatedList<TableVm> tableList = await _service.TableList(searchString,sorting,pageIndex,pageSize,id);

        return PartialView("_TableList", tableList);
    }
}
