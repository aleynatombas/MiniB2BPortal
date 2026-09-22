# Mini B2B Portal (OtoBayi)

Küçük ölçekli B2B e-ticaret uygulaması. Vitrin konsepti **oto yedek parça bayi portalı**: servisler fren, filtre, ateşleme ve aydınlatma parçası arar, sepete ekler ve sipariş verir; yöneticiler katalog, kullanıcı, sipariş durumu, ana sayfa slider’ı ve ürün grid kolonlarını yönetir.

Ürün görselleri şimdilik yer tutucu; gerçek fotoğraflar sonra `wwwroot/uploads/products` ve `wwwroot/uploads/sliders` altına konur.

## Teknolojiler

| Katman | Tercih |
| --- | --- |
| Uygulama | ASP.NET Core MVC (Razor), .NET 10 |
| Mimari | N-Tier: Domain / DataAccess / Business / Web |
| ORM | Entity Framework Core 10 (code-first) |
| Veritabanı | SQL Server Express (`.\SQLEXPRESS`) |
| Kimlik | Cookie authentication, `Admin` / `Customer` rolleri |
| Şifre | BCrypt (düz metin tutulmaz) |
| Arama | SQL tarafında `IQueryable` (bellekte filtre yok) |

## Mimari

```
Browser → MiniB2B.Web → MiniB2B.Business → MiniB2B.DataAccess → SQL Server
```

- **Domain:** entity ve enum
- **DataAccess:** EF Core, ilişkiler, repository, transaction / atomik stok düşümü
- **Business:** iş kuralları, DTO, validasyon, seed
- **Web:** Razor ekranlar, cookie auth, dosya yükleme

Sipariş kaleminde ürün kodu, ad, adet ve birim fiyat **sipariş anında kopyalanır**. Ürün fiyatı sonra değişse eski sipariş aynı kalır.

## Kurulum

1. SQL Server Express çalışıyor olmalı (`.\SQLEXPRESS`).
2. Bağlantı cümlesi `src/MiniB2B.Web/appsettings.json`:

```json
"ConnectionStrings": {
  "DefaultConnection": "Server=.\\SQLEXPRESS;Database=MiniB2BPortal;Trusted_Connection=True;TrustServerCertificate=True;MultipleActiveResultSets=true"
}
```

3. Çalıştırma:

```bash
dotnet restore
dotnet run --project src/MiniB2B.Web
```

İlk çalışmada migration uygulanır ve boş veritabanına seed yazılır.

Uygulama adresi: `http://localhost:5193`

### SQL script ile kurulum

`database/MiniB2BPortal.sql` şemayı oluşturur.

```sql
CREATE DATABASE MiniB2BPortal;
```

Ardından script’i `MiniB2BPortal` üzerinde çalıştırın. Örnek veriler yine uygulamanın ilk açılışındaki seed ile gelir.

## Varsayılan kullanıcılar

| Rol | Kullanıcı adı | Şifre | E-posta |
| --- | --- | --- | --- |
| Admin | `admin` | `Admin123!` | admin@otobayi.com |
| Bayi | `bayi` | `Bayi123!` | bayi@otobayi.com |

Yeni bayi **Kayıt ol** ekranından hesap açabilir.

## Ne yapılır

**Bayi:** kayıt/giriş, ana sayfa slider, dinamik ürün grid, SQL arama, ürün popup, sepete ekleme, sepet güncelle/sil, sipariş oluştur (stok backend’de kontrol edilir), Siparişlerim.

**Yönetici:** ürün ve kullanıcı, sipariş onay/red, slider, grid kolon ayarı (alan, başlık, sıra, render tipi, cihaz görünürlüğü — kod değişikliği gerekmez).

Stok göstergesi ürünün `CriticalStockLevel` değerine göre: Var / Kritik / Yok.

Yetersiz stokta sipariş oluşmaz: *“Ürün A için yeterli stok bulunmamaktadır. Mevcut stok: 5.”*
