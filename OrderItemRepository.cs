using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using PizzaShop.Entity.Data;
using PizzaShop.Entity.Models;
using PizzaShop.Entity.ViewModels.OrderVM;
using PizzaShop.Repository.Interface;

namespace PizzaShop.Repository.Implementaion;

public class OrderItemRepository : GenericRepository<OrderItem> ,IOrderItemRepository
{
    public OrderItemRepository(PizzaShopContext context) :base(context){}
    
    // GET ORDER ITEM BY ID
    public IEnumerable<OrderItem> GetOrderItem(int orderId)
    {
        var orderItem = _context.OrderItems
        .Include(i => i.OrderModifiers).ThenInclude(i => i.Modifier)
        .Include(i => i.Item)
        .Where(w => w.OrderId == orderId);

        return orderItem;
    }

    // ADD ORDER ITEM
    public async Task<int> AddOrderItem(OrderItem orderitem)
    {
        await _context.OrderItems.AddAsync(orderitem);
        await _context.SaveChangesAsync();
        return orderitem.Id;
    }

}
