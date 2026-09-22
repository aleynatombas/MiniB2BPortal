using MiniB2B.Business.Services;
using Microsoft.AspNetCore.Mvc;

namespace MiniB2B.Web.Areas.Admin.Controllers;

public class DashboardController : AdminController
{
    private readonly IProductService _products;
    private readonly IUserService _users;
    private readonly IOrderService _orders;

    public DashboardController(IProductService products, IUserService users, IOrderService orders)
    {
        _products = products;
        _users = users;
        _orders = orders;
    }

    public async Task<IActionResult> Index(CancellationToken cancellationToken)
    {
        ViewBag.ProductCount = await _products.CountAsync(cancellationToken);
        ViewBag.UserCount = await _users.CountAsync(cancellationToken);
        ViewBag.OrderCount = await _orders.CountAsync(cancellationToken);
        return View();
    }
}
