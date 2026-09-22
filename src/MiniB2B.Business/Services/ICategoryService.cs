using MiniB2B.Business.Dtos;

namespace MiniB2B.Business.Services;

public interface ICategoryService
{
    Task<List<CategoryDto>> GetAllAsync(CancellationToken cancellationToken = default);
}
