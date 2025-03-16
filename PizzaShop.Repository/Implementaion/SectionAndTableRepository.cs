using System.Threading.Tasks;
using PizzaShop.Entity.Data;
using PizzaShop.Entity.Models;
using PizzaShop.Repository.Interface;

namespace PizzaShop.Repository.Implementaion;

public class SectionAndTableRepository : ISectionAndTableRepository
{
    private readonly PizzaShopContext _context;

    public SectionAndTableRepository(PizzaShopContext context)
    {
        _context = context;
    }

    // ========== CRUD OPERATION ===========

    public bool AddSection(Section section)
    {
        try
        {
            _context.Add(section);
            return true;  
        }
        catch
        {
            return false;
        }
    }
    public bool UpdateSection(Section section)
    {
        try
        {
            _context.Update(section);
            return true;  
        }
        catch
        {
            return false;
        }
    }
    public bool UpdateTable(Table table)
    {
        try
        {
            _context.Update(table);
            return true;  
        }
        catch
        {
            return false;
        }
    }

    public async Task SaveChangesAsync()
    {
        await _context.SaveChangesAsync();
    }
    public void SaveChanges()
    {
        _context.SaveChanges();
    }

    // ========== GET BY ID ===========

    public Section? GetSectionById(int id)
    {
        return _context.Sections.FirstOrDefault(f => f.Id == id);
    }

    // =========== GET LIST ==========

    public IQueryable<Section?> GetSectionList()
    {
        var sectionList = _context.Sections.Where(x => !x.IsDeleated).OrderBy(o => o.Id).AsQueryable();
        return sectionList;
    }

    public IQueryable<Table>? GetTableList(int id)
    {
       var tableList = _context.Tables.Where(x => !x.IsDeleated && x.SectionId == id).OrderBy(o => o.Id).AsQueryable();
        return tableList;
    }

    

}


