using System;
using System.Collections.Generic;

namespace PizzaShop.Entity.Models;

public partial class PaymentMethod
{
    public int PaymentMethodId { get; set; }

    public string Name { get; set; } = null!;

    public virtual ICollection<Payment> Payments { get; } = new List<Payment>();
}
