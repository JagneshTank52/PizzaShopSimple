using System;
using System.Collections.Generic;

namespace PizzaShop.Entity.Models;

public partial class Wating
{
    public int WatingId { get; set; }

    public int CustomerId { get; set; }

    public int SectionId { get; set; }

    public int NoOfPerson { get; set; }

    public bool IsAssigned { get; set; }

    public bool IsDeleted { get; set; }

    public bool IsDeleated { get; set; }

    public DateTime CreatedAt { get; set; }

    public int CreatedBy { get; set; }

    public DateTime? ModifiedAt { get; set; }

    public int? ModifiedBy { get; set; }

    public virtual Customer Customer { get; set; } = null!;

    public virtual Section Section { get; set; } = null!;
}
