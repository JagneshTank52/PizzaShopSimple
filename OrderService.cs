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
using System.Runtime.CompilerServices;
using System.Threading.Tasks;

namespace PizzaShop.Service.Implementaion;

public class OrderService : IOrderService
{
    private readonly IOrderRepository _repository;
    private readonly IItemRepository _itemrepository;
    private readonly IOrderItemRepository _orderItemRepository;
    private readonly IModifierRepository _modifierRepository;
    private readonly ITaxesAndFeesRepository _taxRepository;
    private readonly IGenericRepository<TableOrderMapping> _tableOrderMappingRepository;
    private readonly ICustomerRepository _customerRepository;

    private readonly IGenericRepository<OrdersTax> _orderTaxRepository;

    private readonly IGenericRepository<OrderModifier> _orderModifierRepository;

    public OrderService(IGenericRepository<TableOrderMapping> tableOrderMappingRepository, ICustomerRepository customerRepository, IOrderRepository repository, IItemRepository itemRepository, IOrderItemRepository orderItemRepository, IGenericRepository<OrderModifier> orderModifierRepository, IModifierRepository modifierRepository, ITaxesAndFeesRepository taxRepository, IGenericRepository<OrdersTax> orderTaxRepository)
    {
        _repository = repository;
        _itemrepository = itemRepository;
        _orderItemRepository = orderItemRepository;
        _orderModifierRepository = orderModifierRepository;
        _modifierRepository = modifierRepository;
        _taxRepository = taxRepository;
        _orderTaxRepository = orderTaxRepository;
        _tableOrderMappingRepository = tableOrderMappingRepository;
        _customerRepository = customerRepository;
    }

