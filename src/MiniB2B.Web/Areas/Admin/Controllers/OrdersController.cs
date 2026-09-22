using MiniB2B.Business.Exceptions;
using MiniB2B.Business.Services;
using MiniB2B.Domain.Enums;
using Microsoft.AspNetCore.Mvc;

namespace MiniB2B.Web.Areas.Admin.Controllers;

public class OrdersController : AdminController
{
    private readonly IOrderService _orders;

    public OrdersController(IOrderService orders)
    {
        _orders = orders;
    }

    public async Task<IActionResult> Index(CancellationToken cancellationToken)
    {
        var items = await _orders.GetAllAsync(cancellationToken);
        return View(items);
    }

    public async Task<IActionResult> Details(int id, CancellationToken cancellationToken)
    {
        try
        {
            var order = await _orders.GetByIdAsync(id, cancellationToken);
            return View(order);
        }
        catch (BusinessException)
        {
            return NotFound();
        }
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> SetStatus(int id, OrderStatus status, CancellationToken cancellationToken)
    {
        try
        {
            await _orders.UpdateStatusAsync(id, status, cancellationToken);
            TempData["Success"] = "Sipariş durumu güncellendi.";
        }
        catch (BusinessException ex)
        {
            TempData["Error"] = ex.Message;
        }

        return RedirectToAction(nameof(Details), new { id });
    }
}
