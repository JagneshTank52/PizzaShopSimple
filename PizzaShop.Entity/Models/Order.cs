using System;
using System.Collections.Generic;

namespace PizzaShop.Entity.Models;

public partial class Order
{
    public int Id { get; set; }

    public bool Status { get; set; }

    public string? OrderInstruction { get; set; }

    public DateOnly Date { get; set; }

    public int Rating { get; set; }

    public int PymentMode { get; set; }

    public int CustomerId { get; set; }

    public decimal TotalAmount { get; set; }

    public decimal? SubAmount { get; set; }

    public decimal? TaxAmount { get; set; }

    public decimal PaidAmount { get; set; }

    public bool? IsAvaiable { get; set; }

    public bool IsDeleated { get; set; }

    public DateTime CreatedAt { get; set; }

    public int CreatedBy { get; set; }

    public DateTime? ModifiedAt { get; set; }

    public int? ModifiedBy { get; set; }

    public virtual User CreatedByNavigation { get; set; } = null!;

    public virtual Customer Customer { get; set; } = null!;

    public virtual User? ModifiedByNavigation { get; set; }

    public virtual PaymentMode PymentModeNavigation { get; set; } = null!;
}
