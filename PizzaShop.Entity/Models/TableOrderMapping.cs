using System;
using System.Collections.Generic;

namespace PizzaShop.Entity.Models;

public partial class TableOrderMapping
{
    public int Id { get; set; }

    public int OrderId { get; set; }

    public int TableId { get; set; }

    public int NoOfPerson { get; set; }

    public bool IsDeleated { get; set; }

    public DateTime CreatedAt { get; set; }

    public int CreatedBy { get; set; }

    public DateTime? ModifiedAt { get; set; }

    public int? ModifiedBy { get; set; }
}
