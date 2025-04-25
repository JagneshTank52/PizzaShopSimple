namespace PizzaShop.Entity.ViewModels.OrderAppVM;

public class OrderTaxVM
{
    public decimal SubTotal {get; set;}

    public List<DefaultTaxVM> DefaultTaxList {get;set;} = new List<DefaultTaxVM>();
    public List<DefaultTaxVM> OptionalTaxList {get;set;} = new List<DefaultTaxVM>();

    public decimal TotalAmount {get;set;}



    
}
