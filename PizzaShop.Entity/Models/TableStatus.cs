using System;
using System.Collections.Generic;

namespace PizzaShop.Entity.Models;

public partial class TableStatus
{
    public int Id { get; set; }

    public string Name { get; set; } = null!;

    public virtual ICollection<Table> Tables { get; } = new List<Table>();
}
