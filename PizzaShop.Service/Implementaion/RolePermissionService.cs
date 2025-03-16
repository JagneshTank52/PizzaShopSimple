using System.Threading.Tasks;
using PizzaShop.Entity.Models;
using PizzaShop.Entity.ViewModels.RolePermissionVM;
using PizzaShop.Repository.Interface;
using PizzaShop.Service.Interface;

namespace PizzaShop.Service.Implementaion;

public class RolePermissionService : IRolePermissionService
{
    public readonly IRolePermissionRepository _repository;

    public RolePermissionService(IRolePermissionRepository repository)
    {
        _repository = repository;
    }

    /// <summary> Give Permission List Role Wise by converting in PermissionVM </summary>
    /// <returns> Return List of IQueryable Permission </returns>    
    public List<PermissionVM> permissionList(int roleId)
    {
        var query = _repository.getPermissions(roleId);

        List<PermissionVM> permissions = query.Select(s => new PermissionVM()
        {
            Id = s.Id,
            RoleId = s.RoleId,
            PermissionId = s.PermissionId,
            PermissionName = s.Permission.Name,
            CanView = s.CanView,
            CanDelete = s.CanDelete,
            CanEdit = s.CanEdit
        }).ToList();

        return permissions;
    }

    /// <summary> Get updated permission list and  </summary>
    /// <returns> Return List of IQueryable Permission </returns> 
    public async Task<(bool status, string message)> UpdatePermission(List<PermissionVM> permissions)
    {
        foreach(var p in permissions){
            
            RoleWisePermission? permission = await _repository.getPermissionById(p.Id); 

            if(permission != null){
                permission.CanView  = p.CanView;
                permission.CanEdit = p.CanEdit;
                permission.CanDelete = p.CanDelete;
            }

            _repository.updatePermission(permission!);
        }

        await _repository.SaveChanges();

        return(true, "Permision Changed");
         
    }

    public List<RoleVM> roleList()
    {
        return _repository.getRoleList();
    }


}
