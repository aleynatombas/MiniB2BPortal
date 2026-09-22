using MiniB2B.Business.Exceptions;

namespace MiniB2B.Web.Infrastructure;

public static class ImageStorage
{
    private static readonly HashSet<string> AllowedExtensions = new(StringComparer.OrdinalIgnoreCase)
    {
        ".jpg", ".jpeg", ".png", ".webp", ".gif", ".svg"
    };

    public static async Task<string> SaveAsync(IFormFile file, string webRootPath, string folder)
    {
        if (file is null || file.Length == 0)
            throw new BusinessException("Lütfen bir görsel seçiniz.");

        if (file.Length > 2 * 1024 * 1024)
            throw new BusinessException("Görsel boyutu 2 MB'ı aşamaz.");

        var extension = Path.GetExtension(file.FileName);
        if (!AllowedExtensions.Contains(extension))
            throw new BusinessException("Desteklenen görsel formatları: jpg, png, webp, gif, svg.");

        var relativeFolder = Path.Combine("uploads", folder);
        var physicalFolder = Path.Combine(webRootPath, relativeFolder);
        Directory.CreateDirectory(physicalFolder);

        var fileName = $"{Guid.NewGuid():N}{extension.ToLowerInvariant()}";
        var physicalPath = Path.Combine(physicalFolder, fileName);

        await using var stream = new FileStream(physicalPath, FileMode.Create);
        await file.CopyToAsync(stream);

        return "/" + Path.Combine(relativeFolder, fileName).Replace('\\', '/');
    }
}
