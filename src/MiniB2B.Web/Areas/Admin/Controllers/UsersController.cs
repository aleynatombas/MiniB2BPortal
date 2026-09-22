using MiniB2B.Business.Dtos;
using MiniB2B.Business.Exceptions;
using MiniB2B.Business.Services;
using MiniB2B.Web.Areas.Admin.Models;
using Microsoft.AspNetCore.Mvc;

namespace MiniB2B.Web.Areas.Admin.Controllers;

public class UsersController : AdminController
{
    private readonly IUserService _users;

    public UsersController(IUserService users)
    {
        _users = users;
    }

    public async Task<IActionResult> Index(string? q, CancellationToken cancellationToken)
    {
        ViewBag.Query = q;
        var items = await _users.GetAllAsync(q, cancellationToken);
        return View(items);
    }

    [HttpGet]
    public async Task<IActionResult> Edit(int id, CancellationToken cancellationToken)
    {
        var user = await _users.GetByIdAsync(id, cancellationToken);
        return View(new UserFormViewModel
        {
            Id = user.Id,
            FirstName = user.FirstName,
            LastName = user.LastName,
            Email = user.Email,
            Phone = user.Phone,
            Username = user.Username,
            Role = user.Role,
            IsActive = user.IsActive
        });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, UserFormViewModel model, CancellationToken cancellationToken)
    {
        model.Id = id;
        if (!ModelState.IsValid)
            return View(model);

        try
        {
            await _users.UpdateAsync(new UserFormDto
            {
                Id = id,
                FirstName = model.FirstName,
                LastName = model.LastName,
                Email = model.Email,
                Phone = model.Phone,
                Username = model.Username,
                Password = model.Password,
                Role = model.Role,
                IsActive = model.IsActive
            }, cancellationToken);

            TempData["Success"] = "Kullanıcı güncellendi.";
            return RedirectToAction(nameof(Index));
        }
        catch (BusinessException ex)
        {
            ModelState.AddModelError(string.Empty, ex.Message);
            return View(model);
        }
    }
}
