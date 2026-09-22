using MiniB2B.Business.Dtos;

namespace MiniB2B.Business.Services;

public interface ICartService
{
    Task<CartDto> GetAsync(int userId, CancellationToken cancellationToken = default);
    Task<int> GetItemCountAsync(int userId, CancellationToken cancellationToken = default);
    Task AddItemAsync(int userId, int productId, int quantity, CancellationToken cancellationToken = default);
    Task UpdateItemAsync(int userId, int itemId, int quantity, CancellationToken cancellationToken = default);
    Task RemoveItemAsync(int userId, int itemId, CancellationToken cancellationToken = default);
}
