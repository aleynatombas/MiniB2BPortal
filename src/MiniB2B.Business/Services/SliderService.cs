using MiniB2B.Business.Dtos;
using MiniB2B.Business.Exceptions;
using MiniB2B.DataAccess;
using MiniB2B.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace MiniB2B.Business.Services;

public class SliderService : ISliderService
{
    private readonly IUnitOfWork _unitOfWork;

    public SliderService(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public Task<List<SliderDto>> GetActiveAsync(CancellationToken cancellationToken = default)
        => Query(activeOnly: true).ToListAsync(cancellationToken);

    public Task<List<SliderDto>> GetAllAsync(CancellationToken cancellationToken = default)
        => Query(activeOnly: false).ToListAsync(cancellationToken);

    public async Task<SliderDto> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        var slider = await _unitOfWork.Sliders.Query()
            .AsNoTracking()
            .FirstOrDefaultAsync(s => s.Id == id, cancellationToken)
            ?? throw new BusinessException("Slider bulunamadı.");
        return Map(slider);
    }

    public async Task CreateAsync(SliderDto dto, CancellationToken cancellationToken = default)
    {
        Validate(dto, imageRequired: true);
        await _unitOfWork.Sliders.AddAsync(new Slider
        {
            Title = dto.Title.Trim(),
            Subtitle = TrimOrNull(dto.Subtitle),
            ImagePath = dto.ImagePath.Trim(),
            LinkUrl = TrimOrNull(dto.LinkUrl),
            DisplayOrder = dto.DisplayOrder,
            IsActive = dto.IsActive
        }, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }

    public async Task UpdateAsync(SliderDto dto, CancellationToken cancellationToken = default)
    {
        Validate(dto, imageRequired: false);
        var slider = await _unitOfWork.Sliders.GetByIdAsync(dto.Id, cancellationToken)
            ?? throw new BusinessException("Slider bulunamadı.");

        slider.Title = dto.Title.Trim();
        slider.Subtitle = TrimOrNull(dto.Subtitle);
        if (!string.IsNullOrWhiteSpace(dto.ImagePath))
            slider.ImagePath = dto.ImagePath.Trim();
        slider.LinkUrl = TrimOrNull(dto.LinkUrl);
        slider.DisplayOrder = dto.DisplayOrder;
        slider.IsActive = dto.IsActive;
        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }

    private IQueryable<SliderDto> Query(bool activeOnly)
    {
        var query = _unitOfWork.Sliders.Query().AsNoTracking();
        if (activeOnly)
            query = query.Where(s => s.IsActive);

        return query
            .OrderBy(s => s.DisplayOrder)
            .ThenBy(s => s.Title)
            .Select(s => new SliderDto
            {
                Id = s.Id,
                Title = s.Title,
                Subtitle = s.Subtitle,
                ImagePath = s.ImagePath,
                LinkUrl = s.LinkUrl,
                DisplayOrder = s.DisplayOrder,
                IsActive = s.IsActive
            });
    }

    private static void Validate(SliderDto dto, bool imageRequired)
    {
        if (string.IsNullOrWhiteSpace(dto.Title))
            throw new BusinessException("Slider başlığı zorunludur.");
        if (imageRequired && string.IsNullOrWhiteSpace(dto.ImagePath))
            throw new BusinessException("Slider görseli zorunludur.");
        if (dto.DisplayOrder < 0)
            throw new BusinessException("Görüntüleme sırası negatif olamaz.");
    }

    private static string? TrimOrNull(string? value)
        => string.IsNullOrWhiteSpace(value) ? null : value.Trim();

    private static SliderDto Map(Slider s) => new()
    {
        Id = s.Id,
        Title = s.Title,
        Subtitle = s.Subtitle,
        ImagePath = s.ImagePath,
        LinkUrl = s.LinkUrl,
        DisplayOrder = s.DisplayOrder,
        IsActive = s.IsActive
    };
}
