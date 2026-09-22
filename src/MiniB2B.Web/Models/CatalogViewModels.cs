using MiniB2B.Business.Dtos;

namespace MiniB2B.Web.Models;

public class HomeViewModel
{
    public IReadOnlyList<SliderDto> Sliders { get; set; } = [];
    public IReadOnlyList<CategoryDto> Categories { get; set; } = [];
    public IReadOnlyList<ProductDto> Featured { get; set; } = [];
}

public class ProductCatalogViewModel
{
    public string? Term { get; set; }
    public IReadOnlyList<ProductGridColumnDto> Columns { get; set; } = [];
    public IReadOnlyList<ProductDto> Products { get; set; } = [];
}

public class ProductGridCellModel
{
    public ProductGridColumnDto Column { get; set; } = null!;
    public ProductDto Product { get; set; } = null!;
    public string? Term { get; set; }
}
