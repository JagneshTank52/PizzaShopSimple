using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using PizzaShop.Entity.Models;
using PizzaShop.Entity.ViewModels.OrderAppVM;
using PizzaShop.Repository.Interface;
using PizzaShop.Service.Interface;

namespace PizzaShop.Service.Implementaion;

public class OrderAppMenuService : IOrderAppMenuService
{
    private readonly IOrderItemRepository _orderItemRepository;
    private readonly ITaxesAndFeesRepository _taxRepository;
    private readonly IGenericRepository<OrdersTax> _orderTaxRepository;
    private readonly IOrderRepository _orderRepository;
    private readonly IItemRepository _itemRepository;
    private readonly IModifierRepository _modifierRepository;
    private readonly IGenericRepository<OrderModifier> _orderModifierRepository;



    public OrderAppMenuService(IOrderItemRepository orderItemRepository, IGenericRepository<OrderModifier> orderModifierRepository, IModifierRepository modifierRepository, IItemRepository itemRepository, ITaxesAndFeesRepository taxRepository, IGenericRepository<OrdersTax> orderTax, IOrderRepository orderRepository)
    {
        _orderItemRepository = orderItemRepository;
        _taxRepository = taxRepository;
        _orderTaxRepository = orderTax;
        _orderModifierRepository = orderModifierRepository;
        _orderRepository = orderRepository;
        _itemRepository = itemRepository;
        _modifierRepository = modifierRepository;
    }


    // SAVE ITEM TO ORDER
    public async Task<(bool status, string message)> AddItemToOrder(OrderRequestVM orderVm, int createrId)
    {
        Order? order = await _orderRepository.GetByIdAsync(orderVm.orderId);

        foreach (var existItem in orderVm.oldItems)
        {
            OrderItem? item = await _orderItemRepository.GetByIdAsync(existItem.ItemId);

            item.Quantity = existItem.Quantity;
            item.ItemInstruction = existItem.Instruction;

            _orderItemRepository.Update(item);
        }


        foreach (var newItem in orderVm.newItems)
        {
            Item? item = await _itemRepository.GetByIdAsync(newItem.ItemId);

            OrderItem orderItem = new OrderItem
            {
                OrderId = orderVm.orderId,
                ItemId = newItem.ItemId,
                Quantity = newItem.Quantity,
                ItemName = item!.Name,
                ItemInstruction = newItem.Instruction,
                TotalModifier = newItem.ModifierList.Count(),
                Rate = item.UnitPrice,
                TotalAmount = item.UnitPrice * newItem.Quantity,
                OrderStatus = 7, // In Progress
                IsItemReady = false,
                PreparedItem = 0,
                CreatedAt = DateTime.Now,
                CreatedBy = createrId,
            };

            int orderItemId = await _orderItemRepository.AddOrderItem(orderItem);


            decimal modifierAmount = 0;

            foreach (var modifier in newItem.ModifierList)
            {

                Modifier? existingModifier = await _modifierRepository.GetByIdAsync(modifier);
                OrderModifier itemModifier = new OrderModifier
                {
                    OrderItemId = orderItemId,
                    Modifierid = modifier,
                    Quantity = newItem.Quantity,
                    Rate = existingModifier!.UnitPrice,
                    TotalAmount = existingModifier!.UnitPrice * newItem.Quantity,
                    CreatedAt = DateTime.Now,
                    CreatedBy = createrId
                };

                modifierAmount += itemModifier.Rate;

                bool isAdded = _orderModifierRepository.Add(itemModifier);
                if (!isAdded)
                {
                    return (false, "Modifier not added");
                }
            }

            orderItem.ModifiersPrice = modifierAmount * newItem.Quantity;

        }

        bool isOrdered = await _orderItemRepository.SaveAsync();

        if (!isOrdered)
        {
            return (false, "Item not Added");
        }

        bool isTaxCalculated = false;

        if (order != null)
        {
            isTaxCalculated = await CountOrderTax(order, orderVm.taxList);
        }

        if (!isTaxCalculated)
        {
            return (false, "Tax not calculated");
        }

        int count = _orderItemRepository.GetAll(f => f.Id == order.Id && !f.IsItemReady).Count();

        if(count != 0){
            order.StatusId = 7; // In Progress
            _orderRepository.Update(order);
            bool isOrderUpdated = await _orderRepository.SaveAsync();

            if(!isOrderUpdated){
                return(false, "order is not updated");
            }
        }
        
        return (true, "Item Added");
    }

