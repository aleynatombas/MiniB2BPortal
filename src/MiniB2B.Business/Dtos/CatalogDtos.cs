using MiniB2B.Domain.Enums;

namespace MiniB2B.Business.Dtos;

public class ProductGridColumnDto
{
    public int Id { get; set; }
    public string FieldName { get; set; } = string.Empty;
    public string Header { get; set; } = string.Empty;
    public GridRenderType RenderType { get; set; }
    public int SortOrder { get; set; }
    public string? Width { get; set; }
    public GridAlignment Alignment { get; set; }
    public bool ShowOnDesktop { get; set; }
    public bool ShowOnTablet { get; set; }
    public bool ShowOnMobile { get; set; }
    public bool IsVisible { get; set; }
}

public class SliderDto
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? Subtitle { get; set; }
    public string ImagePath { get; set; } = string.Empty;
    public string? LinkUrl { get; set; }
    public int DisplayOrder { get; set; }
    public bool IsActive { get; set; }
}
