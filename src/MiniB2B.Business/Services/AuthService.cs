using MiniB2B.Business.Dtos;
using MiniB2B.Business.Exceptions;
using MiniB2B.Business.Security;
using MiniB2B.Business.Validation;
using MiniB2B.DataAccess;
using MiniB2B.Domain.Entities;
using MiniB2B.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace MiniB2B.Business.Services;

public class AuthService : IAuthService
{
    private readonly IUnitOfWork _unitOfWork;

    public AuthService(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<AuthUserDto> RegisterAsync(RegisterDto dto, CancellationToken cancellationToken = default)
    {
        UserRules.ValidateProfile(dto.FirstName, dto.LastName, dto.Email, dto.Phone, dto.Username);
        UserRules.ValidatePassword(dto.Password, required: true);

        var username = dto.Username.Trim();
        var email = dto.Email.Trim().ToLowerInvariant();

        var exists = await _unitOfWork.Users.Query().AnyAsync(
            u => u.Username == username || u.Email == email,
            cancellationToken);

        if (exists)
            throw new BusinessException("Bu kullanıcı adı veya e-posta zaten kayıtlı.");

        var user = new User
        {
            FirstName = dto.FirstName.Trim(),
            LastName = dto.LastName.Trim(),
            Email = email,
            Phone = dto.Phone.Trim(),
            Username = username,
            PasswordHash = PasswordHasher.Hash(dto.Password),
            Role = UserRole.Customer,
            IsActive = true,
            CreatedAt = DateTime.UtcNow,
            Cart = new Cart { CreatedAt = DateTime.UtcNow }
        };

        await _unitOfWork.Users.AddAsync(user, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return Map(user);
    }

    public async Task<AuthUserDto> ValidateLoginAsync(LoginDto dto, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(dto.UsernameOrEmail) || string.IsNullOrWhiteSpace(dto.Password))
            throw new BusinessException("Kullanıcı adı/e-posta ve şifre zorunludur.");

        var key = dto.UsernameOrEmail.Trim();
        var user = await _unitOfWork.Users.Query()
            .FirstOrDefaultAsync(
                u => u.Username == key || u.Email == key.ToLower(),
                cancellationToken);

        if (user is null || !PasswordHasher.Verify(dto.Password, user.PasswordHash))
            throw new BusinessException("Kullanıcı adı veya şifre hatalı.");

        if (!user.IsActive)
            throw new BusinessException("Hesabınız pasif durumdadır. Lütfen yöneticinizle iletişime geçin.");

        return Map(user);
    }

    private static AuthUserDto Map(User user) => new()
    {
        Id = user.Id,
        FirstName = user.FirstName,
        LastName = user.LastName,
        Email = user.Email,
        Username = user.Username,
        Role = user.Role.ToString()
    };
}
