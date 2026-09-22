using MiniB2B.Business.Dtos;
using MiniB2B.Business.Exceptions;
using MiniB2B.Business.Security;
using MiniB2B.Business.Validation;
using MiniB2B.DataAccess;
using MiniB2B.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace MiniB2B.Business.Services;

public class UserService : IUserService
{
    private readonly IUnitOfWork _unitOfWork;

    public UserService(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<List<UserDto>> GetAllAsync(string? search, CancellationToken cancellationToken = default)
    {
        var query = _unitOfWork.Users.Query().AsNoTracking();

        if (!string.IsNullOrWhiteSpace(search))
        {
            var term = search.Trim();
            query = query.Where(u =>
                u.FirstName.Contains(term) ||
                u.LastName.Contains(term) ||
                u.Email.Contains(term) ||
                u.Username.Contains(term) ||
                u.Phone.Contains(term));
        }

        var users = await query
            .OrderBy(u => u.LastName)
            .ThenBy(u => u.FirstName)
            .ToListAsync(cancellationToken);

        return users.Select(Map).ToList();
    }

    public Task<int> CountAsync(CancellationToken cancellationToken = default)
        => _unitOfWork.Users.Query().CountAsync(cancellationToken);

    public async Task<UserDto> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        var user = await _unitOfWork.Users.Query()
            .AsNoTracking()
            .FirstOrDefaultAsync(u => u.Id == id, cancellationToken)
            ?? throw new BusinessException("Kullanıcı bulunamadı.");

        return Map(user);
    }

    public async Task UpdateAsync(UserFormDto dto, CancellationToken cancellationToken = default)
    {
        UserRules.ValidateProfile(dto.FirstName, dto.LastName, dto.Email, dto.Phone, dto.Username);
        UserRules.ValidatePassword(dto.Password, required: false);

        var user = await _unitOfWork.Users.GetByIdAsync(dto.Id, cancellationToken)
            ?? throw new BusinessException("Kullanıcı bulunamadı.");

        var email = dto.Email.Trim().ToLowerInvariant();
        var username = dto.Username.Trim();

        var duplicate = await _unitOfWork.Users.Query().AnyAsync(
            u => u.Id != dto.Id && (u.Email == email || u.Username == username),
            cancellationToken);
        if (duplicate)
            throw new BusinessException("Bu e-posta veya kullanıcı adı başka bir kullanıcıya ait.");

        user.FirstName = dto.FirstName.Trim();
        user.LastName = dto.LastName.Trim();
        user.Email = email;
        user.Phone = dto.Phone.Trim();
        user.Username = username;
        user.Role = dto.Role;
        user.IsActive = dto.IsActive;
        user.UpdatedAt = DateTime.UtcNow;

        if (!string.IsNullOrWhiteSpace(dto.Password))
            user.PasswordHash = PasswordHasher.Hash(dto.Password);

        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }

    private static UserDto Map(User user) => new()
    {
        Id = user.Id,
        FirstName = user.FirstName,
        LastName = user.LastName,
        Email = user.Email,
        Phone = user.Phone,
        Username = user.Username,
        Role = user.Role,
        IsActive = user.IsActive,
        CreatedAt = user.CreatedAt
    };
}
