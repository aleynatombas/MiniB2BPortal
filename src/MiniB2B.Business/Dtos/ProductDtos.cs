namespace MiniB2B.Business.Dtos;

public class ProductDto
{
    public int Id { get; set; }
    public string ProductCode { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string? Brand { get; set; }
    public string? ManufacturerCode { get; set; }
    public string? CustomCode1 { get; set; }
    public string? CustomCode2 { get; set; }
    public string? ImagePath { get; set; }
    public int StockQuantity { get; set; }
    public int CriticalStockLevel { get; set; }
    public decimal Price { get; set; }
    public int CategoryId { get; set; }
    public string CategoryName { get; set; } = string.Empty;
    public bool IsActive { get; set; }
}

public class ProductFormDto
{
    public int Id { get; set; }
    public string ProductCode { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string? Brand { get; set; }
    public string? ManufacturerCode { get; set; }
    public string? CustomCode1 { get; set; }
    public string? CustomCode2 { get; set; }
    public string? ImagePath { get; set; }
    public int StockQuantity { get; set; }
    public int CriticalStockLevel { get; set; } = 5;
    public decimal Price { get; set; }
    public int CategoryId { get; set; }
    public bool IsActive { get; set; } = true;
}

public class CategoryDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
}
