using System;
using System.Collections.Generic;

namespace PizzaShop.Entity.Models;

public partial class RoleWisePermission
{
    public int Id { get; set; }

    public int RoleId { get; set; }

    public int PermissionId { get; set; }

    public bool CanView { get; set; }

    public bool CanEdit { get; set; }

    public bool CanDelete { get; set; }

    public bool IsDeleated { get; set; }

    public DateTime CreatedAt { get; set; }

    public int CreatedBy { get; set; }

    public DateTime? ModifiedAt { get; set; }

    public int? ModifiedBy { get; set; }

    public virtual Permission Permission { get; set; } = null!;

    public virtual UserRole Role { get; set; } = null!;
}
