using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;
using OfficeOpenXml;
using PizzaShop.Entity.Models;
using PizzaShop.Entity.ViewModels.HelperVM;
using PizzaShop.Entity.ViewModels.OrderVM;
using PizzaShop.Repository.Interface;
using PizzaShop.Service.Helper;
using PizzaShop.Service.Interface;
using PizzaShop.Entity.ViewModels.MenuVM;
using PizzaShop.Entity.ViewModels.OrderAppVM;
using System.Threading.Tasks;

namespace PizzaShop.Service.Implementaion;

public class OrderService : IOrderService
{
    private readonly IOrderRepository _repository;
    private readonly IItemRepository _itemrepository;
    private readonly IOrderItemRepository _orderItemRepository;
    private readonly IGenericRepository<OrderModifier> _orderModifierRepository;



    public OrderService(IOrderRepository repository,IItemRepository itemRepository, IOrderItemRepository orderItemRepository, IGenericRepository<OrderModifier> orderModifierRepository)
    {
        _repository = repository;
        _itemrepository = itemRepository;
        _orderItemRepository = orderItemRepository;
        _orderModifierRepository = orderModifierRepository;
    }

    // EXPORT TO EXCEL
    public async Task<(bool status, string message, byte[]? excelBytes)> ExportToExcel(PageInfo pageInfo)
    {
        ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
        FileInfo file = new FileInfo("E:/Surprise Assignment/dummyfiles/order_data10.xlsx");


        // 1 Default Filter
        Expression<Func<Order, bool>> filter = f => !f.IsDeleated;

        // 2 Search Filter
        if (!string.IsNullOrEmpty(pageInfo.SearchString))
        {
            filter = filter.AndAlso(f => f.Id.ToString().Contains(pageInfo.SearchString) || f.Customer.Name.ToLower().Contains(pageInfo.SearchString.ToLower()));
        }

        // 3 Status Filter
        if (pageInfo.Status != 0)
        {
            filter = filter.AndAlso(f => f.StatusId == pageInfo.Status);
        }

        // 4 All Time Filter
        if (pageInfo.FromTime != 0)
        {
            filter = filter.AndAlso(f => f.Date > pageInfo.ToDate.AddDays(-pageInfo.FromTime));
        }

        // 5 From and To Date
        if (pageInfo.FromDate != DateOnly.MinValue && pageInfo.ToDate != DateOnly.MinValue)
        {
            filter = filter.AndAlso(f => f.Date >= pageInfo.FromDate && f.Date <= pageInfo.ToDate);
        }
        else if (pageInfo.FromDate != DateOnly.MinValue)
        {
            filter = filter.AndAlso(f => f.Date >= pageInfo.FromDate && f.Date <= DateOnly.FromDateTime(DateTime.Now));
        }
        else if (pageInfo.ToDate != DateOnly.MinValue)
        {
            filter = filter.AndAlso(f => f.Date <= pageInfo.ToDate);
        }

        // order by
        Func<IQueryable<Order>, IOrderedQueryable<Order>> orderBy;

        orderBy = pageInfo.Sorting switch
        {
            "order_id" => q => pageInfo.SortOrder == "asc" ? q.OrderBy(o => o.Id) : q.OrderByDescending(o => o.Id),
            "date" => q => pageInfo.SortOrder == "asc" ? q.OrderBy(o => o.Date) : q.OrderByDescending(o => o.Date),
            "customer_name" => q => pageInfo.SortOrder == "asc" ? q.OrderBy(o => o.Customer.Name) : q.OrderByDescending(o => o.Customer.Name),
            "total_amount" => q => pageInfo.SortOrder == "asc" ? q.OrderBy(o => o.TotalAmount) : q.OrderByDescending(o => o.TotalAmount),
            _ => q => q.OrderBy(o => o.Id)
        };

        // include
        Func<IQueryable<Order>, IQueryable<Order>> include = q => q.Include(i => i.Customer)
                                                           .Include(i => i.Status)
                                                           .Include(i => i.PymentModeNavigation);


        var pageResult = _repository.GetPagedRecords(pageInfo.PageSize, pageInfo.PageIndex, orderBy, filter, include, true);

        var ViewModels = pageResult.records.Select(s => new ExportOrderVM
        {
            Id = s.Id,
            Customer = s.Customer.Name,
            Status = s.Status.Name,
            PaymentMode = s.PymentModeNavigation.Name,
            Date = s.Date,
            Rating = s.Rating,
            TotalAmount = s.TotalAmount
        }).ToList();


        try
        {
            var excelPackage = await ExcelHalper.DesignExcelFile(ViewModels, pageInfo, pageResult.totalRecord);
            return (true, "File Exported", excelPackage);
        }
        catch (System.Exception)
        {
            return (false, "File Not Exported", null);
            throw;
        }
    }

