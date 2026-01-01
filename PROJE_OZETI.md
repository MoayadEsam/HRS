# Otel Rezervasyon Sistemi (HRS)
## Proje Özeti - MVC Projesi

---

## 📋 Proje Hakkında

Bu proje, modern bir otel rezervasyon ve yönetim sistemidir. ASP.NET Core MVC mimarisi kullanılarak geliştirilmiş olup, otel işletmelerinin tüm operasyonlarını tek bir platformdan yönetmelerini sağlar.

---

## 🛠️ Kullanılan Teknolojiler

| Teknoloji | Versiyon | Kullanım Amacı |
|-----------|----------|----------------|
| **ASP.NET Core MVC** | 9.0 | Web uygulama framework'ü |
| **Entity Framework Core** | 9.0 | ORM (Object-Relational Mapping) |
| **PostgreSQL** | 16 | İlişkisel veritabanı |
| **ASP.NET Identity** | 9.0 | Kimlik doğrulama ve yetkilendirme |
| **Bootstrap** | 5.3 | Responsive CSS framework |
| **jQuery** | 3.7 | JavaScript kütüphanesi |
| **Swiper.js** | 11 | Görsel karusel/slider |
| **QuestPDF** | - | PDF oluşturma |
| **AutoMapper** | 12 | DTO dönüşümleri |
| **DataTables** | 1.13 | Tablo filtreleme ve sayfalama |

---

## 🏗️ MVC Mimari Yapısı

### Model-View-Controller Deseni

```
┌─────────────────────────────────────────────────────────────┐
│                        KULLANICI                            │
└─────────────────────────────────────────────────────────────┘
                              │
                              ▼
┌─────────────────────────────────────────────────────────────┐
│                      CONTROLLER                             │
│  (HotelsController, ReservationsController, vb.)            │
│  - HTTP isteklerini karşılar                                │
│  - Servisler ile iletişim kurar                             │
│  - View'lara model gönderir                                 │
└─────────────────────────────────────────────────────────────┘
                    │                   │
                    ▼                   ▼
┌──────────────────────────┐   ┌──────────────────────────────┐
│         MODEL            │   │            VIEW              │
│  (Hotel, Room, vb.)      │   │  (Razor Views - .cshtml)     │
│  - Veri yapıları         │   │  - HTML/CSS/JS               │
│  - Business logic        │   │  - Kullanıcı arayüzü         │
│  - Validasyon kuralları  │   │  - Form'lar ve listeler      │
└──────────────────────────┘   └──────────────────────────────┘
```

---

## 📁 Katmanlı Mimari (N-Tier Architecture)

```
HRS/
├── HotelReservation.Core/          # 📦 Domain Layer
│   ├── Models/                     # Entity sınıfları
│   ├── DTOs/                       # Data Transfer Objects
│   └── Enums/                      # Enum tanımları
│
├── HotelReservation.Data/          # 💾 Data Access Layer
│   ├── Context/                    # DbContext
│   ├── Configurations/             # EF Fluent API yapılandırmaları
│   ├── Repositories/               # Repository pattern implementasyonu
│   └── Seed/                       # Veritabanı başlangıç verileri
│
├── HotelReservation.Services/      # ⚙️ Business Logic Layer
│   ├── Interfaces/                 # Servis arayüzleri
│   ├── Mappings/                   # AutoMapper profilleri
│   └── [Services]/                 # Servis implementasyonları
│
└── HotelReservation.Web/           # 🌐 Presentation Layer (MVC)
    ├── Controllers/                # MVC Controller'lar
    ├── Views/                      # Razor View'lar
    ├── wwwroot/                    # Statik dosyalar (CSS, JS, images)
    └── Resources/                  # Localization dosyaları
```

---

## 🎨 Kullanılan Tasarım Desenleri (Design Patterns)

| Desen | Uygulama |
|-------|----------|
| **Repository Pattern** | `HotelRepository`, `RoomRepository`, vb. |
| **Unit of Work** | `IUnitOfWork` ile transaction yönetimi |
| **Dependency Injection** | `Program.cs`'de servis kayıtları |
| **DTO Pattern** | `HotelCreateDto`, `ReservationListDto`, vb. |
| **Factory Pattern** | AutoMapper ile nesne dönüşümleri |

---

## 💾 Veritabanı Şeması (Entity Relationship)

### Ana Tablolar

| Tablo | Açıklama | İlişkiler |
|-------|----------|-----------|
| **Hotels** | Otel bilgileri | 1:N → Rooms, HotelImages |
| **Rooms** | Oda bilgileri | N:1 → Hotels, 1:N → Reservations |
| **Reservations** | Rezervasyonlar | N:1 → Rooms, Users |
| **AspNetUsers** | Kullanıcılar | 1:N → Reservations |
| **Expenses** | Giderler | N:1 → ExpenseCategories |
| **Incomes** | Gelirler | N:1 → Reservations |
| **Employees** | Personel | N:1 → Departments |
| **InventoryItems** | Envanter | N:1 → InventoryCategories |

