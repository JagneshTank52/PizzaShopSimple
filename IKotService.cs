using PizzaShop.Entity.ViewModels.OrderAppVM;
using PizzaShop.Entity.ViewModels.OrderVM;
using PizzaShop.Entity.Models; 

namespace PizzaShop.Service.Interface;

public interface IKotService
{
    public Task<OrderTicketList> GetOrderTicketList(int categoryId, string itemStatus, int pageIndex);
    public Task<List<OrderItemVM>> GetOrderItem(int orderId, string itemStatus, int categoryId);
    public OrderItemVM GetOrderItemVM(OrderItem s, string itemStatus);

    public Task<(bool status, string message)> UpdateOrderItemStatus(List<OrderItemStatusVM> orderItem);
}
