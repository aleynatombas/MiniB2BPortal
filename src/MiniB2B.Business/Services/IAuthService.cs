using MiniB2B.Business.Dtos;

namespace MiniB2B.Business.Services;

public interface IAuthService
{
    Task<AuthUserDto> RegisterAsync(RegisterDto dto, CancellationToken cancellationToken = default);
    Task<AuthUserDto> ValidateLoginAsync(LoginDto dto, CancellationToken cancellationToken = default);
}
