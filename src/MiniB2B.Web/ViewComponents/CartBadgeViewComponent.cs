using System.Security.Claims;
using MiniB2B.Business.Services;
using MiniB2B.Web.Infrastructure;
using Microsoft.AspNetCore.Mvc;

namespace MiniB2B.Web.ViewComponents;

public class CartBadgeViewComponent : ViewComponent
{
    private readonly ICartService _cart;

    public CartBadgeViewComponent(ICartService cart)
    {
        _cart = cart;
    }

    public async Task<IViewComponentResult> InvokeAsync()
    {
        var user = User as ClaimsPrincipal;
        var count = 0;
        if (user?.Identity?.IsAuthenticated == true && !user.IsAdmin())
            count = await _cart.GetItemCountAsync(user.GetUserId());

        return View(count);
    }
}
