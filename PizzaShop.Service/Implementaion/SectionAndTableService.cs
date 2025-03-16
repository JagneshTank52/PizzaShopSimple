using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using PizzaShop.Entity.Models;
using PizzaShop.Entity.ViewModels.SectionAndTableVM;
using PizzaShop.Repository.Interface;
using PizzaShop.Service.Helper;
using PizzaShop.Service.Interface;

namespace PizzaShop.Service.Implementaion;

public class SectionAndTableService : ISectionAndTableService
{
    private readonly ISectionAndTableRepository _repository;

    public SectionAndTableService(ISectionAndTableRepository repository)
    {
        _repository = repository;
    }

    // ========== SECTION ===========

    // GET SECTION LIST
    public async Task<List<SectionVM>> SectionList()
    {
        IQueryable<Section?> sections = _repository.GetSectionList();

        List<SectionVM> sectionList = await sections.Select(
            s => new SectionVM(){
                Id = s.Id,
                SectionName = s.Name,
                Description = s.Description
            }
        ).ToListAsync();

        return sectionList;
    }

    // GET SECTION BY ID
    public SectionVM GetSection(int id)
    {
        SectionVM sectionVM = new SectionVM();

        if (id == 0)
        {
            sectionVM.Id = 0;
            return sectionVM;
        }

        Section? section = _repository.GetSectionById(id);

        sectionVM.Id = section!.Id;
        sectionVM.SectionName = section!.Name;
        sectionVM.Description = section!.Description;
        
        return sectionVM;
    }

    // ADD SECTION
    public (bool status, string message) AddSection(SectionVM sectionVM, int createrId)
    {
        Section newSection = new Section
        {
            Name = sectionVM.SectionName,
            Description = sectionVM.Description,
            IsDeleated = false,
            CreatedAt = DateTime.Now,
            CreatedBy = createrId,
            ModifiedAt = DateTime.Now,
            ModifiedBy = createrId
        };

        var isAdded =  _repository.AddSection(newSection);

        if (!isAdded)
        {
            return (isAdded, "Server Error");
        }

        _repository.SaveChanges();
        return (isAdded, "Section Added");
    }

    // EDIT SECTION
    public (bool status, string message) EditSection(SectionVM sectionVM, int modifierId)
    {
        Section? section = _repository.GetSectionById(sectionVM.Id);

        if(section == null){
            return(false,"Section Not Exist");
        }

        section.Name = sectionVM.SectionName;
        section.Description = sectionVM.Description;
        section.ModifiedAt = DateTime.Now;
        section.ModifiedBy = modifierId;

        var isEdit = _repository.UpdateSection(section);

        if (!isEdit)
        {
            return (false, "Server Error");
        }

        _repository.SaveChanges();
        return (true, "Section Updated");
    }

    // DELETE SECTION
    public async Task<(bool status, string? message)> DeleteSection(int id)
    {
       Section? section = _repository.GetSectionById(id);

        if(section == null){

            return(false,"Section does not exsit");
        }

        section.IsDeleated = true;

        if (!_repository.UpdateSection(section))
        {
            return (false, "Server Error");
        }

        var tableList = _repository.GetTableList(id)!.ToList();
        foreach (var table in tableList)
        {
            table.IsDeleated = true;
            if(!_repository.UpdateTable(table)){
                return (false, "Server Error");
            }
        }

        await _repository.SaveChangesAsync();

        return (true, "Section Deleted Successfully"); 
    }

    // ========== TABLE ===========

    public async Task<PaginatedList<TableVm>> TableList(string? searchString, string? sorting, int pageIndex, int pageSize, int id)
    {
       IQueryable<Table>? tables = _repository.GetTableList(id);

        if (!string.IsNullOrEmpty(searchString))
        {
            tables = tables!.Where(s => s.Name!.ToLower().Contains(searchString.ToLower()));
                            
        }

        // users = sorting switch
        // {
        //     "name_asc" => users!.OrderBy(o => o.FirstName),
        //     "name_desc" => users!.OrderByDescending(o => o.FirstName),
        //     "role_asc" => users!.OrderBy(o => o.UserRole.Name),
        //     "role_desc" => users!.OrderByDescending(o => o.UserRole.Name),
        //     _ => users!.OrderBy(o => o.Id)
        // };

        var paginatedTable = tables!.Select(
            s => new TableVm
            {
                Id = s.Id,
                TableName = s.Name,
                Capacity = s.Capacity,
                SectionId = s.SectionId.GetValueOrDefault(),
            }
        );

        PaginatedList<TableVm> tableList = await PaginatedList<TableVm>.CreateAsync(paginatedTable, pageIndex, pageSize);

        return tableList; 
    }

    

}
