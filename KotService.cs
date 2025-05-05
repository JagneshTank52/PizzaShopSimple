using System.Linq.Expressions;

using Microsoft.EntityFrameworkCore;
using PizzaShop.Entity.Models;
using PizzaShop.Entity.ViewModels.MenuVM;
using PizzaShop.Entity.ViewModels.OrderAppVM;
using PizzaShop.Entity.ViewModels.OrderVM;
using PizzaShop.Repository.Interface;
using PizzaShop.Service.Helper;
using PizzaShop.Service.Interface;

namespace PizzaShop.Service.Implementaion;

public class KotService : IKotService
{
    private readonly IUnitOfWork _unitOfWork;

    public KotService(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public OrderItemVM GetOrderItemVM(OrderItem s, string itemStatus)
    {
        return new OrderItemVM
        {
            Id = s.Id,
            ItemId = s.ItemId,
            Name = s.Item.Name,
            Quantity = s.Quantity,
            preparedItem = s.PreparedItem,
            remningItem = s.Quantity - s.PreparedItem,
            itemStatus = itemStatus == "Ready" ? true : false,
            ItemPrice = s.Rate,
            TotalAmount = s.Quantity * s.Rate,
            ItemModifier = s.OrderModifiers.Select(a => new SelectedModifierVM
            {
                Id = a.Id,
                Name = a.Modifier.Name,
                Quantity = a.Quantity,
                ItemRate = a.Modifier.UnitPrice
            }).ToList(),
        };
    }

    // GET ORDER TICKET LIST 
    public async Task<OrderTicketList> GetOrderTicketList(int categoryId, string itemStatus, int pageIndex)
    {
        Expression<Func<Order, bool>> filter = f => !f.IsDeleated && f.StatusId == 7;
 
        //also add expression for order status
        if (categoryId != 0)
        {
            filter = filter.AndAlso(f => f.OrderItems.Any(i => i.Item.CategoryId == categoryId));
        }

        Expression<Func<Order, object>>? orderBy = q => q.Id;
        Func<IQueryable<Order>, IQueryable<Order>>? include = i => i.Include(o => o.Customer)
                                                                    .Include(o => o.Status)
                                                                    .Include(o => o.OrderItems)
                                                                        .ThenInclude(p => p.OrderModifiers).ThenInclude(s => s.Modifier)
                                                                    .Include(o => o.OrderItems).ThenInclude(p => p.Item)
                                                                    .Include(o => o.OrdersTaxes).ThenInclude(p => p.Tax)
                                                                    .Include(o => o.TableOrderMappings)
                                                                        .ThenInclude(mapping => mapping.Table)
                                                                            .ThenInclude(t => t.Section);


        var pageResult = await _unitOfWork.OrderRepository.GetAllAsync(filter, orderBy, include);

        if (pageResult == null)
        {
            return null;
        }

        List<OrderTicketVM> orderTicketList = pageResult.Select(s => new OrderTicketVM
        {
            id = s.Id,
            orderInstruction = s.OrderInstruction ?? "No Instruction",
            sectionName = s.TableOrderMappings.FirstOrDefault()!.Table.Section!.Name,
            tableName = s.TableOrderMappings.FirstOrDefault()!.Table.Name,
            Items = s.OrderItems.Where(w => (categoryId == 0 || w.Item.CategoryId == categoryId) && (itemStatus == "Ready" ? w.PreparedItem > 0 : w.PreparedItem < w.Quantity) && !w.IsDeleated)
                                .Select(item => GetOrderItemVM(item, itemStatus)).ToList(),
            time = itemStatus == "Ready" 
                ? s.OrderItems.Where(w => w.PreparedItem > 0).Min(w => w.ModifiedAt).GetValueOrDefault().ToString("o") 
                : s.ModifiedAt.GetValueOrDefault().ToString("o")
        }).Where(w => w.Items.Count != 0).ToList();


        OrderTicketList orderList = new OrderTicketList
        {
            orderTicket = orderTicketList.Skip((pageIndex - 1) * 4).Take(4).ToList(),
            totalOrder = pageResult.Count(),
        };

        // orderTicketList.Any(a => a.Items.Count() != 0);
        return orderList;

    }

    // GET ORDER ITEM
    public async Task<List<OrderItemVM>> GetOrderItem(int orderId, string itemStatus, int categoryId)
    {
        Expression<Func<OrderItem, bool>> filter = f => !f.IsDeleated && f.OrderId == orderId;

        Expression<Func<OrderItem, object>>? orderBy = q => q.Id;
        Func<IQueryable<OrderItem>, IQueryable<OrderItem>>? include = i => i.Include(o => o.OrderModifiers).ThenInclude(m => m.Modifier)
                                                                    .Include(o => o.Item);

        var orderItems = await _unitOfWork.OrderItemRepository.GetAllAsync(filter, orderBy, include);

        // Filter according to category
        orderItems = orderItems.Where(w => (categoryId == 0 || w.Item.CategoryId == categoryId) && (itemStatus == "Ready" ? w.PreparedItem > 0 : w.PreparedItem < w.Quantity) && !w.IsDeleated);

        List<OrderItemVM> orderItemList = orderItems.Select(s => GetOrderItemVM(s, itemStatus)).ToList();

        return orderItemList;
    }

    // UPDATE ORDER ITEM
    public async Task<(bool status, string message)> UpdateOrderItemStatus(List<OrderItemStatusVM> orderItem)
    {
        bool isUpdated = false;
        int orderId = 0;

        if (orderItem.Any())
        {
            foreach (var i in orderItem)
            {
                var item = await _unitOfWork.OrderItemRepository.GetByIdAsync(i.OrderItemId);
                orderId = item.OrderId;

                // If the item is being marked as ready
                if (i.IsReady == 0)
                {
                    item.PreparedItem += i.ItemQuantity; // Increment prepared items
                    item.ModifiedAt = DateTime.Now;

                    // Check if the item is now fully prepared
                    if (item.Quantity == item.PreparedItem)
                    {
                        item.IsItemReady = true; // Mark item as ready
                    }
                }
                else // If the item is being unmarked as ready
                {
                    item.PreparedItem -= i.ItemQuantity; 
                    item.ModifiedAt = DateTime.Now;// Decrement prepared items

                    // Check if the item is no longer fully prepared
                    if (item.PreparedItem < item.Quantity)
                    {
                        item.IsItemReady = false; // Mark item as not ready
                    }
                }

                // Update the item in the repository
                isUpdated = _unitOfWork.OrderItemRepository.Update(item);


            }
            await UpdateOrderStatus(orderId);

            await _unitOfWork.OrderItemRepository.SaveAsync();

            if (!isUpdated)
            {
                return (isUpdated, "Order not Updated");
            }

            return (isUpdated, "Order updated");
        }
        else
        {
            return (true, "No is selected");
        }

    }

    private async Task UpdateOrderStatus(int orderId)
    {
        // Fetch the order and its items in one go
        var order = await _unitOfWork.OrderRepository.GetOrderWithOrderItem(orderId);
        if (order == null) return;

        // Get the order items once
        var orderItemList = order.OrderItems.Where(i => !i.IsDeleated).ToList();

        // Check if any item is marked as ready or if the order is running
        bool anyItemReady = orderItemList.All(i => i.IsItemReady);
        bool isOrderRunning = orderItemList.Any(item => item.PreparedItem > 0);

        // Only proceed if there are changes to be made
        if (isOrderRunning || anyItemReady)
        {
            // Fetch table mappings in one go
            var tableOrderMappings = await _unitOfWork.TableMappingRepository.GetAllAsync(
                f => f.OrderId == orderId && !f.IsDeleated, 
                o => o.Id, 
                i => i.Include(a => a.Table)
            );

            var tables = tableOrderMappings
                .Where(m => m.Table != null)
                .Select(m => m.Table)
                .ToList();

            // Update table statuses if necessary
            bool tablesUpdated = false;
            foreach (var table in tables)
            {
                if (table.Status != 3) // Only update if the status is not already "Running"
                {
                    table.Status = 3; // Set status to "Running"
                    table.ModifiedAt = DateTime.Now;
                    table.ModifiedBy = 1; // Assuming 1 is the ID of the user making the change
                    tablesUpdated = true;
                }
            }

            if (tablesUpdated)
            {
                _unitOfWork.TableRepository.UpdateRange(tables);
            }

            // Update order status if necessary
            if (anyItemReady)
            {
                order.StatusId = 2; // Assuming 2 is the ID for "Served" status
                _unitOfWork.OrderRepository.Update(order);
            }

            // Save changes only once
            await _unitOfWork.SaveAsync();
        }
    }

}
