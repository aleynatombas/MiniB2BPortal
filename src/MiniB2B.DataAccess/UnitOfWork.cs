using MiniB2B.DataAccess.Repositories;
using MiniB2B.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace MiniB2B.DataAccess;

public class UnitOfWork : IUnitOfWork
{
    private readonly AppDbContext _context;

    public UnitOfWork(AppDbContext context)
    {
        _context = context;
        Users = new EfRepository<User>(context);
        Categories = new EfRepository<Category>(context);
        Products = new EfRepository<Product>(context);
        Carts = new EfRepository<Cart>(context);
        CartItems = new EfRepository<CartItem>(context);
        Orders = new EfRepository<Order>(context);
        OrderItems = new EfRepository<OrderItem>(context);
        Sliders = new EfRepository<Slider>(context);
        ProductGridColumns = new EfRepository<ProductGridColumn>(context);
    }

    public IRepository<User> Users { get; }
    public IRepository<Category> Categories { get; }
    public IRepository<Product> Products { get; }
    public IRepository<Cart> Carts { get; }
    public IRepository<CartItem> CartItems { get; }
    public IRepository<Order> Orders { get; }
    public IRepository<OrderItem> OrderItems { get; }
    public IRepository<Slider> Sliders { get; }
    public IRepository<ProductGridColumn> ProductGridColumns { get; }

    public Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        => _context.SaveChangesAsync(cancellationToken);

    public Task<int> TryDecrementStockAsync(int productId, int quantity, CancellationToken cancellationToken = default)
        => _context.Products
            .Where(p => p.Id == productId && p.IsActive && p.StockQuantity >= quantity)
            .ExecuteUpdateAsync(setters => setters
                .SetProperty(p => p.StockQuantity, p => p.StockQuantity - quantity)
                .SetProperty(p => p.UpdatedAt, DateTime.UtcNow), cancellationToken);

    public async Task<T> ExecuteInTransactionAsync<T>(Func<CancellationToken, Task<T>> work, CancellationToken cancellationToken = default)
    {
        await using var transaction = await _context.Database.BeginTransactionAsync(cancellationToken);
        try
        {
            var result = await work(cancellationToken);
            await transaction.CommitAsync(cancellationToken);
            return result;
        }
        catch
        {
            await transaction.RollbackAsync(cancellationToken);
            throw;
        }
    }
}
