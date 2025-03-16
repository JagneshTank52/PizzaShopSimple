using System;
using System.Collections.Generic;

namespace PizzaShop.Entity.Models;

public partial class Invoice
{
    public int Id { get; set; }

    public int ModifierId { get; set; }

    public int ModifierQuantity { get; set; }

    public decimal? RateOfModifier { get; set; }

    public string? Phone { get; set; }

    public bool IsDeleated { get; set; }

    public DateTime CreatedAt { get; set; }

    public int CreatedBy { get; set; }

    public DateTime? ModifiedAt { get; set; }

    public int? ModifiedBy { get; set; }

    public virtual ICollection<Payment> Payments { get; } = new List<Payment>();
}
