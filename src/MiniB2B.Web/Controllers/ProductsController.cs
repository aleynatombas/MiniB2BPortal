using MiniB2B.Business.Exceptions;
using MiniB2B.Business.Services;
using MiniB2B.Web.Infrastructure;
using MiniB2B.Web.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace MiniB2B.Web.Controllers;

public class ProductsController : Controller
{
    private readonly IProductService _products;
    private readonly IProductGridService _grid;
    private readonly ICartService _cart;

    public ProductsController(IProductService products, IProductGridService grid, ICartService cart)
    {
        _products = products;
        _grid = grid;
        _cart = cart;
    }

    public async Task<IActionResult> Index(string? term, CancellationToken cancellationToken)
    {
        var model = new ProductCatalogViewModel
        {
            Term = term,
            Columns = await _grid.GetVisibleAsync(cancellationToken),
            Products = await _products.SearchAsync(term, activeOnly: true, cancellationToken)
        };
        return View(model);
    }

    [AllowAnonymous]
    public async Task<IActionResult> Details(int id, string? term, string? returnTo, bool partial = false, CancellationToken cancellationToken = default)
    {
        try
        {
            var product = await _products.GetByIdAsync(id, cancellationToken);
            if (!product.IsActive)
                return NotFound();

            ViewBag.Term = term;
            ViewBag.ReturnTo = returnTo ?? (WantsPartial(partial) ? null : "details");
            ViewData["Title"] = product.Name;

            if (WantsPartial(partial))
                return PartialView("_Details", product);

            return View(product);
        }
        catch (BusinessException)
        {
            return NotFound();
        }
    }

    [HttpPost]
    [Authorize(Roles = "Customer")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> AddToCart(int productId, int quantity = 1, string? term = null, string? returnTo = null, CancellationToken cancellationToken = default)
    {
        try
        {
            await _cart.AddItemAsync(User.GetUserId(), productId, quantity, cancellationToken);
            TempData["Status"] = "Ürün sepete eklendi.";
        }
        catch (BusinessException ex)
        {
            TempData["Error"] = ex.Message;
        }

        if (string.Equals(returnTo, "home", StringComparison.OrdinalIgnoreCase))
            return RedirectToAction("Index", "Home");
        if (string.Equals(returnTo, "details", StringComparison.OrdinalIgnoreCase))
            return RedirectToAction(nameof(Details), new { id = productId });

        return RedirectToAction(nameof(Index), new { term });
    }

    private bool WantsPartial(bool partial)
    {
        if (partial)
            return true;

        var requestedWith = Request.Headers["X-Requested-With"].ToString();
        return string.Equals(requestedWith, "XMLHttpRequest", StringComparison.OrdinalIgnoreCase);
    }
}
