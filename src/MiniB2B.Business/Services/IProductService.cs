using MiniB2B.Business.Dtos;

namespace MiniB2B.Business.Services;

public interface IProductService
{
    Task<List<ProductDto>> SearchAsync(string? term, bool activeOnly, CancellationToken cancellationToken = default);
    Task<List<ProductDto>> GetFeaturedAsync(int take, CancellationToken cancellationToken = default);
    Task<int> CountAsync(CancellationToken cancellationToken = default);
    Task<ProductDto> GetByIdAsync(int id, CancellationToken cancellationToken = default);
    Task<int> CreateAsync(ProductFormDto dto, CancellationToken cancellationToken = default);
    Task UpdateAsync(ProductFormDto dto, CancellationToken cancellationToken = default);
    Task SetActiveAsync(int id, bool isActive, CancellationToken cancellationToken = default);
}
