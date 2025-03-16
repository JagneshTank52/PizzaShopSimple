using System;
using System.Collections.Generic;

namespace PizzaShop.Entity.Models;

public partial class OrderModifier
{
    public int Id { get; set; }

    public int OrderItemId { get; set; }

    public int Modifierid { get; set; }

    public int Quantity { get; set; }

    public decimal Rate { get; set; }

    public decimal TotalAmount { get; set; }

    public bool IsDeleated { get; set; }

    public DateTime CreatedAt { get; set; }

    public int CreatedBy { get; set; }

    public DateTime? ModifiedAt { get; set; }

    public int? ModifiedBy { get; set; }
}
