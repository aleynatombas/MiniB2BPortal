using MiniB2B.Business.Dtos;
using MiniB2B.Business.Exceptions;
using MiniB2B.DataAccess;
using MiniB2B.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace MiniB2B.Business.Services;

public class ProductService : IProductService
{
    private readonly IUnitOfWork _unitOfWork;

    public ProductService(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<List<ProductDto>> SearchAsync(string? term, bool activeOnly, CancellationToken cancellationToken = default)
    {
        IQueryable<Product> query = _unitOfWork.Products.Query()
            .AsNoTracking()
            .Include(p => p.Category);

        if (activeOnly)
            query = query.Where(p => p.IsActive);

        if (!string.IsNullOrWhiteSpace(term))
        {
            var t = term.Trim();
            query = query.Where(p =>
                p.ProductCode.Contains(t) ||
                p.Name.Contains(t) ||
                (p.Description != null && p.Description.Contains(t)) ||
                (p.Brand != null && p.Brand.Contains(t)) ||
                (p.ManufacturerCode != null && p.ManufacturerCode.Contains(t)) ||
                (p.CustomCode1 != null && p.CustomCode1.Contains(t)) ||
                (p.CustomCode2 != null && p.CustomCode2.Contains(t)) ||
                (p.Category != null && p.Category.Name.Contains(t)));
        }

        var products = await query.OrderBy(p => p.Name).ToListAsync(cancellationToken);
        return products.Select(Map).ToList();
    }

    public Task<int> CountAsync(CancellationToken cancellationToken = default)
        => _unitOfWork.Products.Query().CountAsync(cancellationToken);

    public async Task<ProductDto> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        var product = await _unitOfWork.Products.Query()
            .AsNoTracking()
            .Include(p => p.Category)
            .FirstOrDefaultAsync(p => p.Id == id, cancellationToken)
            ?? throw new BusinessException("Ürün bulunamadı.");

        return Map(product);
    }

    public async Task<int> CreateAsync(ProductFormDto dto, CancellationToken cancellationToken = default)
    {
        Validate(dto);

        var code = dto.ProductCode.Trim();
        var exists = await _unitOfWork.Products.Query()
            .AnyAsync(p => p.ProductCode == code, cancellationToken);
        if (exists)
            throw new BusinessException("Bu ürün kodu zaten kullanılıyor.");

        await EnsureCategoryExists(dto.CategoryId, cancellationToken);

        var product = new Product
        {
            ProductCode = code,
            Name = dto.Name.Trim(),
            Description = TrimOrNull(dto.Description),
            Brand = TrimOrNull(dto.Brand),
            ManufacturerCode = TrimOrNull(dto.ManufacturerCode),
            CustomCode1 = TrimOrNull(dto.CustomCode1),
            CustomCode2 = TrimOrNull(dto.CustomCode2),
            ImagePath = TrimOrNull(dto.ImagePath),
            StockQuantity = dto.StockQuantity,
            CriticalStockLevel = dto.CriticalStockLevel,
            Price = dto.Price,
            CategoryId = dto.CategoryId,
            IsActive = dto.IsActive,
            CreatedAt = DateTime.UtcNow
        };

        await _unitOfWork.Products.AddAsync(product, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return product.Id;
    }

    public async Task UpdateAsync(ProductFormDto dto, CancellationToken cancellationToken = default)
    {
        Validate(dto);

        var product = await _unitOfWork.Products.GetByIdAsync(dto.Id, cancellationToken)
            ?? throw new BusinessException("Ürün bulunamadı.");

        var code = dto.ProductCode.Trim();
        var duplicate = await _unitOfWork.Products.Query()
            .AnyAsync(p => p.Id != dto.Id && p.ProductCode == code, cancellationToken);
        if (duplicate)
            throw new BusinessException("Bu ürün kodu başka bir ürüne ait.");

        await EnsureCategoryExists(dto.CategoryId, cancellationToken);

        product.ProductCode = code;
        product.Name = dto.Name.Trim();
        product.Description = TrimOrNull(dto.Description);
        product.Brand = TrimOrNull(dto.Brand);
        product.ManufacturerCode = TrimOrNull(dto.ManufacturerCode);
        product.CustomCode1 = TrimOrNull(dto.CustomCode1);
        product.CustomCode2 = TrimOrNull(dto.CustomCode2);
        if (!string.IsNullOrWhiteSpace(dto.ImagePath))
            product.ImagePath = dto.ImagePath.Trim();
        product.StockQuantity = dto.StockQuantity;
        product.CriticalStockLevel = dto.CriticalStockLevel;
        product.Price = dto.Price;
        product.CategoryId = dto.CategoryId;
        product.IsActive = dto.IsActive;
        product.UpdatedAt = DateTime.UtcNow;

        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }

    private async Task EnsureCategoryExists(int categoryId, CancellationToken cancellationToken)
    {
        var exists = await _unitOfWork.Categories.Query()
            .AnyAsync(c => c.Id == categoryId, cancellationToken);
        if (!exists)
            throw new BusinessException("Geçerli bir kategori seçiniz.");
    }

    private static void Validate(ProductFormDto dto)
    {
        if (string.IsNullOrWhiteSpace(dto.ProductCode))
            throw new BusinessException("Ürün kodu zorunludur.");
        if (dto.ProductCode.Trim().Length > 50)
            throw new BusinessException("Ürün kodu en fazla 50 karakter olabilir.");
        if (string.IsNullOrWhiteSpace(dto.Name))
            throw new BusinessException("Ürün adı zorunludur.");
        if (dto.Name.Trim().Length > 200)
            throw new BusinessException("Ürün adı en fazla 200 karakter olabilir.");
        if (dto.Description is { Length: > 2000 })
            throw new BusinessException("Açıklama en fazla 2000 karakter olabilir.");
        if (dto.Brand is { Length: > 100 })
            throw new BusinessException("Marka en fazla 100 karakter olabilir.");
        if (dto.ManufacturerCode is { Length: > 80 })
            throw new BusinessException("Üretici kodu en fazla 80 karakter olabilir.");
        if (dto.CustomCode1 is { Length: > 80 })
            throw new BusinessException("Özel kod 1 en fazla 80 karakter olabilir.");
        if (dto.CustomCode2 is { Length: > 80 })
            throw new BusinessException("Özel kod 2 en fazla 80 karakter olabilir.");
        if (dto.Price <= 0)
            throw new BusinessException("Fiyat 0'dan büyük olmalıdır.");
        if (dto.StockQuantity < 0)
            throw new BusinessException("Stok miktarı negatif olamaz.");
        if (dto.CriticalStockLevel < 0)
            throw new BusinessException("Kritik stok seviyesi negatif olamaz.");
        if (dto.CategoryId <= 0)
            throw new BusinessException("Kategori seçimi zorunludur.");
    }

    private static string? TrimOrNull(string? value)
        => string.IsNullOrWhiteSpace(value) ? null : value.Trim();

    private static ProductDto Map(Product p) => new()
    {
        Id = p.Id,
        ProductCode = p.ProductCode,
        Name = p.Name,
        Description = p.Description,
        Brand = p.Brand,
        ManufacturerCode = p.ManufacturerCode,
        CustomCode1 = p.CustomCode1,
        CustomCode2 = p.CustomCode2,
        ImagePath = p.ImagePath,
        StockQuantity = p.StockQuantity,
        CriticalStockLevel = p.CriticalStockLevel,
        Price = p.Price,
        CategoryId = p.CategoryId,
        CategoryName = p.Category?.Name ?? string.Empty,
        IsActive = p.IsActive
    };
}
