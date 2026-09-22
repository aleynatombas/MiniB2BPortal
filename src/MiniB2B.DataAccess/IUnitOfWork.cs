using MiniB2B.DataAccess.Repositories;
using MiniB2B.Domain.Entities;

namespace MiniB2B.DataAccess;

public interface IUnitOfWork
{
    IRepository<User> Users { get; }
    IRepository<Category> Categories { get; }
    IRepository<Product> Products { get; }
    IRepository<Cart> Carts { get; }
    IRepository<CartItem> CartItems { get; }
    IRepository<Order> Orders { get; }
    IRepository<OrderItem> OrderItems { get; }
    IRepository<Slider> Sliders { get; }
    IRepository<ProductGridColumn> ProductGridColumns { get; }
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
    Task<int> TryDecrementStockAsync(int productId, int quantity, CancellationToken cancellationToken = default);
    Task<T> ExecuteInTransactionAsync<T>(Func<CancellationToken, Task<T>> work, CancellationToken cancellationToken = default);
}