---

## 👥 Kullanıcı Rolleri ve Yetkilendirme

### Role-Based Access Control (RBAC)

```csharp
// Controller seviyesinde yetkilendirme örneği
[Authorize(Roles = "Admin")]
public class AdminDashboardController : Controller
{
    // Sadece Admin rolündeki kullanıcılar erişebilir
}
```

| Rol | Yetkiler |
|-----|----------|
| **Admin** | Tüm sistem yönetimi, finans, personel, envanter, raporlar |
| **User** | Oda arama, rezervasyon yapma, kendi rezervasyonlarını görüntüleme |

---

## ✨ Temel Özellikler

### 🏨 Otel Yönetimi
- CRUD işlemleri (Create, Read, Update, Delete)
- **Çoklu resim yükleme** desteği
- Resim galerisi (Swiper.js karusel)
- Otel detay sayfası

### 🛏️ Oda Yönetimi
- Oda türleri ve fiyatlandırma
- Oda durumu takibi (Müsait, Dolu, Bakımda)
- Oda olanakları (Amenities)

### 📅 Rezervasyon Sistemi
- Online rezervasyon oluşturma
- Tarih seçimi ve müsaitlik kontrolü
- Rezervasyon durumları (Pending, Confirmed, Cancelled)
- **PDF fatura indirme** (QuestPDF)

### 💰 Finans Yönetimi
- Gelir kayıtları
- Gider kayıtları ve kategorileri
- Aylık/yıllık finansal raporlar
- Grafik görselleştirmeler

### 📦 Envanter Yönetimi
- Stok takibi
- Kategori bazlı envanter
- Stok giriş/çıkış hareketleri
- Düşük stok uyarıları

### 👨‍💼 Personel Yönetimi
- Çalışan kayıtları
- Departman yönetimi
- Maaş takibi
- Devamsızlık kayıtları

---

## 🌍 Çoklu Dil Desteği (Localization)

```csharp
// Program.cs - Localization yapılandırması
builder.Services.AddLocalization(options => options.ResourcesPath = "Resources");

var supportedCultures = new[] { "en-US", "tr-TR" };
```

- 🇹🇷 **Türkçe** (tr-TR)
- 🇬🇧 **İngilizce** (en-US)

Resource dosyaları: `SharedResource.resx`, `SharedResource.tr-TR.resx`

---

## 🔐 Güvenlik Özellikleri

| Özellik | Açıklama |
|---------|----------|
| **ASP.NET Identity** | Kullanıcı kimlik doğrulama |
| **Password Hashing** | BCrypt ile şifre hashleme |
| **Anti-Forgery Tokens** | CSRF koruması |
| **[Authorize]** | Controller/Action seviyesinde yetkilendirme |
| **SSL/HTTPS** | Şifreli iletişim |

---

## � Deployment (Canlıya Alma)

### Railway Platformu

```dockerfile
# Multi-stage Dockerfile
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
# Build aşaması

FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS runtime
# Runtime aşaması
```

- **Platform:** Railway
- **Database:** PostgreSQL (Railway managed)
- **Build:** Docker container

---

## 📊 Örnek Controller Kodu

```csharp
public class HotelsController : Controller
{
    private readonly IHotelService _hotelService;
    
    public HotelsController(IHotelService hotelService)
    {
        _hotelService = hotelService;  // Dependency Injection
    }
    
    public async Task<IActionResult> Index()
    {
        var hotels = await _hotelService.GetAllAsync();  // Async/Await
        return View(hotels);  // Model'i View'a gönder
    }
    
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(HotelCreateDto dto)
    {
        if (!ModelState.IsValid)  // Model Validation
            return View(dto);
            
        await _hotelService.CreateAsync(dto);
        return RedirectToAction(nameof(Index));
    }
}
```

---

## � Örnek Razor View Kodu

```html
@model HotelDetailsDto

<div class="card">
    <img src="@Model.ImageUrl" class="card-img-top" alt="@Model.Name">
    <div class="card-body">
        <h5 class="card-title">@Model.Name</h5>
        <p class="card-text">@Model.Description</p>
        
        @if (User.IsInRole("Admin"))
        {
            <a asp-action="Edit" asp-route-id="@Model.Id" class="btn btn-primary">
                @Localizer["Edit"]
            </a>
        }
    </div>
</div>
```

---

## � Varsayılan Giriş Bilgileri

| Rol | E-posta | Şifre |
|-----|---------|-------|
| **Admin** | admin@hotel.com | Admin123! |
| **User** | user@hotel.com | User123! |

---

## 📚 Kaynaklar

- [ASP.NET Core Documentation](https://docs.microsoft.com/aspnet/core)
- [Entity Framework Core](https://docs.microsoft.com/ef/core)
- [Bootstrap 5](https://getbootstrap.com)
- [Railway Deployment](https://railway.app)

---

*Bu proje, ASP.NET Core MVC dersi için hazırlanmıştır.*
