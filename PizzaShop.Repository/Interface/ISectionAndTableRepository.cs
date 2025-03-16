using PizzaShop.Entity.Models;

namespace PizzaShop.Repository.Interface;

public interface ISectionAndTableRepository
{
    //  GET LIST

    IQueryable<Section?> GetSectionList();
    IQueryable<Table>? GetTableList(int id);

    // CRUD OPERATION
    Section? GetSectionById(int id);
    bool AddSection(Section section);

    bool UpdateSection(Section section);
    bool UpdateTable(Table table);

    Task SaveChangesAsync();
    void SaveChanges();

}
