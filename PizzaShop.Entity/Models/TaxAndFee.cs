using System;
using System.Collections.Generic;

namespace PizzaShop.Entity.Models;

public partial class TaxAndFee
{
    public int Id { get; set; }

    public string Name { get; set; } = null!;

    public int TaxType { get; set; }

    public decimal TaxAmount { get; set; }

    public bool? IsEnabled { get; set; }

    public bool IsDefault { get; set; }

    public bool IsDeleated { get; set; }

    public DateTime CreatedAt { get; set; }

    public int CreatedBy { get; set; }

    public DateTime? ModifiedAt { get; set; }

    public int? ModifiedBy { get; set; }

    public virtual ICollection<OrdersTax> OrdersTaxes { get; } = new List<OrdersTax>();
}