    // COUNT ORDER TAX
    public async Task<bool> CountOrderTax(Order order, List<int> optionalTaxList)
    {
        decimal subTotal = 0;
        decimal taxAmount = 0;
        decimal totalAmount = 0;

        subTotal += _orderItemRepository.GetAll(w => w.OrderId == order.Id && !w.IsDeleated).Sum(s => s.TotalAmount + s.ModifiersPrice);

        // Check if there are already taxes for this order
        var existingTaxes = _orderTaxRepository.GetAll(t => t.OrderId == order.Id && !t.IsDeleted).ToList();

        List<TaxAndFee> allTax = _taxRepository.GetAll(g => !g.IsDeleated && (g.IsEnabled ?? true)).ToList();


        List<TaxAndFee> defaultTax = allTax.Where(w => w.IsDefault).ToList();
        List<TaxAndFee> appliedOptionalTaxList = allTax.Where(w => !w.IsDefault && optionalTaxList.Contains(w.Id)).ToList();
        List<TaxAndFee> notAppliedOptionalTaxList = allTax.Where(w => !w.IsDefault && !optionalTaxList.Contains(w.Id)).ToList();

        foreach (var appliedOptionalTax in appliedOptionalTaxList)
        {
            // Check if the appliedOptionalTax ID exists in existingTaxes
            var tax = existingTaxes.FirstOrDefault(t => t.TaxId == appliedOptionalTax.Id);

            if (tax != null)
            {
                tax.TaxAmount = appliedOptionalTax.TaxType == 1
                    ? appliedOptionalTax.TaxAmount / 100 * subTotal
                    : appliedOptionalTax.TaxAmount;
                _orderTaxRepository.Update(tax);
                tax.IsDeleted = false;  // Update the tax in the database
                taxAmount += tax.TaxAmount;
            }
            else
            {
                // The tax doesn't exist in existingTaxes, so you can add it
                OrdersTax newTaxOnOrder = new OrdersTax
                {
                    OrderId = order.Id,
                    TaxId = appliedOptionalTax.Id,
                    IsDeleted = false,
                    TaxAmount = appliedOptionalTax.TaxType == 1
                        ? appliedOptionalTax.TaxAmount / 100 * subTotal
                        : appliedOptionalTax.TaxAmount,
                };

                taxAmount += newTaxOnOrder.TaxAmount;
                bool isUnique = _orderTaxRepository.Add(newTaxOnOrder);
                bool isAdded = await _orderTaxRepository.SaveAsync();

                if (!isAdded)
                {
                    return false;
                }
            }

        }

        foreach (var notAppliedOptionalTax in notAppliedOptionalTaxList)
        {
            var tax = existingTaxes.FirstOrDefault(t => t.TaxId == notAppliedOptionalTax.Id);

            if (tax != null)
            {
                tax.IsDeleted = true;
                _orderTaxRepository.Update(tax);  // Update the tax in the database
            }
        }

        if (existingTaxes.Any())
        {
            // Update existing taxes
            foreach (var existingTax in existingTaxes)
            {
                var defaultTaxItem = defaultTax.FirstOrDefault(dt => dt.Id == existingTax.TaxId);
                if (defaultTaxItem != null)
                {
                    existingTax.TaxAmount = defaultTaxItem.TaxType == 1 ? defaultTaxItem.TaxAmount / 100 * subTotal : defaultTaxItem.TaxAmount;
                    _orderTaxRepository.Update(existingTax);
                    taxAmount += existingTax.TaxAmount;
                }
            }
        }
        else
        {
            // Add new taxes from defaultTax
            foreach (var ordertax in defaultTax)
            {
                OrdersTax taxeOnOrder = new OrdersTax
                {
                    OrderId = order.Id,
                    TaxId = ordertax.Id,
                    TaxAmount = ordertax.TaxType == 1 ? ordertax.TaxAmount / 100 * subTotal : ordertax.TaxAmount,
                };
                taxAmount += taxeOnOrder.TaxAmount;
                bool isUnique = _orderTaxRepository.Add(taxeOnOrder);
                bool isAdded = await _orderTaxRepository.SaveAsync();

                if (!isAdded)
                {
                    return false;
                }
            }
        }



        totalAmount += subTotal + taxAmount;
        order.TotalAmount = totalAmount;
        order.SubAmount = subTotal;
        order.TaxAmount = taxAmount;

        _orderRepository.Update(order);

        bool isUpdated = await _orderItemRepository.SaveAsync();

        if (!isUpdated)
        {
            return false;
        }

        return true;
    }

