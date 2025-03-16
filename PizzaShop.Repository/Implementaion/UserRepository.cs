using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using PizzaShop.Entity.Data;
using PizzaShop.Entity.Models;

using PizzaShop.Repository.Interface;

namespace PizzaShop.Repository.Implementaion;

public class UserRepository : IUserRepository
{

    public readonly PizzaShopContext _context;

    public UserRepository(PizzaShopContext context)
    {
        _context = context;
    }

    #region CRUD OPERATION
    public async Task<bool> AddUserAsync(User user)
    {
        try
        {
            _context.Add(user);
            await _context.SaveChangesAsync();
            return true;
        }
        catch
        {
            return false;
        }
    }
    public async Task<bool> UpdateUserAsync(User user)
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
    #endregion

    #region  GET LIST
    public IQueryable<User>? getUserList()
    {
        var userList = _context.Users.Include(u => u.UserRole).Where(x => !x.IsDeleated).OrderBy(o => o.Id).AsQueryable();
        return userList;
    }
    public List<SelectListItem>? getCountryList()
    {
        return (from c in _context.Countries
                select new SelectListItem
                {
                    Value = c.Id.ToString(),
                    Text = c.Name
                }).ToList();
    }
    public List<SelectListItem>? getStateListByCountryID(int Id)
    {
        return (from c in _context.States
                where c.CountryId == Id
                select new SelectListItem
                {
                    Value = c.Id.ToString(),
                    Text = c.Name
                }).ToList();
    }
    public List<SelectListItem>? getCityListByStateID(int Id)
    {
        return (from c in _context.Cities
                where c.StateId == Id
                select new SelectListItem
                {
                    Value = c.Id.ToString(),
                    Text = c.Name
                }).ToList();

    }
    public List<SelectListItem>? getRoleListByID()
    {
        return _context.UserRoles.Select(c => new SelectListItem()
        {
            Text = c.Name,
            Value = c.Id.ToString()
        }).ToList();
    }
    // public List<SelectListItem>? getRoleListByID(int id)
    // {
    //     return _context.UserRoles.Where(w => w.Id >= id).Select(c => new SelectListItem()
    //     {
    //         Text = c.Name,
    //         Value = c.Id.ToString()
    //     }).ToList();
    // }
    #endregion

   #region GET USER 
    public async Task<User?> GetUserByEmailAsync(string email)
    {
        return await _context.Users.Include
            ("UserRole").FirstOrDefaultAsync(u => u.Email == email);
    }

    public async Task<User?> GetUserByIdAsync(int id)
    {
        return await _context.Users.FirstOrDefaultAsync(u => u.Id == id);
    }
    #endregion 
};
    

