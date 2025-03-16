using PizzaShop.Entity.Models;
using PizzaShop.Entity.ViewModels.RolePermissionVM;

namespace PizzaShop.Repository.Interface;

public interface IRolePermissionRepository
{
    // Get Role list from Db
    List<RoleVM> getRoleList();

    // Get Permission for Role
    IQueryable<RoleWisePermission> getPermissions(int roleId);

    Task<RoleWisePermission?> getPermissionById(int id);

    void updatePermission(RoleWisePermission permission);
    Task SaveChanges();
}
