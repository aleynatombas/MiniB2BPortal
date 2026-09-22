using MiniB2B.Business.Dtos;
using MiniB2B.Business.Exceptions;
using MiniB2B.Business.Services;
using MiniB2B.Web.Infrastructure;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace MiniB2B.Web.Controllers;

[Authorize(Roles = "Customer")]
public class OrdersController : Controller
{
    private readonly IOrderService _orders;

    public OrdersController(IOrderService orders)
    {
        _orders = orders;
    }

    public async Task<IActionResult> Index(CancellationToken cancellationToken)
    {
        var items = await _orders.GetByUserAsync(User.GetUserId(), cancellationToken);
        return View(items);
    }

    public async Task<IActionResult> Details(int id, CancellationToken cancellationToken)
    {
        try
        {
            var order = await _orders.GetByIdAsync(User.GetUserId(), id, cancellationToken);
            return View(order);
        }
        catch (BusinessException)
        {
            return NotFound();
        }
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(CancellationToken cancellationToken)
    {
        try
        {
            var id = await _orders.PlaceOrderAsync(User.GetUserId(), cancellationToken);
            TempData["Status"] = "Sipariş oluşturuldu.";
            return RedirectToAction(nameof(Details), new { id });
        }
        catch (BusinessException ex)
        {
            TempData["Error"] = ex.Message;
            return RedirectToAction("Index", "Cart");
        }
    }
}