    // GET lOAD DEFAULT 
    public OrderFilterVM LoadDefalut()
    {
        OrderFilterVM defaultModel = new OrderFilterVM();

        // THIS IS WRONG CHANGE IT LATER
        defaultModel.OrderStatus = _repository.GetOrderStatus();

        return defaultModel;
    }

    // GET ORDER LIST FOR ORDER PAGE IN MAIN APP
    public NewPaginatedList<OrderListVM> OrderList(PageInfo pageInfo)
    {
        // 1 Default Filter
        Expression<Func<Order, bool>> filter = f => !f.IsDeleated;

        // 2 Search Filter
        if (!string.IsNullOrEmpty(pageInfo.SearchString))
        {
            filter = filter.AndAlso(f => f.Id.ToString().Contains(pageInfo.SearchString) || f.Customer.Name.ToLower().Contains(pageInfo.SearchString.ToLower()));
        }

        // 3 Status Filter
        if (pageInfo.Status != 0)
        {
            filter = filter.AndAlso(f => f.StatusId == pageInfo.Status);
        }

        // 4 All Time Filter
        if (pageInfo.FromTime != 0)
        {
            filter = filter.AndAlso(f => f.Date > pageInfo.ToDate.AddDays(-pageInfo.FromTime));
        }

        // 5 From and To Date
        if (pageInfo.FromDate != DateOnly.MinValue && pageInfo.ToDate != DateOnly.MinValue)
        {
            filter = filter.AndAlso(f => f.Date >= pageInfo.FromDate && f.Date <= pageInfo.ToDate);
        }
        else if (pageInfo.FromDate != DateOnly.MinValue)
        {
            filter = filter.AndAlso(f => f.Date >= pageInfo.FromDate && f.Date <= DateOnly.FromDateTime(DateTime.Now));
        }
        else if (pageInfo.ToDate != DateOnly.MinValue)
        {
            filter = filter.AndAlso(f => f.Date <= pageInfo.ToDate);
        }

        // order by
        Func<IQueryable<Order>, IOrderedQueryable<Order>> orderBy;

        orderBy = pageInfo.Sorting switch
        {
            "order_id" => q => pageInfo.SortOrder == "asc" ? q.OrderBy(o => o.Id) : q.OrderByDescending(o => o.Id),
            "date" => q => pageInfo.SortOrder == "asc" ? q.OrderBy(o => o.Date) : q.OrderByDescending(o => o.Date),
            "customer_name" => q => pageInfo.SortOrder == "asc" ? q.OrderBy(o => o.Customer.Name) : q.OrderByDescending(o => o.Customer.Name),
            "total_amount" => q => pageInfo.SortOrder == "asc" ? q.OrderBy(o => o.TotalAmount) : q.OrderByDescending(o => o.TotalAmount),
            _ => q => q.OrderBy(o => o.Id)
        };

        // include
        Func<IQueryable<Order>, IQueryable<Order>> include = q => q.Include(i => i.Customer)
                                                           .Include(i => i.Status)
                                                           .Include(i => i.PymentModeNavigation);


        var pageResult = _repository.GetPagedRecords(pageInfo.PageSize, pageInfo.PageIndex, orderBy, filter, include);

        var ViewModels = pageResult.records.Select(s => new OrderListVM
        {
            Id = s.Id,
            CustomerId = s.CustomerId,
            Customer = s.Customer.Name,
            StatusId = s.StatusId,
            Status = s.Status.Name,
            PaymentModeId = 1,
            PaymentMode = "Cash",
            Date = s.Date,
            Rating = s.Rating,
            TotalAmount = s.TotalAmount
        }).ToList();

        NewPaginatedList<OrderListVM> orderList = new NewPaginatedList<OrderListVM>(ViewModels, pageResult.totalRecord, pageInfo);

        return orderList;
    }

