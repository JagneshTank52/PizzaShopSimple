using System;
using System.Collections.Generic;

namespace PizzaShop.Entity.Models;

public partial class OrdersTax
{
    public int Id { get; set; }

    public int? OrderId { get; set; }

    public int? TaxId { get; set; }

    public decimal TaxAmount { get; set; }

    public virtual TaxAndFee? Tax { get; set; }
}
