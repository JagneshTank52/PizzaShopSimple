using System.Linq.Expressions;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using PizzaShop.Entity.Models;
using PizzaShop.Entity.ViewModels.MenuVM;
using PizzaShop.Entity.ViewModels.OrderAppVM;
using PizzaShop.Entity.ViewModels.OrderVM;
using PizzaShop.Repository.Implementaion;
using PizzaShop.Repository.Interface;
using PizzaShop.Service.Helper;
using PizzaShop.Service.Interface;

namespace PizzaShop.Service.Implementaion;

public class KotService : IKotService
{
    private readonly IOrderRepository _orderRepository;
    private readonly IOrderItemRepository _orderItemRepository;

    public KotService(IOrderRepository orderRepository, IOrderItemRepository orderItemRepository)
    {
        _orderRepository = orderRepository;
        _orderItemRepository = orderItemRepository;
    }

    // GET ORDER TICKET LIST 
    public OrderTicketList GetOrderTicketList(int categoryId, string itemStatus, int pageIndex)
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


        var pageResult = _orderRepository.GetAll(filter,orderBy,include);

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
            Items = s.OrderItems.Where(w => (categoryId == 0 || w.Item.CategoryId == categoryId) && 
                                     (itemStatus == "Ready" ? w.PreparedItem > 0 : w.PreparedItem < w.Quantity) && 
                                     !w.IsItemReady).Select(s => new OrderItemVM
            {
                Id = s.Id,
                ItemId = s.ItemId,
                Name = s.Item.Name,
                ItemInstruction = s.ItemInstruction ?? "No Instruction",
                Quantity = s.Quantity,
                preparedItem = s.PreparedItem,
                itemStatus = itemStatus == "Ready" ? true : false,
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
            time = s.OrderItems.Any(i => i.PreparedItem > 0) 
                ? s.OrderItems.Where(i => i.PreparedItem > 0).Min(i => i.ModifiedAt).ToString("o") 
                : s.CreatedAt.GetValueOrDefault().ToString("o")
        }).Where(w => w.Items.Count != 0).ToList();

        
        OrderTicketList orderList = new OrderTicketList{
            orderTicket = orderTicketList.Skip((pageIndex-1) * 4).Take(4).ToList(),
            totalOrder = pageResult.Count(), 
        };

        // orderTicketList.Any(a => a.Items.Count() != 0);
        return orderList;

    }

    // GET ORDER ITEM
    public List<OrderItemVM> GetOrderItem(int orderId, string itemStatus, int categoryId)
    {
        var orderItems = _orderItemRepository.GetOrderItem(orderId);

        // filte according to category
        orderItems = orderItems.Where(w => (categoryId == 0 || w.Item.CategoryId == categoryId) && (itemStatus == "Ready" ? w.PreparedItem > 0 : w.PreparedItem < w.Quantity));

        List<OrderItemVM> orderItemList = orderItems.Select(s => new OrderItemVM
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
        }).ToList();

        return orderItemList;
    }

    // UPDATE ORDER ITEM
    public async Task<(bool status, string message)> UpdateOrderItemStatus(List<OrderItemStatusVM> orderItem)
    {
        bool isUpdated = false;

        foreach (var i in orderItem)
        {
            var item = await _orderItemRepository.GetByIdAsync(i.OrderItemId);

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
                item.PreparedItem -= i.ItemQuantity; // Decrement prepared items

                // Check if the item is no longer fully prepared
                if (item.PreparedItem < item.Quantity)
                {
                    item.IsItemReady = false; // Mark item as not ready
                }
            }

            // Update the item in the repository
            isUpdated = _orderItemRepository.Update(item);
        }

        await _orderItemRepository.SaveAsync();

        // Call the method to update the order status
        await UpdateOrderStatus(orderItem[0].OrderId); // Assuming OrderId is part of the orderItem

        // Return status and message
        if (!isUpdated)
        {
            return (isUpdated, "Order not Updated");
        }

        return (isUpdated, "Order updated");
    }

    private async Task UpdateOrderStatus(int orderId)
    {
        var order = await _orderRepository.GetByIdAsync(orderId);
        if (order == null) return;

        // Check if all items are marked as ready
        bool allItemsReady = order.OrderItems.All(i => i.IsItemReady);

        // Update order status
        if (allItemsReady)
        {
            order.StatusId =  5;/* Set the ID for "Served" status */;
        }

        _orderRepository.Update(order);
        await _orderRepository.SaveAsync();
    }

}
