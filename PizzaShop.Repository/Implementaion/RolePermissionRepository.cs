using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using PizzaShop.Entity.Data;
using PizzaShop.Entity.Models;
using PizzaShop.Entity.ViewModels.RolePermissionVM;
using PizzaShop.Repository.Interface;

namespace PizzaShop.Repository.Implementaion;

public class RolePermissionRepository : IRolePermissionRepository
{
    readonly PizzaShopContext _context;

    public RolePermissionRepository(PizzaShopContext context)
    {
        _context = context;
    }

    public async Task<RoleWisePermission?> getPermissionById(int id)
    {
        return await _context.RoleWisePermissions.FirstOrDefaultAsync(f => f.Id == id);
    }


    /// <summary> Give Permission List Role Wise </summary>
    /// <returns> Return List of IQueryable Permission </returns>
    public IQueryable<RoleWisePermission> getPermissions(int roleId)
    {
        return _context.RoleWisePermissions.Include(i => i.Permission).Where(w => w.RoleId == roleId && !w.IsDeleated);
    }


    public List<RoleVM> getRoleList()
    {
        return _context.UserRoles.Select(s => new RoleVM(){
            Id = s.Id,
            RoleName = s.Name
        }).ToList();
    }

    public async Task SaveChanges()
    {
        await _context.SaveChangesAsync();
    }


    public void updatePermission(RoleWisePermission permission)
    {
        try{
            _context.RoleWisePermissions.Update(permission);
        }
        catch(Exception e)
        {
            Console.WriteLine(e);
        }
    }
}
