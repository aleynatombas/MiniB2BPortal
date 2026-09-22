using MiniB2B.Domain.Enums;

namespace MiniB2B.Web.Infrastructure;

public static class OrderDisplay
{
    public static string StatusText(OrderStatus status) => status switch
    {
        OrderStatus.Approved => "Onaylandı",
        OrderStatus.Rejected => "Reddedildi",
        _ => "Beklemede"
    };
}
