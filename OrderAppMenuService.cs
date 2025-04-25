using System.Threading.Tasks;
using PizzaShop.Entity.Models;
using PizzaShop.Repository.Interface;
using PizzaShop.Service.Interface;

namespace PizzaShop.Service.Implementaion;

public class OrderAppMenuService : IOrderAppMenuService
{
    private readonly IOrderItemRepository _orderItemRepository;
    private readonly ITaxesAndFeesRepository _taxRepository;
    private readonly IGenericRepository<OrdersTax> _orderTaxRepository;
    private readonly IOrderRepository _orderRepository;


    public OrderAppMenuService(IOrderItemRepository orderItemRepository, ITaxesAndFeesRepository taxRepository,IGenericRepository<OrdersTax> orderTax,IOrderRepository orderRepository)
    {   
        _orderItemRepository = orderItemRepository;
        _taxRepository = taxRepository;
        _orderTaxRepository = orderTax;
        _orderRepository = orderRepository;
    }

    public async Task<string> GetOrderTax(int orderId){

        Order? order = await _orderRepository.GetByIdAsync(orderId);
        decimal subTotal = 0;
        decimal taxAmount = 0;
        decimal totalAmount = 0;
        


        subTotal += _orderItemRepository.GetAll(w => w.OrderId == orderId && !w.IsDeleated).Sum(s => s.TotalAmount  + s.ModifiersPrice);
        // totalAmount = 

        List<TaxAndFee> defaultTax = _taxRepository.GetAll(g => !g.IsDeleated && g.IsDefault && (g.IsEnabled ?? true)).ToList();

        foreach(var ordertax in defaultTax){
            OrdersTax taxeOnOrder = new OrdersTax{
                OrderId = orderId,
                TaxId = ordertax.Id,
                TaxAmount = ordertax.TaxType == 1 ? ordertax.TaxAmount / 100 * subTotal : ordertax.TaxAmount,
            };
            taxAmount += taxeOnOrder.TaxAmount;
            bool isUnique = _orderTaxRepository.Add(taxeOnOrder);
            bool isAdded = await _orderTaxRepository.SaveAsync();

            if(!isAdded){
                return "false";
            }
        }

        totalAmount += subTotal + taxAmount;
        order.TotalAmount = totalAmount;
        order.SubAmount = subTotal;
        order.TaxAmount = taxAmount;

        _orderRepository.Update(order);

        bool isUpdeted = await _orderItemRepository.SaveAsync();

        if(!isUpdeted){
            return "false";
        }
        
        return "true";
    }

    public async Task<(OrderTaxVM orderTax, bool status, string message)> GetOrderTax(int orderId){
        
        // Check if the order tax already exists
        var existingOrderTaxCount = _orderTaxRepository.GetAll(t => t.OrderId == orderId).Count();
        bool isNewOrder = existingOrderTaxCount == 0;

        List<DefaultTaxVM> defaultTax = new List<DefaultTaxVM>();
        List<DefaultTaxVM> optionalTax = new List<DefaultTaxVM>();

        if (isNewOrder) {
            // Get default taxes
            defaultTax = _taxRepository.GetAll(g => !g.IsDeleated && g.IsDefault && (g.IsEnabled ?? true)).Select(s => new DefaultTaxVM{
                TaxId = s.Id,
                TaxName = s.Name,
                TaxAmount = s.TaxAmount,
                isPercenteage = s.TaxType ==  1 : true :false,
            }).ToList();
            // Get optional taxes
        } else {
            // If it's an existing order, filter default taxes based on existing order tax
            defaultTax = _orderTaxRepository.GetAll(t => t.OrderId == orderId && !t.IsDeleated, i => i.TaxAndFee).select(s => new DefaultTaxVM{
                TaxId = s.Id,
                TaxName = s.Name,
                TaxAmount = s.TaxAmount,
                isPercenteage = s.TaxType ==  1 : true :false,
            }).ToList();
        }

            optionalTax = _taxRepository.GetAll(g => !g.IsDeleated && !g.IsDefault && (g.IsEnabled ?? true)).select(s => new DefaultTaxVM{
                TaxId = s.Id,
                TaxName = s.Name,
                TaxAmount = s.TaxAmount,
                isPercenteage = s.TaxType ==  1 : true :false,
            }).ToList();

        // Calculate subTotal and totalAmount from the order
        Order? order = await _orderRepository.GetByIdAsync(orderId);
        decimal subTotal = order?.SubAmount ?? 0; // Assuming SubAmount is already calculated
        decimal totalAmount = order?.TotalAmount ?? 0; // Assuming TotalAmount is already calculated

        OrderTaxVM orderTax = new OrderTaxVM{
            SubAmount = subTotal,
            TotalAmount = totalAmount,
            DefaultTaxList = defaultTax,
            OptionalTaxList = optionalTax,
        };


        return (orderTax, true, "All Tax fetched").
        // ... continue with the rest of the logic to handle taxes and return the result
    }
}
