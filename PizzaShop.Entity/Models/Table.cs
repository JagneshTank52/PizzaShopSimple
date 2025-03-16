using System;
using System.Collections.Generic;

namespace PizzaShop.Entity.Models;

public partial class Table
{
    public int Id { get; set; }

    public string Name { get; set; } = null!;

    public int? SectionId { get; set; }

    public short Capacity { get; set; }

    public int? Status { get; set; }

    public bool? IsAvaiable { get; set; }

    public bool IsDeleated { get; set; }

    public DateTime CreatedAt { get; set; }

    public int CreatedBy { get; set; }

    public DateTime? ModifiedAt { get; set; }

    public int? ModifiedBy { get; set; }

    public virtual Section? Section { get; set; }

    public virtual TableStatus? StatusNavigation { get; set; }
}