    // GET ORDER TAX LIST
    public async Task<(OrderTaxVM orderTax, bool status, string message)> GetOrderTaxList(int orderId)
    {

        // Check if the order tax already exists
        int existingOrderTaxCount = _orderTaxRepository.GetAll(t => t.OrderId == orderId).Count();
        bool isNewOrder = existingOrderTaxCount == 0;

        List<TaxVM> defaultTax = new List<TaxVM>();
        List<TaxVM> optionalTax = new List<TaxVM>();

        if (isNewOrder)
        {
            // Get default taxes
            defaultTax = _taxRepository.GetAll(g => !g.IsDeleated && g.IsDefault && (g.IsEnabled ?? true)).Select(s => new TaxVM
            {
                TaxId = s.Id,
                TaxName = s.Name,
                TaxAmount = s.TaxAmount,
                isPercenteage = s.TaxType == 1 ? true : false,
            }).ToList();
            // Get optional taxes
        }
        else
        {
            // If it's an existing order, filter default taxes based on existing order tax
            // defaultTax = _orderTaxRepository.GetAll(t => t.OrderId == orderId && !t.IsDeleated, i => i.Tax).select(s => new TaxVM
            var orderTaxies = _orderTaxRepository.GetAll(filter: t => t.OrderId == orderId && t.Tax!.IsDefault, orderBy: o => o.Id, include: i => i.Include(a => a.Tax));

            defaultTax = orderTaxies.Select(s => new TaxVM
            {
                TaxId = s.Id,
                TaxName = s.Tax.Name,
                ActualAmount = s.TaxAmount,
                TaxAmount = s.Tax.TaxAmount,
                isPercenteage = s.Tax.TaxType == 1 ? true : false,
            }).ToList();
        }

        var appliedOptionalTax = _orderTaxRepository.GetAll(filter: t => t.OrderId == orderId && !t.Tax!.IsDefault, orderBy: o => o.Id, include: i => i.Include(a => a.Tax));

        optionalTax = _taxRepository.GetAll(g => !g.IsDeleated && !g.IsDefault && (g.IsEnabled ?? true)).Select(s => new TaxVM
        {
            TaxId = s.Id,
            TaxName = s.Name,
            TaxAmount = s.TaxAmount,
            ActualAmount = 0,
            isPercenteage = s.TaxType == 1 ? true : false,
        }).ToList();

        foreach (var tax in appliedOptionalTax)
        {
            var appliedTax = optionalTax.FirstOrDefault(f => f.TaxId == tax.TaxId);
            appliedTax.ActualAmount = tax.TaxAmount;
        }

        // Calculate subTotal and totalAmount from the order
        Order? order = await _orderRepository.GetByIdAsync(orderId);
        decimal subTotal = order?.SubAmount ?? 0; // Assuming SubAmount is already calculated
        decimal totalAmount = order?.TotalAmount ?? 0; // Assuming TotalAmount is already calculated

        OrderTaxVM orderTax = new OrderTaxVM
        {
            SubTotal = subTotal,
            TotalAmount = totalAmount,
            DefaultTaxList = defaultTax,
            OptionalTaxList = optionalTax,
        };


        return (orderTax, true, "All Tax fetched");
        // ... continue with the rest of the logic to handle taxes and return the result
    }
}
