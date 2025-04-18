using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PizzaShop.Entity.ViewModels.OrderAppVM;
using PizzaShop.Service.Implementaion;
using PizzaShop.Service.Interface;

namespace PizzaShop.Web.Controllers;

[Authorize(Roles = "Chef, Account Manager")]
public class OrderAppMenuController : Controller
{
    private readonly IMenuService _menuService;
    private readonly IOrderService _orderService;

    public OrderAppMenuController(IMenuService menuService, IOrderService orderService)
    {
        _menuService = menuService;
        _orderService = orderService;
    }

    // GET - MENU MODULE MAIN PAGE
    [HttpGet]
    public IActionResult Menu(int id)
    {
        return View();
    }

    // GET - CATEGORY SIDEBAR FOR MENU MODULE
    [HttpGet]
    public IActionResult GetCategorySidebar()
    {
        var categoryList = _menuService.CategoryList();

        return PartialView("_MenuSidebar", categoryList);
    }

    // GET - ORDER MENU PER ORDER ID
    [HttpGet]
    public async Task<IActionResult> GetOrderCard(int orderId)
    {
        var order = await _orderService.GetOrderAsync(orderId);
        return PartialView("_OrderMenu", order);
    }

    // GET - ITEM LIST AS PER CATEGORY
    [HttpGet]
    public IActionResult GetItemList(int categoryId)
    {
        var itemList = _menuService.GetMenuItem(categoryId);

        return PartialView("_MenuCardItem", itemList);
    }

    // GET - ITEM ORDER ITEM LIST
    public async Task<IActionResult> GetOrderItemListAsync(int orderId)
    {

        var orderItemList = await _orderService.GetOrderItemsAsync(orderId);
        return PartialView("_OrderItemList", orderItemList);
    }

    // GET - ITEM WISE MODIFIER
    public IActionResult GetItemWiseModifier(int itemId)
    {
        var itemModifierList = _orderService.GetItemWiseModifier(itemId);

        return PartialView("_AddModifier", itemModifierList);
    }

    [HttpPost]
    public async Task<IActionResult> AddItemToOrder([FromBody] AddItemVM addedItem)
    {
      var (status, message) = await _orderService.AddItemToOrder(addedItem);

      return Json(new {success= status, msg = message});
    }

    [HttpPost]
    public async Task<IActionResult> ToggleFavorite(int itemId)
    {
        var (status, message) = await _menuService.ToggleFavorite(itemId);
        return Json(new {success= status, msg = message});
    }
}
