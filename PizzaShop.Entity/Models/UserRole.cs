using System;
using System.Collections.Generic;

namespace PizzaShop.Entity.Models;

public partial class UserRole
{
    public int Id { get; set; }

    public string Name { get; set; } = null!;

    public virtual ICollection<RoleWisePermission> RoleWisePermissions { get; } = new List<RoleWisePermission>();

    public virtual ICollection<User> Users { get; } = new List<User>();
}
