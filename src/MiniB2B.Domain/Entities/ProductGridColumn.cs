using MiniB2B.Domain.Enums;

namespace MiniB2B.Domain.Entities;

/// <summary>
/// Ürün listesi grid kolonlarının veritabanı üzerinden yapılandırılması.
/// FieldName, Product üzerindeki property adı veya özel bir anahtar (Quantity, AddToCart) olabilir.
/// </summary>
public class ProductGridColumn
{
    public int Id { get; set; }
    public string FieldName { get; set; } = string.Empty;
    public string Header { get; set; } = string.Empty;
    public GridRenderType RenderType { get; set; }
    public int SortOrder { get; set; }
    public string? Width { get; set; }
    public GridAlignment Alignment { get; set; } = GridAlignment.Left;
    public bool ShowOnDesktop { get; set; } = true;
    public bool ShowOnTablet { get; set; } = true;
    public bool ShowOnMobile { get; set; } = true;
    public bool IsVisible { get; set; } = true;
}
