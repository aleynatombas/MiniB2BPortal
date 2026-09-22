using MiniB2B.Business.Dtos;

namespace MiniB2B.Business.Services;

public interface ISliderService
{
    Task<List<SliderDto>> GetActiveAsync(CancellationToken cancellationToken = default);
    Task<List<SliderDto>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<SliderDto> GetByIdAsync(int id, CancellationToken cancellationToken = default);
    Task CreateAsync(SliderDto dto, CancellationToken cancellationToken = default);
    Task UpdateAsync(SliderDto dto, CancellationToken cancellationToken = default);
}
