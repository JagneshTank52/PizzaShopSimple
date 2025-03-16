using System;
using System.Collections.Generic;

namespace PizzaShop.Entity.Models;

public partial class ItemsModifier
{
    public int Id { get; set; }

    public int ItemId { get; set; }

    public int ModifierGorupId { get; set; }

    public bool IsDeleated { get; set; }

    public DateTime? DeletedAt { get; set; }

    public int? DeletedBy { get; set; }

    public DateTime CreatedAt { get; set; }

    public int CreatedBy { get; set; }

    public DateTime? ModifiedAt { get; set; }

    public int? ModifiedBy { get; set; }

    public virtual Item Item { get; set; } = null!;
}
