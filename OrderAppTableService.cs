using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using PizzaShop.Entity.Models;
using PizzaShop.Entity.ViewModels.OrderAppVM;
using PizzaShop.Entity.ViewModels.SectionAndTableVM;
using PizzaShop.Repository.Interface;
using PizzaShop.Service.Interface;

namespace PizzaShop.Service.Implementaion;

public class OrderAppTableService : IOrderAppTableService
{
    private readonly ISectionRepository _sectionRepository;
    private readonly ITableRepository _tableRepository;
    private readonly IWatingRepository _watingRepository;
    private readonly ICustomerRepository _customerRepository;
    private readonly IOrderRepository _orderRepository;
    public readonly IGenericRepository<TableOrderMapping> _tableMappingRepository;

    public OrderAppTableService(IGenericRepository<TableOrderMapping> tableMappingRepository, ISectionRepository sectionRepository, ITableRepository tableRepository, ICustomerRepository customerRepository, IWatingRepository watingRepository, IOrderRepository orderRepository)
    {
        _sectionRepository = sectionRepository;
        _tableRepository = tableRepository;
        _watingRepository = watingRepository;
        _customerRepository = customerRepository;
        _orderRepository = orderRepository;
        _tableMappingRepository = tableMappingRepository;
    }

    // GET WAITING TOKEN BY SECTION ID
    public async Task<WatingTokenVM> GetWatingToken(int watingId, int sectionId)
    {
        WatingTokenVM newWatingToken = new WatingTokenVM();

        List<SelectListItem> sectionList = LoadDefaultSectionList();

        newWatingToken.SectionList = sectionList;

        if (watingId == 0)
        {
            if (sectionId != 0)
            {
                newWatingToken.SectionId = sectionId;
            }
            return newWatingToken;
        }

        Wating? watingToken = await _watingRepository.GetWatingById(watingId);

        if (watingToken == null)
        {
            // newWatingToken.SectionId = sectionId;
            return newWatingToken;
        }

        newWatingToken.WatingId = watingId;
        newWatingToken.Email = watingToken.Customer.Email;
        newWatingToken.Name = watingToken.Customer.Name;
        newWatingToken.PhoneNumber = watingToken.Customer.Phone!;
        newWatingToken.SectionId = watingToken.SectionId;
        newWatingToken.TotalPerson = watingToken.NoOfPerson;

        return newWatingToken;
    }

    // GET SECTION LIST WITH TABLE LIST
    public List<SectionVM> SectionList()
    {
        var sectionList = _sectionRepository.GetSectionListWithTable();

        List<SectionVM> sectionListVm = sectionList.Select(
            s => new SectionVM()
            {
                Id = s!.Id,
                SectionName = s.Name,
                Description = s.Description,
                TableList = s.Tables
                    .Where(w => w.SectionId == s.Id)
                    .Select(s =>
                    {
                        TableOrderMapping? tableOrderMapping = s.TableOrderMappings.Where(w => !w.IsDeleated).FirstOrDefault();

                        return new TableCardVM
                        {
                            Id = s.Id,
                            orderId = tableOrderMapping?.OrderId ?? 0,
                            SectionId = s.SectionId.GetValueOrDefault(),
                            StatusId = s.Status,
                            StatusName = s.StatusNavigation.Name,
                            TableName = s.Name,
                            Capacity = s.Capacity,
                            PaidAmount = s.TableOrderMappings.Where(w => w.Order != null).Sum(s => s.Order.TotalAmount),
                            Time = tableOrderMapping != null ? DateTime.Now.Subtract(tableOrderMapping.CreatedAt) : TimeSpan.Zero
                        };
                    }).ToList(),
                AvaiableCount = s.Tables.Where(w => w.Status == 1).Count(),
                AssignedCount = s.Tables.Where(w => w.Status == 5).Count(),
                RunningCount = s.Tables.Where(w => w.Status == 3).Count(),
            }
        ).ToList();

        return sectionListVm;
    }

    // LOAD DEFAULT
    public List<SelectListItem> LoadDefaultSectionList()
    {

        var selectList = _sectionRepository.GetSectionList().Select(s => new SelectListItem
        {
            Value = s!.Id.ToString(),
            Text = s.Name
        }).ToList();

        return selectList;
    }

    // CREATE WATING TOKEN
    public async Task<(bool status, string message)> CreateWatingToken(WatingTokenVM watingTokenVm, int createrId)
    {
        Customer? customer;
        int customerId;

        customer = await _customerRepository.GetCustomerByEmail(watingTokenVm.Email);

        if (customer != null)
        {
            if (customer.Phone != watingTokenVm.PhoneNumber || customer.Name != watingTokenVm.Name)
            {
                customer.Name = watingTokenVm.Name;
                customer.Phone = watingTokenVm.PhoneNumber;
            }
            customer.ModifiedBy = createrId;
            customer.ModifiedAt = DateTime.Now;
            customerId = customer.Id;

            bool isUpdated = _customerRepository.Update(customer);
        }
        else
        {
            customer = new Customer
            {
                Name = watingTokenVm.Name,
                Email = watingTokenVm.Email,
                Phone = watingTokenVm.PhoneNumber,
                CreatedAt = DateTime.Now,
                CreatedBy = createrId,
            };

            customerId = _customerRepository.AddCustomer(customer);
        }

        Wating watingToken = new Wating
        {
            CustomerId = customerId,
            SectionId = watingTokenVm.SectionId,
            NoOfPerson = watingTokenVm.TotalPerson,
            IsAssigned = false,
            CreatedAt = DateTime.Now,
            CreatedBy = createrId,
        };

        bool isUnique = _watingRepository.Add(watingToken);
        bool isAdded = await _watingRepository.SaveAsync();

        if (!isAdded)
        {
            return (isAdded, "Server Error");
        }

        return (isAdded, "Waiting token generated successfully");
    }

