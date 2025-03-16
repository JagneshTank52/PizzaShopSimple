using System;
using System.Collections.Generic;

namespace PizzaShop.Entity.Models;

public partial class Permission
{
    public int Id { get; set; }

    public string Name { get; set; } = null!;

    public bool IsDeleated { get; set; }

    public DateTime CreatedAt { get; set; }

    public int CreatedBy { get; set; }

    public DateTime? ModifiedAt { get; set; }

    public int? ModifiedBy { get; set; }

    public virtual ICollection<RoleWisePermission> RoleWisePermissions { get; } = new List<RoleWisePermission>();
}
