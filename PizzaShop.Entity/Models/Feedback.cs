using System;
using System.Collections.Generic;

namespace PizzaShop.Entity.Models;

public partial class Feedback
{
    public int Id { get; set; }

    public int OrderId { get; set; }

    public int FoodRating { get; set; }

    public int ServiceRating { get; set; }

    public int AmbienceRating { get; set; }

    public string? Comment { get; set; }
}