    // UPDATE WATING TOKEN
    public async Task<(bool status, string message)> UpdateWatingToken(WatingTokenVM watingTokenVm, int createrId)
    {
        Wating? watingToken = await _watingRepository.GetWatingById(watingTokenVm.WatingId);

        if(watingToken == null){
            return(false,"Wating Token not exist.");
        }

        watingToken.Customer.Name = watingTokenVm.Name;
        watingToken.Customer.Phone = watingTokenVm.PhoneNumber;
        watingToken.SectionId = watingTokenVm.SectionId;
        watingToken.NoOfPerson = watingTokenVm.TotalPerson;
        watingToken.ModifiedAt = DateTime.Now;
        watingToken.ModifiedBy = createrId;

        bool isUpdated = _watingRepository.Update(watingToken);

        if(!isUpdated){
            return(false, "Wating token does not update.");
        }

        bool isSaved = await _watingRepository.SaveAsync();

        if(!isSaved){
            return(false, "Wating token does not update.");
        }

        return(true,"Waiting token updated successfully");

    }

    // EVENT ON SELECT TABLE 
    public async Task<bool> SelectTable(int tableId, string status)
    {
        var table = await _tableRepository.GetByIdAsync(tableId);

        if (table == null)
        {
            return false;
        }

        if (table.IsAvaiable.GetValueOrDefault())
        {
            table.Status = 4;
            table.IsAvaiable = false; // 1 - Avaiable status
        }
        else
        {
            if (table.Status == 4)
            {  // 4 - Selected status
                table.Status = 1;
                table.IsAvaiable = true;
            }
        }

        bool isUpdate = await _tableRepository.SaveAsync();

        if (!isUpdate)
        {
            return false;
        }
        return true;
    }

    // GET ASSIGNMENT MODAL
    public AssignTableVM GetAssignModal(int sectionId)
    {
        AssignTableVM newAssign = new AssignTableVM();

        newAssign.SectionId = sectionId;
        newAssign.SectionList = LoadDefaultSectionList();

        var watingList = _watingRepository.GetAll(f => !f.IsDeleted && f.SectionId == sectionId, o => o.WatingId, q => q.Include(i => i.Customer));

        newAssign.watingLists = watingList.Select(
            s => new WatingListVM
            {
                WatingId = s.WatingId,
                Name = s.Customer.Name,
                Email = s.Customer.Email,
                PhoneNumber = s.Customer.Phone!,
                NoOfPerson = s.NoOfPerson
            }).ToList();

        return newAssign;
    }

    // ASSIGN TABLE
    public async Task<(int orderId, bool? status, string? message)> AssignTable(AssignTableVM assignTableVM, int createrId)
    {
        Customer? customer = new Customer();
        Wating? watingCustomer;
        int customerId;

        // Chek for customer is in wating list or not
        if (assignTableVM.WatingId != 0)
        {
            watingCustomer = await _watingRepository.GetByIdAsync(assignTableVM.WatingId);
            customerId = watingCustomer!.CustomerId;
            // customer = await _customerRepository.GetByIdAsync(watingCustomer!.CustomerId);

            watingCustomer.IsDeleted = true;
            await _watingRepository.DeleteAsync(watingCustomer.WatingId);
        }
        else
        {
            customer.Name = assignTableVM.Name;
            customer.Email = assignTableVM.Email;
            customer.Phone = assignTableVM.PhoneNumber;
            customer.CreatedAt = DateTime.Now;
            customer.CreatedBy = createrId;

            customerId = _customerRepository.AddCustomer(customer);
        }

        Order newOrder = new Order
        {
            Date = DateOnly.FromDateTime(DateTime.Now), 
            OrderInstruction = "",
            CustomerId = customerId,
            CreatedAt = DateTime.Now,
            CreatedBy = createrId,
            SubAmount = 0,
            TaxAmount = 0,
            PaidAmount = 0,
            StatusId = 5,
        };

        int orderId = _orderRepository.AddOrder(newOrder);

        bool isAssign = await _orderRepository.SaveAsync();

        if (!isAssign)
        {
            return (orderId, false, "Table Not Assign");
        }

        bool isTableAssign = _tableMappingRepository.Add(new TableOrderMapping
        {
            OrderId = orderId,
            TableId = assignTableVM.SelectedTableList[0],
            NoOfPerson = assignTableVM.TotalPerson,
            CreatedBy = createrId
        });

        Table? table = await _tableRepository.GetByIdAsync(assignTableVM.SelectedTableList[0]);

        table.Status = 5; //Assigned
        table.ModifiedAt = DateTime.Now;
        table.ModifiedBy = createrId;

        _tableRepository.Update(table);

        await _tableRepository.SaveAsync();
        return (orderId, true, "Table Assign");
    }

}
