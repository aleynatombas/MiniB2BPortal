using System.Diagnostics;
using MiniB2B.Business.Services;
using MiniB2B.Web.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace MiniB2B.Web.Controllers;

[AllowAnonymous]
public class HomeController : Controller
{
    private readonly ISliderService _sliders;
    private readonly ICategoryService _categories;
    private readonly IProductService _products;

    public HomeController(ISliderService sliders, ICategoryService categories, IProductService products)
    {
        _sliders = sliders;
        _categories = categories;
        _products = products;
    }

    public async Task<IActionResult> Index(CancellationToken cancellationToken)
    {
        return View(new HomeViewModel
        {
            Sliders = await _sliders.GetActiveAsync(cancellationToken),
            Categories = await _categories.GetAllAsync(cancellationToken),
            Featured = await _products.GetFeaturedAsync(8, cancellationToken)
        });
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}
