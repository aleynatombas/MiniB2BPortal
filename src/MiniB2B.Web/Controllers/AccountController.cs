using System.Security.Claims;
using MiniB2B.Business.Dtos;
using MiniB2B.Business.Exceptions;
using MiniB2B.Business.Services;
using MiniB2B.Web.Infrastructure;
using MiniB2B.Web.Models;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace MiniB2B.Web.Controllers;

public class AccountController : Controller
{
    private readonly IAuthService _authService;

    public AccountController(IAuthService authService)
    {
        _authService = authService;
    }

    [HttpGet]
    [AllowAnonymous]
    public IActionResult Login(string? returnUrl = null, string? portal = null)
    {
        if (User.Identity?.IsAuthenticated == true)
            return RedirectForRole(User.IsAdmin() ? "Admin" : "Customer", returnUrl);

        return View(new LoginViewModel
        {
            ReturnUrl = returnUrl,
            Portal = ResolvePortal(portal, returnUrl)
        });
    }

    [HttpPost]
    [AllowAnonymous]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Login(LoginViewModel model, CancellationToken cancellationToken)
    {
        model.Portal = ResolvePortal(model.Portal, model.ReturnUrl);

        if (!ModelState.IsValid)
            return View(model);

        try
        {
            var user = await _authService.ValidateLoginAsync(new LoginDto
            {
                UsernameOrEmail = model.UsernameOrEmail,
                Password = model.Password
            }, cancellationToken);

            var isAdmin = string.Equals(user.Role, "Admin", StringComparison.OrdinalIgnoreCase);
            if (IsAdminPortal(model.Portal) && !isAdmin)
            {
                ModelState.AddModelError(string.Empty, "Bu giriş yönetim paneli içindir. Kullanıcı hesabıyla kullanıcı girişini kullanın.");
                return View(model);
            }

            if (IsCustomerPortal(model.Portal) && isAdmin)
            {
                ModelState.AddModelError(string.Empty, "Bu giriş kullanıcı vitrini içindir. Yönetici hesabıyla yönetici girişini kullanın.");
                return View(model);
            }

            await SignInAsync(user);
            return RedirectForRole(user.Role, model.ReturnUrl);
        }
        catch (BusinessException ex)
        {
            ModelState.AddModelError(string.Empty, ex.Message);
            return View(model);
        }
    }

    [HttpGet]
    [AllowAnonymous]
    public IActionResult Register()
    {
        if (User.Identity?.IsAuthenticated == true)
            return RedirectForRole(User.IsAdmin() ? "Admin" : "Customer");

        return View(new RegisterViewModel());
    }

    [HttpPost]
    [AllowAnonymous]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Register(RegisterViewModel model, CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
            return View(model);

        try
        {
            var user = await _authService.RegisterAsync(new RegisterDto
            {
                FirstName = model.FirstName,
                LastName = model.LastName,
                Email = model.Email,
                Phone = model.Phone,
                Username = model.Username,
                Password = model.Password
            }, cancellationToken);

            await SignInAsync(user);
            return RedirectForRole(user.Role);
        }
        catch (BusinessException ex)
        {
            ModelState.AddModelError(string.Empty, ex.Message);
            return View(model);
        }
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Logout()
    {
        await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
        return RedirectToAction(nameof(Login));
    }

    [AllowAnonymous]
    public IActionResult AccessDenied() => View();

    private IActionResult RedirectForRole(string role, string? returnUrl = null)
    {
        var isAdmin = string.Equals(role, "Admin", StringComparison.OrdinalIgnoreCase);
        if (isAdmin)
        {
            if (!string.IsNullOrWhiteSpace(returnUrl)
                && Url.IsLocalUrl(returnUrl)
                && returnUrl.StartsWith("/Admin", StringComparison.OrdinalIgnoreCase))
                return Redirect(returnUrl);

            return RedirectToAction("Index", "Dashboard", new { area = "Admin" });
        }

        if (!string.IsNullOrWhiteSpace(returnUrl)
            && Url.IsLocalUrl(returnUrl)
            && !returnUrl.StartsWith("/Admin", StringComparison.OrdinalIgnoreCase))
            return Redirect(returnUrl);

        return RedirectToAction("Index", "Home", new { area = "" });
    }

    private static string? ResolvePortal(string? portal, string? returnUrl)
    {
        if (string.Equals(portal, "admin", StringComparison.OrdinalIgnoreCase)
            || (!string.IsNullOrWhiteSpace(returnUrl)
                && returnUrl.StartsWith("/Admin", StringComparison.OrdinalIgnoreCase)))
            return "admin";

        if (string.Equals(portal, "customer", StringComparison.OrdinalIgnoreCase)
            || string.Equals(portal, "bayi", StringComparison.OrdinalIgnoreCase))
            return "customer";

        return null;
    }

    private static bool IsAdminPortal(string? portal)
        => string.Equals(portal, "admin", StringComparison.OrdinalIgnoreCase);

    private static bool IsCustomerPortal(string? portal)
        => string.Equals(portal, "customer", StringComparison.OrdinalIgnoreCase)
           || string.Equals(portal, "bayi", StringComparison.OrdinalIgnoreCase);

    private async Task SignInAsync(AuthUserDto user)
    {
        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new(ClaimTypes.Name, user.Username),
            new(ClaimTypes.Email, user.Email),
            new(ClaimTypes.Role, user.Role),
            new("FullName", user.FullName)
        };

        var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
        var principal = new ClaimsPrincipal(identity);
        await HttpContext.SignInAsync(
            CookieAuthenticationDefaults.AuthenticationScheme,
            principal,
            new AuthenticationProperties
            {
                IsPersistent = true,
                ExpiresUtc = DateTimeOffset.UtcNow.AddHours(8)
            });
    }

}
