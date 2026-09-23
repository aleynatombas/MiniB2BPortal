# Mini B2B Portal (OtoTedarik)

Küçük ölçekli B2B e-ticaret uygulaması. Vitrin **oto yedek parça bayi portalı**: kullanıcılar (servis / bayi) parça arar, sepete ekler ve sipariş verir; yönetici ürün, kullanıcı ve sipariş yönetir.

Bu depo, ödev teslimi için gerekenleri içerir:

1. Kaynak kod (`src/`)
2. Veritabanı oluşturma: EF Core migration ve isteğe bağlı SQL script
3. Projenin nasıl çalıştırılacağını anlatan bu README
4. Kullanılan teknolojiler
5. Mimari / teknik tercihlerin kısa açıklaması

Ürün, slider ve marka görselleri `src/MiniB2B.Web/wwwroot/uploads` altındadır.

---

## Kullanılan teknolojiler

| Katman | Tercih |
| --- | --- |
| Dil / çalışma zamanı | C#, .NET 10 |
| Web | ASP.NET Core MVC (Razor) |
| Mimari | N-Tier: Domain / DataAccess / Business / Web |
| ORM | Entity Framework Core 10 (code-first) |
| Veritabanı | SQL Server Express (`.\SQLEXPRESS`) |
| Kimlik | Cookie authentication, `Admin` / `Customer` rolleri |
| Şifre | BCrypt (veritabanında açık metin tutulmaz) |
| Arama | SQL tarafında `IQueryable` (bellekte filtre yok) |

