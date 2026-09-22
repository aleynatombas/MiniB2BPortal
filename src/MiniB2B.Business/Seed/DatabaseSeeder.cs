using MiniB2B.Business.Security;
using MiniB2B.DataAccess;
using MiniB2B.Domain.Entities;
using MiniB2B.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace MiniB2B.Business.Seed;

public static class DatabaseSeeder
{
    public static async Task SeedAsync(IServiceProvider services)
    {
        using var scope = services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        await db.Database.MigrateAsync();

        if (!await db.Users.AnyAsync())
            await SeedFreshAsync(db);
        else
            await RebrandToAutoPartsAsync(db);

        await FillMissingImagesAsync(db);
    }

    private static async Task SeedFreshAsync(AppDbContext db)
    {
        var categories = CreateCategories();
        db.Categories.AddRange(categories);
        await db.SaveChangesAsync();

        db.Users.AddRange(
            new User
            {
                FirstName = "Sistem",
                LastName = "Yöneticisi",
                Email = "admin@otobayi.com",
                Phone = "0212 000 00 01",
                Username = "admin",
                PasswordHash = PasswordHasher.Hash("Admin123!"),
                Role = UserRole.Admin,
                IsActive = true,
                CreatedAt = DateTime.UtcNow,
                Cart = new Cart { CreatedAt = DateTime.UtcNow }
            },
            new User
            {
                FirstName = "Mehmet",
                LastName = "Usta",
                Email = "bayi@otobayi.com",
                Phone = "0532 111 22 33",
                Username = "bayi",
                PasswordHash = PasswordHasher.Hash("Bayi123!"),
                Role = UserRole.Customer,
                IsActive = true,
                CreatedAt = DateTime.UtcNow,
                Cart = new Cart { CreatedAt = DateTime.UtcNow }
            });

        db.Products.AddRange(Catalog.Select(p => CreateProduct(p, categories[p.CategoryIndex].Id, resetStock: true)));
        db.Sliders.AddRange(CreateSliders());
        db.ProductGridColumns.AddRange(CreateGridColumns());
        await db.SaveChangesAsync();
    }

    private static async Task RebrandToAutoPartsAsync(AppDbContext db)
    {
        var needsRebrand = await db.Categories.AnyAsync(c => c.Name == "Bağlantı Elemanları")
            || await db.Products.AnyAsync(p => p.ProductCode == "CVT-M8-20")
            || await db.Sliders.AnyAsync(s => s.Title == "Eylül Kampanyası");
        if (!needsRebrand)
            return;

        var categories = await db.Categories.OrderBy(c => c.Id).ToListAsync();
        var names = new[]
        {
            ("Fren Sistemi", "Balata, disk, hortum"),
            ("Filtreler", "Yağ, hava, yakıt ve polen filtreleri"),
            ("Ateşleme", "Buji, bobin ve sensörler"),
            ("Aydınlatma", "Far, sinyal ve ampul")
        };
        for (var i = 0; i < categories.Count && i < names.Length; i++)
        {
            categories[i].Name = names[i].Item1;
            categories[i].Description = names[i].Item2;
        }

        var products = await db.Products.ToListAsync();
        foreach (var product in products)
        {
            var row = Catalog.FirstOrDefault(c => c.OldCode == product.ProductCode);
            if (row is null)
                continue;

            ApplyCatalog(product, row, resetStock: false);
            if (row.CategoryIndex < categories.Count)
                product.CategoryId = categories[row.CategoryIndex].Id;
        }

        var test = products.FirstOrDefault(p => p.ProductCode == "TST-001");
        if (test is not null)
        {
            test.ProductCode = "ATS-ABS-ON";
            test.Name = "ABS Sensörü Ön";
            test.Description = "Ön teker ABS hız sensörü.";
            test.Brand = "Bosch";
            test.ManufacturerCode = "0265007508";
            test.CustomCode1 = "ABS";
            test.CustomCode2 = "FRONT";
            test.ImagePath = "/uploads/products/placeholder.svg";
            test.UpdatedAt = DateTime.UtcNow;
        }

        var sliders = await db.Sliders.OrderBy(s => s.DisplayOrder).ToListAsync();
        var fresh = CreateSliders();
        for (var i = 0; i < sliders.Count && i < fresh.Count; i++)
        {
            sliders[i].Title = fresh[i].Title;
            sliders[i].Subtitle = fresh[i].Subtitle;
            sliders[i].LinkUrl = fresh[i].LinkUrl;
            sliders[i].DisplayOrder = fresh[i].DisplayOrder;
            sliders[i].IsActive = true;
        }

        var admin = await db.Users.FirstOrDefaultAsync(u => u.Username == "admin");
        if (admin is not null)
            admin.Email = "admin@otobayi.com";
        var dealer = await db.Users.FirstOrDefaultAsync(u => u.Username == "bayi");
        if (dealer is not null)
        {
            dealer.FirstName = "Mehmet";
            dealer.LastName = "Usta";
            dealer.Email = "bayi@otobayi.com";
        }

        await db.SaveChangesAsync();
    }

