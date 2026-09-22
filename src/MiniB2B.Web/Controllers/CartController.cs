using MiniB2B.Business.Exceptions;
using MiniB2B.Business.Services;
using MiniB2B.Web.Infrastructure;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace MiniB2B.Web.Controllers;

[Authorize(Roles = "Customer")]
public class CartController : Controller
{
    private readonly ICartService _cart;

    public CartController(ICartService cart)
    {
        _cart = cart;
    }

    public async Task<IActionResult> Index(CancellationToken cancellationToken)
    {
        var cart = await _cart.GetAsync(User.GetUserId(), cancellationToken);
        return View(cart);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Update(int itemId, int quantity, CancellationToken cancellationToken)
    {
        try
        {
            await _cart.UpdateItemAsync(User.GetUserId(), itemId, quantity, cancellationToken);
            TempData["Status"] = "Sepet güncellendi.";
        }
        catch (BusinessException ex)
        {
            TempData["Error"] = ex.Message;
        }

        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Remove(int itemId, CancellationToken cancellationToken)
    {
        try
        {
            await _cart.RemoveItemAsync(User.GetUserId(), itemId, cancellationToken);
            TempData["Status"] = "Ürün sepetten çıkarıldı.";
        }
        catch (BusinessException ex)
        {
            TempData["Error"] = ex.Message;
        }

        return RedirectToAction(nameof(Index));
    }
}
