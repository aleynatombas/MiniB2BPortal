using MiniB2B.Business.Dtos;
using MiniB2B.Business.Exceptions;
using MiniB2B.Business.Services;
using MiniB2B.Web.Areas.Admin.Models;
using MiniB2B.Web.Infrastructure;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace MiniB2B.Web.Areas.Admin.Controllers;

public class ProductsController : AdminController
{
    private readonly IProductService _products;
    private readonly ICategoryService _categories;
    private readonly IWebHostEnvironment _env;

    public ProductsController(IProductService products, ICategoryService categories, IWebHostEnvironment env)
    {
        _products = products;
        _categories = categories;
        _env = env;
    }

    public async Task<IActionResult> Index(string? q, CancellationToken cancellationToken)
    {
        ViewBag.Query = q;
        var items = await _products.SearchAsync(q, activeOnly: false, cancellationToken);
        return View(items);
    }

    [HttpGet]
    public async Task<IActionResult> Create(CancellationToken cancellationToken)
    {
        await FillCategories(0, cancellationToken);
        return View(new ProductFormViewModel());
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(ProductFormViewModel model, CancellationToken cancellationToken)
    {
        await FillCategories(model.CategoryId, cancellationToken);
        if (!ModelState.IsValid)
            return View(model);

        try
        {
            var dto = ToDto(model);
            if (model.ImageFile is { Length: > 0 })
                dto.ImagePath = await ImageStorage.SaveAsync(model.ImageFile, _env.WebRootPath, "products");

            await _products.CreateAsync(dto, cancellationToken);
            TempData["Success"] = "Ürün eklendi.";
            return RedirectToAction(nameof(Index));
        }
        catch (BusinessException ex)
        {
            ModelState.AddModelError(string.Empty, ex.Message);
            return View(model);
        }
    }

    [HttpGet]
    public async Task<IActionResult> Edit(int id, CancellationToken cancellationToken)
    {
        try
        {
            var product = await _products.GetByIdAsync(id, cancellationToken);
            await FillCategories(product.CategoryId, cancellationToken);
            return View(ToViewModel(product));
        }
        catch (BusinessException ex)
        {
            TempData["Error"] = ex.Message;
            return RedirectToAction(nameof(Index));
        }
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, ProductFormViewModel model, CancellationToken cancellationToken)
    {
        model.Id = id;
        await FillCategories(model.CategoryId, cancellationToken);
        if (!ModelState.IsValid)
            return View(model);

        try
        {
            var dto = ToDto(model);
            if (model.ImageFile is { Length: > 0 })
                dto.ImagePath = await ImageStorage.SaveAsync(model.ImageFile, _env.WebRootPath, "products");

            await _products.UpdateAsync(dto, cancellationToken);
            TempData["Success"] = "Ürün güncellendi.";
            return RedirectToAction(nameof(Index));
        }
        catch (BusinessException ex)
        {
            ModelState.AddModelError(string.Empty, ex.Message);
            return View(model);
        }
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> SetActive(int id, bool isActive, CancellationToken cancellationToken)
    {
        try
        {
            await _products.SetActiveAsync(id, isActive, cancellationToken);
            TempData["Success"] = isActive ? "Ürün yeniden yayınlandı." : "Ürün pasifleştirildi. Katalogda görünmez; eski siparişler durur.";
        }
        catch (BusinessException ex)
        {
            TempData["Error"] = ex.Message;
        }

        return RedirectToAction(nameof(Index));
    }

    private async Task FillCategories(int selectedId, CancellationToken cancellationToken)
    {
        var categories = await _categories.GetAllAsync(cancellationToken);
        ViewBag.Categories = new SelectList(categories, "Id", "Name", selectedId);
    }

    private static ProductFormDto ToDto(ProductFormViewModel model) => new()
    {
        Id = model.Id,
        ProductCode = model.ProductCode,
        Name = model.Name,
        Description = model.Description,
        Brand = model.Brand,
        ManufacturerCode = model.ManufacturerCode,
        CustomCode1 = model.CustomCode1,
        CustomCode2 = model.CustomCode2,
        ImagePath = model.ImagePath,
        StockQuantity = model.StockQuantity,
        CriticalStockLevel = model.CriticalStockLevel,
        Price = model.Price,
        CategoryId = model.CategoryId,
        IsActive = model.IsActive
    };

    private static ProductFormViewModel ToViewModel(ProductDto product) => new()
    {
        Id = product.Id,
        ProductCode = product.ProductCode,
        Name = product.Name,
        Description = product.Description,
        Brand = product.Brand,
        ManufacturerCode = product.ManufacturerCode,
        CustomCode1 = product.CustomCode1,
        CustomCode2 = product.CustomCode2,
        ImagePath = product.ImagePath,
        StockQuantity = product.StockQuantity,
        CriticalStockLevel = product.CriticalStockLevel,
        Price = product.Price,
        CategoryId = product.CategoryId,
        IsActive = product.IsActive
    };
}
