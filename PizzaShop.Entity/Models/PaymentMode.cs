using System;
using System.Collections.Generic;

namespace PizzaShop.Entity.Models;

public partial class PaymentMode
{
    public int Id { get; set; }

    public string Name { get; set; } = null!;

    public virtual ICollection<Order> Orders { get; } = new List<Order>();
}
