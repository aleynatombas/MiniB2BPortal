namespace MiniB2B.Domain.Enums;

/// <summary>
/// Ürün grid kolonunun nasıl çizileceğini belirler.
/// Değerler ProductGridColumns tablosundan okunur; kod değişikliği gerekmez.
/// </summary>
public enum GridRenderType
{
    Text = 0,
    Image = 1,
    StockBadge = 2,
    Currency = 3,
    QuantityInput = 4,
    AddToCartButton = 5
}
