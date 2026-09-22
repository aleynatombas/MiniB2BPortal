using MiniB2B.Business.Catalog;
using MiniB2B.Business.Dtos;
using MiniB2B.Business.Exceptions;
using MiniB2B.DataAccess;
using MiniB2B.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace MiniB2B.Business.Services;

public class ProductGridService : IProductGridService
{
    private readonly IUnitOfWork _unitOfWork;

    public ProductGridService(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public Task<List<ProductGridColumnDto>> GetVisibleAsync(CancellationToken cancellationToken = default)
        => Query(visibleOnly: true).ToListAsync(cancellationToken);

    public Task<List<ProductGridColumnDto>> GetAllAsync(CancellationToken cancellationToken = default)
        => Query(visibleOnly: false).ToListAsync(cancellationToken);

    public async Task<ProductGridColumnDto> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        var column = await _unitOfWork.ProductGridColumns.Query()
            .AsNoTracking()
            .FirstOrDefaultAsync(c => c.Id == id, cancellationToken)
            ?? throw new BusinessException("Grid kolonu bulunamadı.");
        return Map(column);
    }

    public async Task UpdateAsync(ProductGridColumnDto dto, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(dto.Header))
            throw new BusinessException("Kolon başlığı zorunludur.");
        if (!ProductFieldAccessor.IsAllowed(dto.FieldName))
            throw new BusinessException("Geçersiz kolon alanı. Yalnızca ürün alanları veya Adet / Sepete ekle kullanılabilir.");
        if (dto.SortOrder < 0)
            throw new BusinessException("Sıra numarası negatif olamaz.");

        var field = dto.FieldName.Trim();
        var duplicate = await _unitOfWork.ProductGridColumns.Query()
            .AnyAsync(c => c.Id != dto.Id && c.FieldName == field, cancellationToken);
        if (duplicate)
            throw new BusinessException("Bu alan adı başka bir kolonda kullanılıyor.");

        var column = await _unitOfWork.ProductGridColumns.GetByIdAsync(dto.Id, cancellationToken)
            ?? throw new BusinessException("Grid kolonu bulunamadı.");

        column.FieldName = field;
        column.Header = dto.Header.Trim();
        column.RenderType = dto.RenderType;
        column.SortOrder = dto.SortOrder;
        column.Width = string.IsNullOrWhiteSpace(dto.Width) ? null : dto.Width.Trim();
        column.Alignment = dto.Alignment;
        column.ShowOnDesktop = dto.ShowOnDesktop;
        column.ShowOnTablet = dto.ShowOnTablet;
        column.ShowOnMobile = dto.ShowOnMobile;
        column.IsVisible = dto.IsVisible;
        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }

    private IQueryable<ProductGridColumnDto> Query(bool visibleOnly)
    {
        var query = _unitOfWork.ProductGridColumns.Query().AsNoTracking();
        if (visibleOnly)
            query = query.Where(c => c.IsVisible);

        return query
            .OrderBy(c => c.SortOrder)
            .Select(c => new ProductGridColumnDto
            {
                Id = c.Id,
                FieldName = c.FieldName,
                Header = c.Header,
                RenderType = c.RenderType,
                SortOrder = c.SortOrder,
                Width = c.Width,
                Alignment = c.Alignment,
                ShowOnDesktop = c.ShowOnDesktop,
                ShowOnTablet = c.ShowOnTablet,
                ShowOnMobile = c.ShowOnMobile,
                IsVisible = c.IsVisible
            });
    }

    private static ProductGridColumnDto Map(ProductGridColumn c) => new()
    {
        Id = c.Id,
        FieldName = c.FieldName,
        Header = c.Header,
        RenderType = c.RenderType,
        SortOrder = c.SortOrder,
        Width = c.Width,
        Alignment = c.Alignment,
        ShowOnDesktop = c.ShowOnDesktop,
        ShowOnTablet = c.ShowOnTablet,
        ShowOnMobile = c.ShowOnMobile,
        IsVisible = c.IsVisible
    };
}
