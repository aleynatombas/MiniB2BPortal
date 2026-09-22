using MiniB2B.Business.Dtos;
using MiniB2B.Business.Exceptions;
using MiniB2B.DataAccess;
using MiniB2B.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace MiniB2B.Business.Services;

public class CartService : ICartService
{
    private readonly IUnitOfWork _unitOfWork;

    public CartService(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<CartDto> GetAsync(int userId, CancellationToken cancellationToken = default)
    {
        var cart = await _unitOfWork.Carts.Query()
            .AsNoTracking()
            .Include(c => c.Items)
            .ThenInclude(i => i.Product)
            .FirstOrDefaultAsync(c => c.UserId == userId, cancellationToken);

        if (cart is null)
            return new CartDto();

        return new CartDto
        {
            Items = cart.Items
                .OrderBy(i => i.Product.Name)
                .Select(i => new CartLineDto
                {
                    ItemId = i.Id,
                    ProductId = i.ProductId,
                    ProductCode = i.Product.ProductCode,
                    Name = i.Product.Name,
                    ImagePath = i.Product.ImagePath,
                    UnitPrice = i.Product.Price,
                    Quantity = i.Quantity,
                    StockQuantity = i.Product.StockQuantity,
                    IsActive = i.Product.IsActive
                })
                .ToList()
        };
    }

    public async Task AddItemAsync(int userId, int productId, int quantity, CancellationToken cancellationToken = default)
    {
        EnsureQuantity(quantity);

        var product = await _unitOfWork.Products.GetByIdAsync(productId, cancellationToken)
            ?? throw new BusinessException("Ürün bulunamadı.");

        var cart = await _unitOfWork.Carts.Query()
            .Include(c => c.Items)
            .FirstOrDefaultAsync(c => c.UserId == userId, cancellationToken);

        var currentQty = cart?.Items.FirstOrDefault(i => i.ProductId == productId)?.Quantity ?? 0;
        EnsureCanTake(product, currentQty + quantity);

        if (cart is null)
        {
            await _unitOfWork.Carts.AddAsync(new Cart
            {
                UserId = userId,
                CreatedAt = DateTime.UtcNow,
                Items = { new CartItem { ProductId = productId, Quantity = quantity } }
            }, cancellationToken);
        }
        else
        {
            var item = cart.Items.FirstOrDefault(i => i.ProductId == productId);
            if (item is null)
                cart.Items.Add(new CartItem { ProductId = productId, Quantity = quantity });
            else
                item.Quantity = currentQty + quantity;

            cart.UpdatedAt = DateTime.UtcNow;
        }

        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }

    public async Task UpdateItemAsync(int userId, int itemId, int quantity, CancellationToken cancellationToken = default)
    {
        EnsureQuantity(quantity);

        var item = await GetOwnedItemAsync(userId, itemId, cancellationToken);
        EnsureCanTake(item.Product, quantity);

        item.Quantity = quantity;
        item.Cart.UpdatedAt = DateTime.UtcNow;
        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }

    public async Task RemoveItemAsync(int userId, int itemId, CancellationToken cancellationToken = default)
    {
        var item = await GetOwnedItemAsync(userId, itemId, cancellationToken);
        item.Cart.UpdatedAt = DateTime.UtcNow;
        _unitOfWork.CartItems.Remove(item);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }

    private async Task<CartItem> GetOwnedItemAsync(int userId, int itemId, CancellationToken cancellationToken)
    {
        return await _unitOfWork.CartItems.Query()
            .Include(i => i.Cart)
            .Include(i => i.Product)
            .FirstOrDefaultAsync(i => i.Id == itemId && i.Cart.UserId == userId, cancellationToken)
            ?? throw new BusinessException("Sepet kalemi bulunamadı.");
    }

    private static void EnsureQuantity(int quantity)
    {
        if (quantity <= 0)
            throw new BusinessException("Adet 1 veya daha büyük olmalıdır.");
    }

    private static void EnsureCanTake(Product product, int quantity)
    {
        if (!product.IsActive)
            throw new BusinessException("Pasif ürün sepete eklenemez.");

        if (product.StockQuantity <= 0)
            throw new BusinessException($"{product.Name} için stok bulunmamaktadır.");

        if (quantity > product.StockQuantity)
            throw new BusinessException($"{product.Name} için yeterli stok bulunmamaktadır. Mevcut stok: {product.StockQuantity}.");
    }
}