    private static async Task FillMissingImagesAsync(AppDbContext db)
    {
        var missing = await db.Products
            .Where(p => p.ImagePath == null || p.ImagePath == "")
            .ToListAsync();
        if (missing.Count == 0)
            return;

        foreach (var product in missing)
            product.ImagePath = "/uploads/products/placeholder.svg";
        await db.SaveChangesAsync();
    }

    private static List<Category> CreateCategories() =>
    [
        new() { Name = "Fren Sistemi", Description = "Balata, disk, hortum", IsActive = true },
        new() { Name = "Filtreler", Description = "Yağ, hava, yakıt ve polen filtreleri", IsActive = true },
        new() { Name = "Ateşleme", Description = "Buji, bobin ve sensörler", IsActive = true },
        new() { Name = "Aydınlatma", Description = "Far, sinyal ve ampul", IsActive = true }
    ];

    private static List<Slider> CreateSliders() =>
    [
        new() { Title = "Bahar bakımı", Subtitle = "Fren setlerinde %15 bayi iskontosu.", ImagePath = "/uploads/sliders/kampanya.svg", LinkUrl = "/Products?term=Fren", DisplayOrder = 1, IsActive = true },
        new() { Title = "Öne çıkan: Yağ filtresi", Subtitle = "Mann W712/75 stoktan teslim.", ImagePath = "/uploads/sliders/one-cikan.svg", LinkUrl = "/Products?term=Mann", DisplayOrder = 2, IsActive = true },
        new() { Title = "Sevkiyat duyurusu", Subtitle = "İstanbul içi siparişler 24 saat içinde hazırlanır.", ImagePath = "/uploads/sliders/duyuru.svg", LinkUrl = "/Orders", DisplayOrder = 3, IsActive = true }
    ];

    private static List<ProductGridColumn> CreateGridColumns() =>
    [
        Col("ImagePath", "Görsel", GridRenderType.Image, 1, "72px", GridAlignment.Center, showMobile: false),
        Col("ProductCode", "Ürün Kodu", GridRenderType.Text, 2, "120px", GridAlignment.Left),
        Col("Name", "Ürün Adı", GridRenderType.Text, 3, null, GridAlignment.Left),
        Col("Brand", "Marka", GridRenderType.Text, 4, "130px", GridAlignment.Left, showMobile: false),
        Col("StockQuantity", "Stok", GridRenderType.StockBadge, 5, "100px", GridAlignment.Center),
        Col("Price", "Fiyat", GridRenderType.Currency, 6, "110px", GridAlignment.Right),
        Col("Quantity", "Adet", GridRenderType.QuantityInput, 7, "90px", GridAlignment.Center),
        Col("AddToCart", "İşlem", GridRenderType.AddToCartButton, 8, "120px", GridAlignment.Center)
    ];

