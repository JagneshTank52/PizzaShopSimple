using System;
using System.Collections.Generic;

namespace PizzaShop.Entity.Models;

public partial class User
{
    public int Id { get; set; }

    public string FirstName { get; set; } = null!;

    public string? LastName { get; set; }

    public string UserName { get; set; } = null!;

    public string Email { get; set; } = null!;

    public string Password { get; set; } = null!;

    public string? ProfileImage { get; set; }

    public string PhoneNumber { get; set; } = null!;

    public int CountryId { get; set; }

    public int StateId { get; set; }

    public int CityId { get; set; }

    public string Address { get; set; } = null!;

    public bool? IsFirstTime { get; set; }

    public string ZipCode { get; set; } = null!;

    public int UserRoleId { get; set; }

    public bool IsDeleated { get; set; }

    public DateTime? CreatedAt { get; set; }

    public int? CreatedBy { get; set; }

    public DateTime? ModifiedAt { get; set; }

    public int? ModifiedBy { get; set; }

    public bool? IsActive { get; set; }

    public virtual City City { get; set; } = null!;

    public virtual Country Country { get; set; } = null!;

    public virtual User? CreatedByNavigation { get; set; }

    public virtual ICollection<User> InverseCreatedByNavigation { get; } = new List<User>();

    public virtual ICollection<User> InverseModifiedByNavigation { get; } = new List<User>();

    public virtual User? ModifiedByNavigation { get; set; }

    public virtual ICollection<Modifier> ModifierCreatedByNavigations { get; } = new List<Modifier>();

    public virtual ICollection<Modifier> ModifierModifiedByNavigations { get; } = new List<Modifier>();

    public virtual ICollection<Order> OrderCreatedByNavigations { get; } = new List<Order>();

    public virtual ICollection<Order> OrderModifiedByNavigations { get; } = new List<Order>();

    public virtual State State { get; set; } = null!;

    public virtual UserRole UserRole { get; set; } = null!;
}
