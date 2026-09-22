namespace MiniB2B.Business.Dtos;

public class CartDto
{
    public List<CartLineDto> Items { get; set; } = [];
    public decimal Total => Items.Sum(i => i.LineTotal);
}

public class CartLineDto
{
    public int ItemId { get; set; }
    public int ProductId { get; set; }
    public string ProductCode { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string? ImagePath { get; set; }
    public decimal UnitPrice { get; set; }
    public int Quantity { get; set; }
    public int StockQuantity { get; set; }
    public bool IsActive { get; set; }
    public decimal LineTotal => UnitPrice * Quantity;
    public bool ExceedsStock => Quantity > StockQuantity || !IsActive;
}
