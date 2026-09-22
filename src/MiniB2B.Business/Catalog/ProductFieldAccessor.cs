using System.Globalization;
using System.Reflection;
using MiniB2B.Business.Dtos;

namespace MiniB2B.Business.Catalog;

public static class ProductFieldAccessor
{
    private static readonly Dictionary<string, PropertyInfo> Properties =
        typeof(ProductDto).GetProperties(BindingFlags.Public | BindingFlags.Instance)
            .ToDictionary(p => p.Name, StringComparer.OrdinalIgnoreCase);

    public static readonly IReadOnlyList<string> AllowedFieldNames =
        Properties.Keys
            .Concat(["Quantity", "AddToCart"])
            .OrderBy(n => n)
            .ToArray();

    public static bool IsAllowed(string fieldName)
        => AllowedFieldNames.Contains(fieldName, StringComparer.OrdinalIgnoreCase);

    public static object? GetValue(ProductDto product, string fieldName)
        => Properties.TryGetValue(fieldName, out var property)
            ? property.GetValue(product)
            : null;

    public static string GetText(ProductDto product, string fieldName)
    {
        var value = GetValue(product, fieldName);
        return value switch
        {
            null => string.Empty,
            decimal number => number.ToString("N2", CultureInfo.CurrentUICulture),
            _ => Convert.ToString(value, CultureInfo.CurrentUICulture) ?? string.Empty
        };
    }
}
