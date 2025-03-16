using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using PizzaShop.Entity.ViewModels.RolePermissionVM;
using PizzaShop.Service.Interface;

namespace PizzaShop.Web.Controllers;

public class RoleAndPermission : Controller
{
    readonly IRolePermissionService _service;

    public RoleAndPermission(IRolePermissionService service)
    {
        _service = service;
    }

    #region Permission

     /// <summary>
    /// This method get all permission from db for that role
    /// </summary>
    /// <returns>
    /// PermissionVm list (which has permission for each role)
    // </returns>
    [HttpGet]
    public IActionResult Permission(int id)
    {
        List<PermissionVM> permissions = _service.permissionList(id);
        
        return View(permissions);
    }
    #endregion

    [HttpPost]
    public async Task<IActionResult> PermissionPost(List<PermissionVM> permissions, int roleId){
        var (status, message) = await _service.UpdatePermission(permissions);
        return RedirectToAction("Permission","RoleAndPermission",new {id= roleId});
    }

   
    #region Role

    /// <summary>
    /// This action give role list from role table
    /// </summary>
    /// <returns></returns>
    public IActionResult Role()
    {
        List<RoleVM> roleList = _service.roleList();
        return View(roleList);
    }
    #endregion
}