    // EXPORT TO EXCEL
    public async Task<(bool status, string message, byte[]? excelBytes)> ExportToExcel(PageInfo pageInfo)
    {
        ExcelPackage.LicenseContext = LicenseContext.NonCommercial;

        // 1 DEFAULT FLTER
        Expression<Func<Order, bool>> filter = f => !f.IsDeleated;

        // 2 SEARCH FLTER
        if (!string.IsNullOrEmpty(pageInfo.SearchString))
        {
            filter = filter.AndAlso(f => f.Id.ToString().Contains(pageInfo.SearchString) || f.Customer.Name.ToLower().Contains(pageInfo.SearchString.ToLower()));
        }

        // 3 STATUS FLTER
        if (pageInfo.Status != 0)
        {
            filter = filter.AndAlso(f => f.StatusId == pageInfo.Status);
        }

        // 4 ALL TIME FLTER
        if (pageInfo.FromTime != 0)
        {
            filter = filter.AndAlso(f => f.Date > pageInfo.ToDate.AddDays(-pageInfo.FromTime));
        }

        // 5 FROM AND TO DATE FLTER
        if (pageInfo.FromDate != DateOnly.MinValue && pageInfo.ToDate != DateOnly.MinValue)
        {
            filter = filter.AndAlso(f => f.Date >= pageInfo.FromDate && f.Date <= pageInfo.ToDate);
        }

        // 6 ORDER BY
        Func<IQueryable<Order>, IOrderedQueryable<Order>> orderBy;

        orderBy = pageInfo.Sorting switch
        {
            "order_id" => q => pageInfo.SortOrder == "asc" ? q.OrderBy(o => o.Id) : q.OrderByDescending(o => o.Id),
            "date" => q => pageInfo.SortOrder == "asc" ? q.OrderBy(o => o.Date) : q.OrderByDescending(o => o.Date),
            "customer_name" => q => pageInfo.SortOrder == "asc" ? q.OrderBy(o => o.Customer.Name) : q.OrderByDescending(o => o.Customer.Name),
            "total_amount" => q => pageInfo.SortOrder == "asc" ? q.OrderBy(o => o.TotalAmount) : q.OrderByDescending(o => o.TotalAmount),
            _ => q => q.OrderBy(o => o.Id)
        };

        // 7 INCLUDE
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

    // GET LOAD DEFAULT 
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
        // 1 DEFAULT FLTER
        Expression<Func<Order, bool>> filter = f => !f.IsDeleated;

        // 2 SEARCH FLTER
        if (!string.IsNullOrEmpty(pageInfo.SearchString))
        {
            filter = filter.AndAlso(f => f.Id.ToString().Contains(pageInfo.SearchString) || f.Customer.Name.ToLower().Contains(pageInfo.SearchString.ToLower()));
        }

        // 3 STATUS FLTER
        if (pageInfo.Status != 0)
        {
            filter = filter.AndAlso(f => f.StatusId == pageInfo.Status);
        }

        // 4 ALL TIME FLTER
        if (pageInfo.FromTime != 0)
        {
            filter = filter.AndAlso(f => f.Date > pageInfo.ToDate.AddDays(-pageInfo.FromTime));
        }

        // 5 FROM AND TO DATE
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

        // 6 ORDER BY
        Func<IQueryable<Order>, IOrderedQueryable<Order>> orderBy;

        orderBy = pageInfo.Sorting switch
        {
            "order_id" => q => pageInfo.SortOrder == "asc" ? q.OrderBy(o => o.Id) : q.OrderByDescending(o => o.Id),
            "date" => q => pageInfo.SortOrder == "asc" ? q.OrderBy(o => o.Date) : q.OrderByDescending(o => o.Date),
            "customer_name" => q => pageInfo.SortOrder == "asc" ? q.OrderBy(o => o.Customer.Name) : q.OrderByDescending(o => o.Customer.Name),
            "total_amount" => q => pageInfo.SortOrder == "asc" ? q.OrderBy(o => o.TotalAmount) : q.OrderByDescending(o => o.TotalAmount),
            _ => q => q.OrderBy(o => o.Id)
        };

        // 7 INCLUDE
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
            PaymentModeId = s.PymentMode,
            PaymentMode = s.PymentModeNavigation.Name,
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
                ItemPrice = s.Rate,
                TotalAmount = s.Quantity * s.Rate,
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

    // GET ORDER
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

    // GET ORDER ITEMS BY ORDER ID
    public async Task<(List<OrderItemVM>? orderItems, bool status, string message)> GetOrderItemsAsync(int orderId)
    {
        Order? order = await _repository.GetOrderForSummaryAsync(orderId);

        if (order == null)
        {
            return (null, false, "Order not found");
        }

        List<OrderItemVM> orderItemList = order.OrderItems.Select(s => new OrderItemVM
        {
            Id = s.Id,
            ItemId = s.ItemId,
            Name = s.Item.Name,
            Quantity = s.Quantity,
            ItemPrice = s.Rate,
            TotalAmount = s.Quantity * s.Rate,
            MaxQuantity = s.Item.Quantity,
            ItemInstruction = s.ItemInstruction!,
            isItemReady = s.IsItemReady,
            preparedItem = s.PreparedItem,
            remningItem = s.Quantity - s.PreparedItem,
            ItemModifier = s.OrderModifiers.Select(a => new SelectedModifierVM
            {
                Id = a.Id,
                Name = a.Modifier.Name,
                Quantity = a.Quantity,
                ItemRate = a.Modifier.UnitPrice
            }).ToList()
        }).ToList();

        return (orderItemList, true, "Item fetched");
    }

    // GET ITEM WITH SELECTED MODIFIER
    public async Task<(List<OrderItemVM>? selectedItem, bool status, string message)> GetItemWithSelectedModifier(SelectedItemVM addItemVM)
    {
        List<ItemModifierGroupVM> itemModifierGroupList = _itemrepository.GetItemModifierGroupList(addItemVM.itemId);
        List<SelectedModifierVM> validModifiers = new List<SelectedModifierVM>();

        foreach (var itemModifierGroup in itemModifierGroupList)
        {
            if (addItemVM.modifierList.ContainsKey(itemModifierGroup.modifierGroupId))
            {
                List<int> selectedModifierIds = addItemVM.modifierList[itemModifierGroup.modifierGroupId];

                if (selectedModifierIds.Count >= itemModifierGroup.minModifier && selectedModifierIds.Count <= itemModifierGroup.maxModifier)
                {
                    // Filter the modifierList for valid selected modifiers
                    var validModifiersList = itemModifierGroup.modifierList
                        .Where(modifier => selectedModifierIds.Contains(modifier.Id));// Filter by selected modifier Ids

                    validModifiers.AddRange(validModifiersList);
                }
                else
                {
                    return (null, false, "Selected Modifier must be with in range.");
                }
            }
        }

        Item? item = await _itemrepository.GetByIdAsync(addItemVM.itemId);

        if (item == null)
        {
            return (null, false, "Item does not found");
        }

        OrderItemVM selectedItem = new OrderItemVM
        {
            ItemId = item.Id,
            Id = 0,
            Name = item.Name,
            ItemPrice = item.UnitPrice,
            Quantity = 1,
            MaxQuantity = item.Quantity,
            ItemModifier = validModifiers
        };

        List<OrderItemVM>? selectedOrderItems = new List<OrderItemVM>{
            selectedItem
        };

        return (selectedOrderItems, true, "Item Select");
    }

    // GET ITEM WISE MODIFIER
    public List<ItemModifierGroupVM> GetItemWiseModifier(int itemId)
    {
        List<ItemModifierGroupVM> itemModifierGroupList = _itemrepository.GetItemModifierGroupList(itemId);

        return itemModifierGroupList;
    }

    // // SAVE ITEM TO ORDER
    // public async Task<(bool status, string message)> AddItemToOrder(OrderRequestVM orderVm, int createrId)
    // {
    //     Order? order = await _repository.GetByIdAsync(orderVm.orderId);

    //     foreach (var existItem in orderVm.oldItems)
    //     {
    //         OrderItem? item = await _orderItemRepository.GetByIdAsync(existItem.ItemId);

    //         item.Quantity = existItem.Quantity;

    //         _orderItemRepository.Update(item);
    //     }


    //     foreach (var newItem in orderVm.newItems)
    //     {
    //         Item? item = await _itemrepository.GetByIdAsync(newItem.ItemId);

    //         OrderItem orderItem = new OrderItem
    //         {
    //             OrderId = orderVm.orderId,
    //             ItemId = newItem.ItemId,
    //             Quantity = newItem.Quantity,
    //             ItemName = item!.Name,
    //             ItemInstruction = newItem.Instruction,
    //             TotalModifier = newItem.ModifierList.Count(),
    //             Rate = item.UnitPrice,
    //             TotalAmount = item.UnitPrice * newItem.Quantity,
    //             OrderStatus = 7, // In Progress
    //             IsItemReady = false,
    //             PreparedItem = 0,
    //             CreatedAt = DateTime.Now,
    //             CreatedBy = createrId,
    //         };

    //         int orderItemId = await _orderItemRepository.AddOrderItem(orderItem);


    //         decimal modifierAmount = 0;

    //         foreach (var modifier in newItem.ModifierList)
    //         {
                 
    //             Modifier? existingModifier = await _modifierRepository.GetByIdAsync(modifier);
    //             OrderModifier itemModifier = new OrderModifier
    //             {
    //                 OrderItemId = orderItemId,
    //                 Modifierid = modifier,
    //                 Quantity = newItem.Quantity,
    //                 Rate = existingModifier!.UnitPrice,
    //                 TotalAmount = existingModifier!.UnitPrice * newItem.Quantity,
    //                 CreatedAt = DateTime.Now,
    //                 CreatedBy = createrId
    //             };

    //             modifierAmount += itemModifier.Rate;

    //             bool isAdded = _orderModifierRepository.Add(itemModifier);
    //             if (!isAdded)
    //             {
    //                 return (false, "Modifier not added");
    //             }
    //         }

    //         orderItem.ModifiersPrice = modifierAmount;

    //     }

    //     bool isOrdered = await _orderItemRepository.SaveAsync();

    //     if (!isOrdered)
    //     {
    //         return (false, "Item not Added");
    //     }

    //     // bool isTaxCalculated = await GetOrderTax(addItemVM.orderId);

    //     // if (!isTaxCalculated)
    //     // {
    //     //     return (false, "Tax not calculated");
    //     // }

    //     return (true, "Item Added");
    // }

    // // FIND TAX ON ORDER
    // public async Task<bool> GetOrderTax(int orderId)
    // {

    //     Order? order = await _repository.GetByIdAsync(orderId);
    //     decimal subTotal = 0;
    //     decimal taxAmount = 0;
    //     decimal totalAmount = 0;



    //     subTotal += _orderItemRepository.GetAll(w => w.OrderId == orderId && !w.IsDeleated).Sum(s => s.TotalAmount + s.ModifiersPrice);
    //     // totalAmount = 

    //     List<TaxAndFee> defaultTax = _taxRepository.GetAll(g => !g.IsDeleated && g.IsDefault && (g.IsEnabled ?? true)).ToList();

    //     foreach (var ordertax in defaultTax)
    //     {
    //         OrdersTax taxeOnOrder = new OrdersTax
    //         {
    //             OrderId = orderId,
    //             TaxId = ordertax.Id,
    //             TaxAmount = ordertax.TaxType == 1 ? ordertax.TaxAmount / 100 * subTotal : ordertax.TaxAmount,
    //         };
    //         taxAmount += taxeOnOrder.TaxAmount;
    //         bool isUnique = _orderTaxRepository.Add(taxeOnOrder);
    //         bool isAdded = await _orderTaxRepository.SaveAsync();

    //         if (!isAdded)
    //         {
    //             return false;
    //         }
    //     }

    //     totalAmount += subTotal + taxAmount;
    //     order.TotalAmount = totalAmount;
    //     order.SubAmount = subTotal;
    //     order.TaxAmount = taxAmount;

    //     _repository.Update(order);

    //     bool isUpdeted = await _orderItemRepository.SaveAsync();

    //     if (!isUpdeted)
    //     {
    //         return false;
    //     }

    //     return true;
    // }

    // DELETE ORDER ITEM
    public async Task<(bool status, string message)> DeleteOrderItem(int itemId, int modifyBy)
    {

        OrderItem? orderItem = await _orderItemRepository.GetByIdAsync(itemId);

        if (orderItem == null)
        {
            return (false, "Item not found");
        }

        orderItem.IsDeleated = true;
        orderItem.ModifiedBy = modifyBy;
        orderItem.ModifiedAt = DateTime.Now;
        bool isUpdate = _orderItemRepository.Update(orderItem);

        bool isRemove = await _orderItemRepository.SaveAsync();

        if (!isRemove)
        {
            return (false, "Item not removed");
        }

        return (true, "Item remove");
    }

    // GET CUSTOMER DETAILS FROM ORDER ID
    public async Task<CustomerDetalisVM> GetCustomerDetails(int orderId)
    {
        Order? order = await _repository.GetOrderWithCustomer(orderId);
        TableOrderMapping? tableOrder = order.TableOrderMappings.FirstOrDefault();

        CustomerDetalisVM customer = new CustomerDetalisVM
        {
            CustomerId = order.CustomerId,
            orderId = orderId,
            Email = order.Customer.Email,
            Name = order.Customer.Name,
            PhoneNumber = order.Customer.Phone!,
            TotalPerson = tableOrder!.NoOfPerson,
        };

        return customer;

    }

    // UPDATE CUSTOMER DETAILS 
    public async Task<(bool status, string message)> UpdateCustomerDetails(CustomerDetalisVM customerVm)
    {

        TableOrderMapping? table = _tableOrderMappingRepository
     .GetAll(g => !g.IsDeleated && g.OrderId == customerVm.orderId)
     .FirstOrDefault();
        Customer? customer = await _customerRepository.GetByIdAsync(customerVm.CustomerId);

        if (customer == null || table == null)
        {
            return (false, "customer not find or table not assign");
        }

        customer.Name = customerVm.Name;
        customer.Phone = customerVm.PhoneNumber;
        customer.Email = customerVm.Email;
        table.NoOfPerson = customerVm.TotalPerson;

        _customerRepository.Update(customer);
        _tableOrderMappingRepository.Update(table);


        bool isUpdated = await _customerRepository.SaveAsync();

        if (!isUpdated)
        {
            return (false, "Error in update customer");
        }

        return (true, "Customer Updated Succesfully.");

    }
}

