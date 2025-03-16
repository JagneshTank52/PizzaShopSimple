using PizzaShop.Entity.ViewModels.RolePermissionVM;

namespace PizzaShop.Service.Interface;

public interface IRolePermissionService 
{
    List<RoleVM> roleList();

    List<PermissionVM> permissionList(int roleId);

    Task<(bool status, string message)> UpdatePermission(List<PermissionVM> permissions);
}