    // GET ORDER SUMMARY
    public async Task<OrderSummaryVM>? GetOrderSummary(int orderId)
    {
        Order order = await _repository.GetOrderForSummaryAsync(orderId);

        if (order == null)
        {
            return null;
        }

        OrderSummaryVM orderSummary = new OrderSummaryVM
        {
            OrderId = orderId,
            InvoiceId = 1,
            OrderStatus = order.Status.Name,
            PaidOn = DateTime.Now,
            ModifiedOn = DateTime.Now,
            OrderDuration = "2 hour",
            CustomerName = order.Customer.Name,
            Phone = order.Customer.Phone!,
            Email = order.Customer.Email,
            NoOfPerson = order.TableOrderMappings.FirstOrDefault()!.NoOfPerson,
            TableName = order.TableOrderMappings.FirstOrDefault()!.Table.Name,
            Section = order.TableOrderMappings.FirstOrDefault()!.Table.Section!.Name,
            Items = order.OrderItems.Select(s => new OrderItemVM
            {
                Id = s.Id,
                ItemId = s.ItemId,
                Name = s.Item.Name,
                Quantity = s.Quantity,
                ItemPrice = s.UnitPrice,
                TotalAmount = s.Quantity * s.UnitPrice,
                ItemModifier = s.OrderModifiers.Select(a => new SelectedModifierVM
                {
                    Id = a.Id,
                    Name = a.Modifier.Name,
                    Quantity = a.Quantity,
                    ItemRate = a.Modifier.UnitPrice
                }).ToList()
            }).ToList(),
            taxes = order.OrdersTaxes.Select(s => new OrderTaxesVM
            {
                Id = s.Id,
                Name = s.Tax.Name,
                Amount = s.TaxAmount,
            }).ToList(),
            Subtotal = order.SubAmount,
            Total = order.TotalAmount
        };

        return orderSummary;
    }

    public async Task<OrderMenuVM> GetOrderAsync(int orderId)
    {
        Order order = await _repository.GetOrderForSummaryAsync(orderId);

        if (order == null)
        {
            return null;
        }

        OrderMenuVM orderMenu = new OrderMenuVM
        {
            OrderId = orderId,
            SectionName = order.TableOrderMappings.FirstOrDefault()!.Table.Section!.Name,
            TableList = order.TableOrderMappings.Select(s => s.Table.Name).ToList(),
        };

        return orderMenu;
    }

    public async Task<List<OrderItemVM>> GetOrderItemsAsync(int orderId)
    {
        Order order = await _repository.GetOrderForSummaryAsync(orderId);

        if (order == null)
        {
            return null;
        }

        List<OrderItemVM> orderItemList = order.OrderItems.Select(s => new OrderItemVM
        {
            Id = s.Id,
            ItemId = s.ItemId,
            Name = s.Item.Name,
            Quantity = s.Quantity,
            ItemPrice = s.UnitPrice,
            TotalAmount = s.Quantity * s.UnitPrice,
            ItemModifier = s.OrderModifiers.Select(a => new SelectedModifierVM
            {
                Id = a.Id,
                Name = a.Modifier.Name,
                Quantity = a.Quantity,
                ItemRate = a.Modifier.UnitPrice
            }).ToList()
        }).ToList();

        return orderItemList;
    }

    public List<ItemModifierGroupVM> GetItemWiseModifier(int itemId)
    {
        List<ItemModifierGroupVM> itemModifierGroupList = _itemrepository.GetItemModifierGroupList(itemId);

        return itemModifierGroupList;
    }

    public async Task<(bool status, string message)> AddItemToOrder(AddItemVM addItemVM)
    {
        OrderItem orderItem = new OrderItem
        {
            OrderId = addItemVM.orderId,
            ItemId = addItemVM.itemId,
            Quantity = 1,
            ItemName = "",
            OrderStatus = 7,
            TotalModifier = addItemVM.modifierList!.Count(),
        };

        int orderItemId = await _orderItemRepository.AddOrderItem(orderItem);

        OrderModifier itemModifier;

        foreach(var modifier in addItemVM.modifierList!){
            itemModifier = new OrderModifier{
                OrderItemId = orderItemId,
                Modifierid = modifier,
                Quantity = 1
            };

            bool isAdded = _orderModifierRepository.Add(itemModifier);
            if(!isAdded){
                return(false, "Modifier not added");
            }
        }

        bool isOrdered = await _orderItemRepository.SaveAsync();

        if(!isOrdered){
            return(false,"Item not Added");
        }
        return(true, "Item Added");
    }
}

