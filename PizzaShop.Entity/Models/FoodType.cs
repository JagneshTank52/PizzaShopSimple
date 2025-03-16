using System;
using System.Collections.Generic;

namespace PizzaShop.Entity.Models;

public partial class FoodType
{
    public int Id { get; set; }

    public string Name { get; set; } = null!;

    public virtual ICollection<Item> Items { get; } = new List<Item>();
}
