using System;
using System.Collections.Generic;

namespace PizzaShop.Entity.Models;

public partial class OrderItem
{
    public int Id { get; set; }

    public int OrderId { get; set; }

    public int ItemId { get; set; }

    public int Quantity { get; set; }

    public string ItemName { get; set; } = null!;

    public string? ItemInstruction { get; set; }

    public int? TotalModifier { get; set; }

    public decimal UnitPrice { get; set; }

    public decimal Rate { get; set; }

    public decimal TotalAmount { get; set; }

    public decimal TaxPercantage { get; set; }

    public int OrderStatus { get; set; }

    public int IsReadyItem { get; set; }

    public bool IsDeleated { get; set; }

    public DateTime CreatedAt { get; set; }

    public int CreatedBy { get; set; }

    public DateTime? ModifiedAt { get; set; }

    public int? ModifiedBy { get; set; }

    public virtual Item Item { get; set; } = null!;

    public virtual Item ItemNameNavigation { get; set; } = null!;

    public virtual OrderStatus OrderStatusNavigation { get; set; } = null!;
}
