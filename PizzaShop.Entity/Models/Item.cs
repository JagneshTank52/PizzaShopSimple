using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PizzaShop.Entity.Models;

public partial class Item
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int Id { get; set; }

    public string Name { get; set; } = null!;

    public int ItemType { get; set; }

    public decimal UnitPrice { get; set; }

    public int Quantity { get; set; }

    public int Unit { get; set; }

    public bool? DefaultTax { get; set; }

    public string? Description { get; set; }

    public decimal? TextPercentage { get; set; }

    public bool IsFavorite { get; set; }

    public string? ItemImage { get; set; }

    public string ShortCode { get; set; } = null!;

    public int CategoryId { get; set; }

    public bool IsDeleated { get; set; }

    public DateTime CreatedAt { get; set; }

    public int CreatedBy { get; set; }

    public DateTime? ModifiedAt { get; set; }

    public int? ModifiedBy { get; set; }

    public bool? IsAvaiable { get; set; }

    public virtual Category Category { get; set; } = null!;

    public virtual FoodType ItemTypeNavigation { get; set; } = null!;

    public virtual ICollection<ItemsModifier> ItemsModifiers { get; } = new List<ItemsModifier>();

    public virtual ICollection<OrderItem> OrderItemItemNameNavigations { get; } = new List<OrderItem>();

    public virtual ICollection<OrderItem> OrderItemItems { get; } = new List<OrderItem>();

    public virtual MeasuringUnit UnitNavigation { get; set; } = null!;
}
