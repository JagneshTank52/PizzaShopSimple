using System;
using System.Collections.Generic;

namespace PizzaShop.Entity.Models;

public partial class Payment
{
    public int Id { get; set; }

    public int PaymentMethodId { get; set; }

    public int PaymentStatusId { get; set; }

    public decimal TotalAmount { get; set; }

    public int InvoiceId { get; set; }

    public virtual Invoice Invoice { get; set; } = null!;

    public virtual PaymentMethod PaymentMethod { get; set; } = null!;

    public virtual PaymentStatus PaymentStatus { get; set; } = null!;
}
