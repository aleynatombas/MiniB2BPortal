using MiniB2B.Business.Dtos;
using MiniB2B.Business.Exceptions;
using MiniB2B.Business.Services;
using MiniB2B.Web.Areas.Admin.Models;
using MiniB2B.Web.Infrastructure;
using Microsoft.AspNetCore.Mvc;

namespace MiniB2B.Web.Areas.Admin.Controllers;

public class SlidersController : AdminController
{
    private readonly ISliderService _sliders;
    private readonly IWebHostEnvironment _env;

    public SlidersController(ISliderService sliders, IWebHostEnvironment env)
    {
        _sliders = sliders;
        _env = env;
    }

    public async Task<IActionResult> Index(CancellationToken cancellationToken)
        => View(await _sliders.GetAllAsync(cancellationToken));

    [HttpGet]
    public IActionResult Create() => View(new SliderFormViewModel());

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(SliderFormViewModel model, CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
            return View(model);

        try
        {
            var dto = ToDto(model);
            if (model.ImageFile is { Length: > 0 })
                dto.ImagePath = await ImageStorage.SaveAsync(model.ImageFile, _env.WebRootPath, "sliders");

            await _sliders.CreateAsync(dto, cancellationToken);
            TempData["Success"] = "Slider eklendi.";
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
            return View(ToViewModel(await _sliders.GetByIdAsync(id, cancellationToken)));
        }
        catch (BusinessException ex)
        {
            TempData["Error"] = ex.Message;
            return RedirectToAction(nameof(Index));
        }
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, SliderFormViewModel model, CancellationToken cancellationToken)
    {
        model.Id = id;
        if (!ModelState.IsValid)
            return View(model);

        try
        {
            var dto = ToDto(model);
            if (model.ImageFile is { Length: > 0 })
                dto.ImagePath = await ImageStorage.SaveAsync(model.ImageFile, _env.WebRootPath, "sliders");

            await _sliders.UpdateAsync(dto, cancellationToken);
            TempData["Success"] = "Slider güncellendi.";
            return RedirectToAction(nameof(Index));
        }
        catch (BusinessException ex)
        {
            ModelState.AddModelError(string.Empty, ex.Message);
            return View(model);
        }
    }

    private static SliderDto ToDto(SliderFormViewModel model) => new()
    {
        Id = model.Id,
        Title = model.Title,
        Subtitle = model.Subtitle,
        ImagePath = model.ImagePath ?? string.Empty,
        LinkUrl = model.LinkUrl,
        DisplayOrder = model.DisplayOrder,
        IsActive = model.IsActive
    };

    private static SliderFormViewModel ToViewModel(SliderDto slider) => new()
    {
        Id = slider.Id,
        Title = slider.Title,
        Subtitle = slider.Subtitle,
        ImagePath = slider.ImagePath,
        LinkUrl = slider.LinkUrl,
        DisplayOrder = slider.DisplayOrder,
        IsActive = slider.IsActive
    };
}
