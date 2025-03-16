using System;
using System.Collections.Generic;

namespace PizzaShop.Entity.Models;

public partial class Section
{
    public int Id { get; set; }

    public string Name { get; set; } = null!;

    public string? Description { get; set; }

    public bool IsDeleated { get; set; }

    public DateTime? CreatedAt { get; set; }

    public int? CreatedBy { get; set; }

    public DateTime? ModifiedAt { get; set; }

    public int? ModifiedBy { get; set; }

    public virtual ICollection<Table> Tables { get; } = new List<Table>();

    public virtual ICollection<Wating> Watings { get; } = new List<Wating>();
}
