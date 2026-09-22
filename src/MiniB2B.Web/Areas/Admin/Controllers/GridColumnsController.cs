using MiniB2B.Business.Catalog;
using MiniB2B.Business.Dtos;
using MiniB2B.Business.Exceptions;
using MiniB2B.Business.Services;
using MiniB2B.Domain.Enums;
using MiniB2B.Web.Areas.Admin.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace MiniB2B.Web.Areas.Admin.Controllers;

public class GridColumnsController : AdminController
{
    private readonly IProductGridService _grid;

    public GridColumnsController(IProductGridService grid)
    {
        _grid = grid;
    }

    public async Task<IActionResult> Index(CancellationToken cancellationToken)
        => View(await _grid.GetAllAsync(cancellationToken));

    [HttpGet]
    public async Task<IActionResult> Edit(int id, CancellationToken cancellationToken)
    {
        try
        {
            var column = await _grid.GetByIdAsync(id, cancellationToken);
            FillLookups(column.FieldName);
            return View(ToViewModel(column));
        }
        catch (BusinessException ex)
        {
            TempData["Error"] = ex.Message;
            return RedirectToAction(nameof(Index));
        }
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, GridColumnFormViewModel model, CancellationToken cancellationToken)
    {
        model.Id = id;
        FillLookups(model.FieldName);
        if (!ModelState.IsValid)
            return View(model);

        try
        {
            await _grid.UpdateAsync(ToDto(model), cancellationToken);
            TempData["Success"] = "Grid kolonu güncellendi.";
            return RedirectToAction(nameof(Index));
        }
        catch (BusinessException ex)
        {
            ModelState.AddModelError(string.Empty, ex.Message);
            return View(model);
        }
    }

    private void FillLookups(string selectedField)
    {
        ViewBag.Fields = new SelectList(ProductFieldAccessor.AllowedFieldNames, selectedField);
        ViewBag.RenderTypes = new SelectList(Enum.GetValues<GridRenderType>());
        ViewBag.Alignments = new SelectList(Enum.GetValues<GridAlignment>());
    }

    private static ProductGridColumnDto ToDto(GridColumnFormViewModel model) => new()
    {
        Id = model.Id,
        FieldName = model.FieldName,
        Header = model.Header,
        RenderType = model.RenderType,
        SortOrder = model.SortOrder,
        Width = model.Width,
        Alignment = model.Alignment,
        ShowOnDesktop = model.ShowOnDesktop,
        ShowOnTablet = model.ShowOnTablet,
        ShowOnMobile = model.ShowOnMobile,
        IsVisible = model.IsVisible
    };

    private static GridColumnFormViewModel ToViewModel(ProductGridColumnDto column) => new()
    {
        Id = column.Id,
        FieldName = column.FieldName,
        Header = column.Header,
        RenderType = column.RenderType,
        SortOrder = column.SortOrder,
        Width = column.Width,
        Alignment = column.Alignment,
        ShowOnDesktop = column.ShowOnDesktop,
        ShowOnTablet = column.ShowOnTablet,
        ShowOnMobile = column.ShowOnMobile,
        IsVisible = column.IsVisible
    };
}
