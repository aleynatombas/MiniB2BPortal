using MiniB2B.Business.Dtos;

namespace MiniB2B.Business.Services;

public interface IUserService
{
    Task<List<UserDto>> GetAllAsync(string? search, CancellationToken cancellationToken = default);
    Task<int> CountAsync(CancellationToken cancellationToken = default);
    Task<UserDto> GetByIdAsync(int id, CancellationToken cancellationToken = default);
    Task UpdateAsync(UserFormDto dto, CancellationToken cancellationToken = default);
}
