using MiniB2B.Business.Dtos;
using MiniB2B.Domain.Enums;

namespace MiniB2B.Business.Services;

public interface IOrderService
{
    Task<int> PlaceOrderAsync(int userId, CancellationToken cancellationToken = default);
    Task<List<OrderSummaryDto>> GetByUserAsync(int userId, CancellationToken cancellationToken = default);
    Task<List<OrderSummaryDto>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<int> CountAsync(CancellationToken cancellationToken = default);
    Task<OrderDetailDto> GetByIdAsync(int userId, int orderId, CancellationToken cancellationToken = default);
    Task<OrderDetailDto> GetByIdAsync(int orderId, CancellationToken cancellationToken = default);
    Task UpdateStatusAsync(int orderId, OrderStatus status, CancellationToken cancellationToken = default);
}