    private static readonly CatalogRow[] Catalog =
    [
        new("CVT-M8-20", "FRN-BAL-ON", "Ön Fren Balatası", "Golf / Passat uyumlu seramik balata seti.", "Bosch", "5Q0698151", "GDB1732", "OEM", 120, 20, 890m, 0, "/uploads/products/cvt.svg"),
        new("SMN-M8", "FRN-DSC-280", "Fren Diski 280 mm", "Havalandırmalı ön disk, 280x22 mm.", "TRW", "DF4859", "280MM", "VENT", 80, 15, 1240m, 0, "/uploads/products/smn.svg"),
        new("RND-M8", "FRN-HOS-ON", "Ön Fren Hortumu", "ATE hidrolik hortum, EPDM.", "ATE", "330626", "HOSE", null, 4, 10, 285m, 0, "/uploads/products/rnd.svg"),
        new("KBL-3G15", "FLT-YAG-W712", "Yağ Filtresi W712/75", "Spin-on yağ filtresi, 3/4-16 UNF.", "Mann", "W712/75", "W712", "SPIN", 25, 8, 145m, 1, "/uploads/products/kbl.svg"),
        new("PRZ-16A", "FLT-HVA-LX", "Hava Filtresi", "Panel hava filtresi, 1.6 TSI.", "Mahle", "LX1780", "AIR", "TSI", 40, 10, 210m, 1, "/uploads/products/prz.svg"),
        new("SGT-10A", "FLT-YAK-KL", "Yakıt Filtresi", "Dizel hat filtre, 2.0 TDI.", "Bosch", "F026402048", "FUEL", "TDI", 2, 5, 320m, 1, "/uploads/products/sgt.svg"),
        new("ELD-5L", "ATS-BJU-FR7", "Buji FR7DCX", "Standart buji, 4'lü kutu.", "NGK", "FR7DCX", "SPARK", "4LU", 18, 6, 96m, 2, "/uploads/products/eld.svg"),
        new("ELD-MOP", "ATS-BOB-IG", "Ateşleme Bobini", "Kalem tip ateşleme bobini.", "Valeo", "245103", "COIL", null, 0, 4, 780m, 2, "/uploads/products/mop.svg"),
        new("A4-80G", "AYD-FAR-H7", "H7 Far Ampulü", "12V 55W, uzun ömür.", "Osram", "64210", "H7", "12V", 60, 12, 68m, 3, "/uploads/products/a4.svg"),
        new("KAL-MAVI", "AYD-SIN-LED", "LED Sinyal Lambası", "Canbus uyumlu sarı LED.", "Philips", "WY21W", "LED", "CAN", 9, 10, 185m, 3, "/uploads/products/kal.svg"),
        new("CVT-M10-30", "FRN-BAL-ARK", "Arka Fren Balatası", "Arka disk balata seti.", "Brembo", "P85075", "REAR", "OEM", 55, 15, 640m, 0, "/uploads/products/cvt10.svg"),
        new("KBL-3G25", "FLT-POL-CUK", "Polen Filtresi", "Aktif karbon kabin filtresi.", "Mann", "CUK26007", "CABIN", "CARB", 14, 5, 175m, 1, "/uploads/products/kbl25.svg")
    ];

    private static Product CreateProduct(CatalogRow row, int categoryId, bool resetStock)
    {
        var product = new Product { CategoryId = categoryId, IsActive = true, CreatedAt = DateTime.UtcNow };
        ApplyCatalog(product, row, resetStock);
        return product;
    }

    private static void ApplyCatalog(Product product, CatalogRow row, bool resetStock)
    {
        product.ProductCode = row.Code;
        product.Name = row.Name;
        product.Description = row.Description;
        product.Brand = row.Brand;
        product.ManufacturerCode = row.ManufacturerCode;
        product.CustomCode1 = row.Custom1;
        product.CustomCode2 = row.Custom2;
        product.ImagePath = row.Image;
        product.CriticalStockLevel = row.Critical;
        product.Price = row.Price;
        product.UpdatedAt = DateTime.UtcNow;
        if (resetStock)
            product.StockQuantity = row.Stock;
    }

    private static ProductGridColumn Col(
        string field, string header, GridRenderType type, int order, string? width, GridAlignment align, bool showMobile = true)
        => new()
        {
            FieldName = field,
            Header = header,
            RenderType = type,
            SortOrder = order,
            Width = width,
            Alignment = align,
            ShowOnDesktop = true,
            ShowOnTablet = true,
            ShowOnMobile = showMobile,
            IsVisible = true
        };

    private sealed record CatalogRow(
        string OldCode, string Code, string Name, string Description, string Brand,
        string ManufacturerCode, string Custom1, string? Custom2,
        int Stock, int Critical, decimal Price, int CategoryIndex, string Image);
}
