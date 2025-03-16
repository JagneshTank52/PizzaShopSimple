using PizzaShop.Entity.ViewModels.SectionAndTableVM;
using PizzaShop.Service.Helper;

namespace PizzaShop.Service.Interface;

public interface ISectionAndTableService
{
    // SECTION
    public Task<List<SectionVM>> SectionList();
    public SectionVM GetSection(int id);
    public (bool status, string message) AddSection(SectionVM sectionVM,int createrId);
    public (bool status, string message) EditSection(SectionVM sectionVM,int modifierId);
    public Task<(bool status, string? message)> DeleteSection(int id);



    //  TABLE
    public Task<PaginatedList<TableVm>> TableList(string? searchString, string? sorting, int pageIndex, int pageSize, int id);

}