Gereksinim: [.NET 10 SDK](https://dotnet.microsoft.com/download) ve çalışan bir SQL Server Express örneği.

---

## Mimari ve teknik tercihler

```
Tarayıcı → MiniB2B.Web → MiniB2B.Business → MiniB2B.DataAccess → SQL Server
```

| Proje | Görev |
| --- | --- |
| `src/MiniB2B.Domain` | Entity ve enum |
| `src/MiniB2B.DataAccess` | EF Core, ilişkiler (PK / FK / unique), repository, transaction, atomik stok düşümü |
| `src/MiniB2B.Business` | İş kuralları, DTO, validasyon, seed |
| `src/MiniB2B.Web` | Razor ekranlar, cookie auth, dosya yükleme |

**Rol ayrımı:** Kullanıcı girişi vitrine (katalog, sepet, sipariş) gider. Yönetici girişi yönetim paneline (ürün, kullanıcı, sipariş) gider. Kayıt yalnızca kullanıcı hesabı açar.

**Sipariş anı kopyası:** Sipariş kaleminde ürün kodu, ad, adet ve birim fiyat sipariş oluşturulurken kopyalanır. Ürün fiyatı sonra değişse eski sipariş aynı kalır.

**Stok:** Sipariş backend’de kontrol edilir ve atomik düşülür. Yetersiz stokta sipariş oluşmaz.

**Ürün grid:** Katalog satırları `ProductGridColumns` tablosundan okunur (alan, sıra, render tipi, genişlik, hizalama, cihaz görünürlüğü). Kod değiştirmeden yönetilebilir.

**Görseller:** Yönetici ürün / slider görselini yükler; dosyalar `wwwroot/uploads` altında saklanır, yol veritabanına yazılır.

Çözüm dosyası (`.sln`) yoktur. Uygulama Web projesinden çalıştırılır.

Ödev minimum tabloların üzerine ek tablo, alan ve yardımcı yapı eklemeye izin verir. Aşağıdakiler bu tercihlerdir.

### Minimum tablolar

| Tablo | Karşılık |
| --- | --- |
| Kullanıcılar | `Users` |
| Ürünler | `Products` |
| Kategoriler | `Categories` |
| Sepet | `Carts` |
| Sepet ürünleri | `CartItems` |
| Siparişler | `Orders` |
| Sipariş kalemleri | `OrderItems` (sipariş anındaki kod, ad, adet, birim fiyat, satır tutarı) |

### Ek tablolar

| Tablo | Neden |
| --- | --- |
| `ProductGridColumns` | Ürün listesi kolonları veritabanından yönetilir (alan, sıra, render tipi, genişlik, hizalama, masaüstü / tablet / telefon). |
| `Sliders` | Ana sayfa kampanya / öne çıkan / duyuru slaytları yönetim panelinden yönetilir. |

### Ek alanlar

| Yer | Alan | Neden |
| --- | --- | --- |
| `Products` | `CriticalStockLevel` | Stok rozeti ürün bazında Var / Kritik / Yok |
| `Products` | `CategoryId`, `IsActive` | Katalog gruplama ve pasifleştirme |
| `Users` | `Role`, `IsActive`, `PasswordHash` | Yönetici / kullanıcı ayrımı, pasif hesap, BCrypt |
| `Orders` | `Status` (`Pending` / `Approved` / `Rejected`) | Beklemede + onay / red |

### Ek ekran / uç nokta

| Yer | Ne işe yarar |
| --- | --- |
| `/Admin/Dashboard` | Ürün, kullanıcı, sipariş özeti |
| `/Admin/Sliders` | Slider ekle / düzenle |
| `/Admin/GridColumns` | Grid kolonlarını kod değiştirmeden ayarla |
| `/Admin/Products/SetActive` | Ürünü pasifleştir / yeniden yayınla |
| `/Products/Details?partial=true` | Ürün detayı popup |
| `/Account/Login?portal=customer` veya `admin` | Kullanıcı ve yönetici girişi ayrı |

---

## Kurulum ve çalıştırma

1. SQL Server Express çalışıyor olmalı (`.\SQLEXPRESS`).
2. Bağlantı cümlesi `src/MiniB2B.Web/appsettings.json`:

```json
"ConnectionStrings": {
  "DefaultConnection": "Server=.\\SQLEXPRESS;Database=MiniB2BPortal;Trusted_Connection=True;TrustServerCertificate=True;MultipleActiveResultSets=true"
}
```

Farklı bir SQL Server kullanıyorsanız yalnızca `Server=` değerini değiştirin.

3. Depo kökünden:

```bash
dotnet restore
dotnet run --project src/MiniB2B.Web --urls http://localhost:5193
```

İlk çalışmada EF Core migration uygulanır (`src/MiniB2B.DataAccess/Migrations/20260920201310_InitialCreate.cs`) ve boş veritabanına örnek veri yazılır.

Adres: http://localhost:5193

### Alternatif: SQL script

`database/MiniB2BPortal.sql` şemayı oluşturur.

```sql
CREATE DATABASE MiniB2BPortal;
```

Script’i `MiniB2BPortal` üzerinde çalıştırın. Örnek kullanıcı ve katalog, uygulamanın ilk açılışındaki seed ile gelir.

---

## Varsayılan kullanıcılar

| Rol | Kullanıcı adı | Şifre | E-posta | Giriş sonrası |
| --- | --- | --- | --- | --- |
| Yönetici | `admin` | `Admin123!` | admin@otobayi.com | Yönetim paneli |
| Kullanıcı | `bayi` | `Bayi123!` | bayi@otobayi.com | Ana sayfa / vitrin |

Yeni kullanıcı **Kayıt ol** ekranından hesap açabilir. Giriş sayfasında **Kullanıcı girişi** ve **Yönetici girişi** ayrıdır.

---

## Ne yapılır

**Kullanıcı:** kayıt veya kullanıcı girişinden sonra ana sayfaya gider. Slider, dinamik ürün grid, SQL arama (ad, kod, marka, üretici kodu, açıklama, özel kodlar), ürün detayı (popup + tam sayfa), sepete ekleme, sepet güncelle / sil, sipariş oluştur, Siparişlerim.

**Yönetici:** yönetici girişinden sonra panele gider; alışveriş yapmaz. Ürün ekle / düzenle / ara / pasifleştir, kullanıcı listele / görüntüle / düzenle, sipariş onayla veya reddet, slider ve grid kolonlarını yönetir.

Stok göstergesi ürüne özel `CriticalStockLevel` değerine göre: **Var** / **Kritik** / **Yok**.

Yetersiz stokta sipariş oluşmaz: *“Ürün A için yeterli stok bulunmamaktadır. Mevcut stok: 5.”*
