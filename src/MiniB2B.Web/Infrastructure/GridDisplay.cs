using MiniB2B.Business.Dtos;
using MiniB2B.Domain.Enums;

namespace MiniB2B.Web.Infrastructure;

public static class GridDisplay
{
    public static string AlignClass(GridAlignment alignment) => alignment switch
    {
        GridAlignment.Center => "text-center",
        GridAlignment.Right => "text-end",
        _ => "text-start"
    };

    public static string VisibilityClass(ProductGridColumnDto column)
    {
        if (column.ShowOnMobile)
            return string.Empty;
        if (column.ShowOnTablet)
            return "d-none d-md-table-cell";
        if (column.ShowOnDesktop)
            return "d-none d-lg-table-cell";
        return "d-none";
    }
}
