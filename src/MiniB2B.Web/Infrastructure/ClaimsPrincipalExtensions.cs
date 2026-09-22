using System.Security.Claims;

namespace MiniB2B.Web.Infrastructure;

public static class ClaimsPrincipalExtensions
{
    public static int GetUserId(this ClaimsPrincipal user)
    {
        var value = user.FindFirstValue(ClaimTypes.NameIdentifier);
        return int.TryParse(value, out var id) ? id : 0;
    }

    public static string GetFullName(this ClaimsPrincipal user)
        => user.FindFirstValue("FullName") ?? user.Identity?.Name ?? string.Empty;

    public static bool IsAdmin(this ClaimsPrincipal user)
        => user.IsInRole("Admin");
}
