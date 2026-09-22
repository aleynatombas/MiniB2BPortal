using MiniB2B.Business.Dtos;
using MiniB2B.Business.Exceptions;
using MiniB2B.DataAccess;
using MiniB2B.Domain.Entities;
using MiniB2B.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace MiniB2B.Business.Services;

public class OrderService : IOrderService
{
    private readonly IUnitOfWork _unitOfWork;

    public OrderService(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public Task<int> PlaceOrderAsync(int userId, CancellationToken cancellationToken = default)
        => _unitOfWork.ExecuteInTransactionAsync(ct => PlaceOrderCoreAsync(userId, ct), cancellationToken);

    public async Task<List<OrderSummaryDto>> GetByUserAsync(int userId, CancellationToken cancellationToken = default)
    {
        return await _unitOfWork.Orders.Query()
            .AsNoTracking()
            .Where(o => o.UserId == userId)
            .OrderByDescending(o => o.OrderDate)
            .Select(o => new OrderSummaryDto
            {
                Id = o.Id,
                OrderNumber = o.OrderNumber,
                OrderDate = o.OrderDate,
                Status = o.Status,
                TotalAmount = o.TotalAmount
            })
            .ToListAsync(cancellationToken);
    }

    public async Task<List<OrderSummaryDto>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return await _unitOfWork.Orders.Query()
            .AsNoTracking()
            .OrderByDescending(o => o.OrderDate)
            .Select(o => new OrderSummaryDto
            {
                Id = o.Id,
                OrderNumber = o.OrderNumber,
                OrderDate = o.OrderDate,
                Status = o.Status,
                TotalAmount = o.TotalAmount,
                CustomerName = o.User.FirstName + " " + o.User.LastName
            })
            .ToListAsync(cancellationToken);
    }

    public Task<int> CountAsync(CancellationToken cancellationToken = default)
        => _unitOfWork.Orders.Query().CountAsync(cancellationToken);

    public async Task<OrderDetailDto> GetByIdAsync(int userId, int orderId, CancellationToken cancellationToken = default)
    {
        var order = await LoadOrderAsync(o => o.Id == orderId && o.UserId == userId, includeUser: false, cancellationToken);
        return MapDetail(order);
    }

    public async Task<OrderDetailDto> GetByIdAsync(int orderId, CancellationToken cancellationToken = default)
    {
        var order = await LoadOrderAsync(o => o.Id == orderId, includeUser: true, cancellationToken);
        return MapDetail(order);
    }

    public async Task UpdateStatusAsync(int orderId, OrderStatus status, CancellationToken cancellationToken = default)
    {
        if (status is not (OrderStatus.Approved or OrderStatus.Rejected))
            throw new BusinessException("Sipariş durumu yalnızca onaylandı veya reddedildi olabilir.");

        var order = await _unitOfWork.Orders.GetByIdAsync(orderId, cancellationToken)
            ?? throw new BusinessException("Sipariş bulunamadı.");

        if (order.Status != OrderStatus.Pending)
            throw new BusinessException("Yalnızca bekleyen siparişlerin durumu değiştirilebilir.");

        order.Status = status;
        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }

    private async Task<int> PlaceOrderCoreAsync(int userId, CancellationToken cancellationToken)
    {
        var cart = await _unitOfWork.Carts.Query()
            .Include(c => c.Items)
            .FirstOrDefaultAsync(c => c.UserId == userId, cancellationToken);

        if (cart is null || cart.Items.Count == 0)
            throw new BusinessException("Sepetiniz boş.");

        var productIds = cart.Items.Select(i => i.ProductId).Distinct().ToList();
        var products = await _unitOfWork.Products.Query()
            .AsNoTracking()
            .Where(p => productIds.Contains(p.Id))
            .ToDictionaryAsync(p => p.Id, cancellationToken);

        foreach (var line in cart.Items)
        {
            if (!products.TryGetValue(line.ProductId, out var product))
                throw new BusinessException("Ürün bulunamadı.");

            if (!product.IsActive)
                throw new BusinessException($"{product.Name} artık satışta değil.");

            var affected = await _unitOfWork.TryDecrementStockAsync(product.Id, line.Quantity, cancellationToken);
            if (affected == 0)
            {
                var current = await _unitOfWork.Products.Query()
                    .AsNoTracking()
                    .Where(p => p.Id == product.Id)
                    .Select(p => p.StockQuantity)
                    .FirstAsync(cancellationToken);
                throw new BusinessException($"{product.Name} için yeterli stok bulunmamaktadır. Mevcut stok: {current}.");
            }
        }

        var lines = cart.Items.Select(i =>
        {
            var product = products[i.ProductId];
            return new OrderItem
            {
                ProductId = i.ProductId,
                ProductCode = product.ProductCode,
                ProductName = product.Name,
                Quantity = i.Quantity,
                UnitPrice = product.Price,
                TotalPrice = product.Price * i.Quantity
            };
        }).ToList();

        var order = new Order
        {
            OrderNumber = await NextOrderNumberAsync(cancellationToken),
            UserId = userId,
            OrderDate = DateTime.UtcNow,
            Status = OrderStatus.Pending,
            TotalAmount = lines.Sum(i => i.TotalPrice),
            Items = lines
        };

        await _unitOfWork.Orders.AddAsync(order, cancellationToken);

        foreach (var item in cart.Items.ToList())
            _unitOfWork.CartItems.Remove(item);

        cart.UpdatedAt = DateTime.UtcNow;
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return order.Id;
    }

    private async Task<string> NextOrderNumberAsync(CancellationToken cancellationToken)
    {
        var lastId = await _unitOfWork.Orders.Query()
            .Select(o => (int?)o.Id)
            .MaxAsync(cancellationToken) ?? 0;
        return $"SIP-{(lastId + 1):D6}";
    }

    private async Task<Order> LoadOrderAsync(System.Linq.Expressions.Expression<Func<Order, bool>> predicate, bool includeUser, CancellationToken cancellationToken)
    {
        IQueryable<Order> query = _unitOfWork.Orders.Query().AsNoTracking().Include(o => o.Items);
        if (includeUser)
            query = query.Include(o => o.User);

        return await query.FirstOrDefaultAsync(predicate, cancellationToken)
            ?? throw new BusinessException("Sipariş bulunamadı.");
    }

    private static OrderDetailDto MapDetail(Order order) => new()
    {
        Id = order.Id,
        OrderNumber = order.OrderNumber,
        OrderDate = order.OrderDate,
        Status = order.Status,
        TotalAmount = order.TotalAmount,
        CustomerName = order.User is null ? string.Empty : $"{order.User.FirstName} {order.User.LastName}",
        Items = order.Items
            .OrderBy(i => i.ProductName)
            .Select(i => new OrderLineDto
            {
                ProductCode = i.ProductCode,
                ProductName = i.ProductName,
                Quantity = i.Quantity,
                UnitPrice = i.UnitPrice,
                LineTotal = i.TotalPrice
            })
            .ToList()
    };
}
