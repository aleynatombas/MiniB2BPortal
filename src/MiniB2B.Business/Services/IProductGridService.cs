using MiniB2B.Business.Dtos;

namespace MiniB2B.Business.Services;

public interface IProductGridService
{
    Task<List<ProductGridColumnDto>> GetVisibleAsync(CancellationToken cancellationToken = default);
    Task<List<ProductGridColumnDto>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<ProductGridColumnDto> GetByIdAsync(int id, CancellationToken cancellationToken = default);
    Task UpdateAsync(ProductGridColumnDto dto, CancellationToken cancellationToken = default);
}
