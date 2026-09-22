using System.ComponentModel.DataAnnotations;
using MiniB2B.Domain.Enums;

namespace MiniB2B.Web.Areas.Admin.Models;

public class ProductFormViewModel
{
    public int Id { get; set; }

    [Required(ErrorMessage = "Ürün kodu zorunludur.")]
    [StringLength(50, ErrorMessage = "Ürün kodu en fazla 50 karakter olabilir.")]
    [Display(Name = "Ürün kodu")]
    public string ProductCode { get; set; } = string.Empty;

    [Required(ErrorMessage = "Ürün adı zorunludur.")]
    [StringLength(200, ErrorMessage = "Ürün adı en fazla 200 karakter olabilir.")]
    [Display(Name = "Ürün adı")]
    public string Name { get; set; } = string.Empty;

    [StringLength(2000, ErrorMessage = "Açıklama en fazla 2000 karakter olabilir.")]
    [Display(Name = "Açıklama")]
    public string? Description { get; set; }

    [StringLength(100, ErrorMessage = "Marka en fazla 100 karakter olabilir.")]
    [Display(Name = "Marka")]
    public string? Brand { get; set; }

    [StringLength(80, ErrorMessage = "Üretici kodu en fazla 80 karakter olabilir.")]
    [Display(Name = "Üretici kodu")]
    public string? ManufacturerCode { get; set; }

    [StringLength(80, ErrorMessage = "Özel kod 1 en fazla 80 karakter olabilir.")]
    [Display(Name = "Özel kod 1")]
    public string? CustomCode1 { get; set; }

    [StringLength(80, ErrorMessage = "Özel kod 2 en fazla 80 karakter olabilir.")]
    [Display(Name = "Özel kod 2")]
    public string? CustomCode2 { get; set; }

    public string? ImagePath { get; set; }

    [Display(Name = "Görsel")]
    public IFormFile? ImageFile { get; set; }

    [Range(0, int.MaxValue, ErrorMessage = "Stok negatif olamaz.")]
    [Display(Name = "Stok miktarı")]
    public int StockQuantity { get; set; }

    [Range(0, int.MaxValue, ErrorMessage = "Kritik stok negatif olamaz.")]
    [Display(Name = "Kritik stok seviyesi")]
    public int CriticalStockLevel { get; set; } = 5;

    [Range(0.01, double.MaxValue, ErrorMessage = "Fiyat 0'dan büyük olmalıdır.")]
    [Display(Name = "Fiyat")]
    public decimal Price { get; set; }

    [Range(1, int.MaxValue, ErrorMessage = "Kategori seçiniz.")]
    [Display(Name = "Kategori")]
    public int CategoryId { get; set; }

    [Display(Name = "Aktif")]
    public bool IsActive { get; set; } = true;
}

public class UserFormViewModel
{
    public int Id { get; set; }

    [Required(ErrorMessage = "Ad zorunludur.")]
    [Display(Name = "Ad")]
    public string FirstName { get; set; } = string.Empty;

    [Required(ErrorMessage = "Soyad zorunludur.")]
    [Display(Name = "Soyad")]
    public string LastName { get; set; } = string.Empty;

    [Required(ErrorMessage = "E-posta zorunludur.")]
    [EmailAddress(ErrorMessage = "Geçerli bir e-posta giriniz.")]
    [Display(Name = "E-posta")]
    public string Email { get; set; } = string.Empty;

    [Required(ErrorMessage = "Telefon zorunludur.")]
    [Display(Name = "Telefon")]
    public string Phone { get; set; } = string.Empty;

    [Required(ErrorMessage = "Kullanıcı adı zorunludur.")]
    [MinLength(3, ErrorMessage = "Kullanıcı adı en az 3 karakter olmalıdır.")]
    [Display(Name = "Kullanıcı adı")]
    public string Username { get; set; } = string.Empty;

    [DataType(DataType.Password)]
    [Display(Name = "Yeni şifre (boş bırakılırsa değişmez)")]
    public string? Password { get; set; }

    [Display(Name = "Rol")]
    public UserRole Role { get; set; }

    [Display(Name = "Aktif")]
    public bool IsActive { get; set; } = true;
}

public class SliderFormViewModel
{
    public int Id { get; set; }

    [Required(ErrorMessage = "Başlık zorunludur.")]
    [Display(Name = "Başlık")]
    public string Title { get; set; } = string.Empty;

    [Display(Name = "Alt başlık")]
    public string? Subtitle { get; set; }

    public string? ImagePath { get; set; }

    [Display(Name = "Görsel")]
    public IFormFile? ImageFile { get; set; }

    [Display(Name = "Bağlantı")]
    public string? LinkUrl { get; set; }

    [Range(0, int.MaxValue, ErrorMessage = "Sıra negatif olamaz.")]
    [Display(Name = "Sıra")]
    public int DisplayOrder { get; set; }

    [Display(Name = "Aktif")]
    public bool IsActive { get; set; } = true;
}

public class GridColumnFormViewModel
{
    public int Id { get; set; }

    [Required(ErrorMessage = "Alan adı zorunludur.")]
    [Display(Name = "Alan")]
    public string FieldName { get; set; } = string.Empty;

    [Required(ErrorMessage = "Başlık zorunludur.")]
    [Display(Name = "Başlık")]
    public string Header { get; set; } = string.Empty;

    [Display(Name = "Gösterim tipi")]
    public GridRenderType RenderType { get; set; }

    [Range(0, int.MaxValue, ErrorMessage = "Sıra negatif olamaz.")]
    [Display(Name = "Sıra")]
    public int SortOrder { get; set; }

    [Display(Name = "Genişlik")]
    public string? Width { get; set; }

    [Display(Name = "Hizalama")]
    public GridAlignment Alignment { get; set; }

    [Display(Name = "Masaüstü")]
    public bool ShowOnDesktop { get; set; } = true;

    [Display(Name = "Tablet")]
    public bool ShowOnTablet { get; set; } = true;

    [Display(Name = "Telefon")]
    public bool ShowOnMobile { get; set; } = true;

    [Display(Name = "Görünür")]
    public bool IsVisible { get; set; } = true;
}
