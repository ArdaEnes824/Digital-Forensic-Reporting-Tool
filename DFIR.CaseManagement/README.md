# DFIR Case Management System

Dijital adli bilişim soruşturmalarını yönetmek için geliştirilmiş **ASP.NET Core 8** tabanlı web uygulaması.  
Bilgisayar Mühendisliği OOP dersi kapsamında hazırlanmıştır.

---

## Teknoloji Yığını

| Katman | Teknoloji |
|---|---|
| Backend | ASP.NET Core 8 Web API (C#) |
| ORM | Entity Framework Core 8 + SQL Server |
| Kimlik Doğrulama | JWT Bearer + Refresh Token (HMAC-SHA256) |
| Şifreleme | PBKDF2 (PasswordHasher) |
| PDF Rapor | QuestPDF |
| Excel Rapor | ClosedXML |
| Doğrulama | FluentValidation |
| Frontend | Bootstrap 5 + Chart.js (CDN), Vanilla JS |
| API Belgesi | Swagger / OpenAPI |

---

## Uygulanan OOP Kavramları ve Tasarım Desenleri

| Kavram / Desen | Uygulama Yeri |
|---|---|
| **Encapsulation** | `MalwareAnalysis` — RiskScore/EntropyScore private setter, yalnızca `RunAnalysis()` üzerinden değişir |
| **Inheritance** | `User → Admin / Analyst / Viewer` (TPH), `BaseEntity` |
| **Abstraction** | `IHashable`, `IAnalyzable`, `IReportGenerator` arayüzleri |
| **Polymorphism** | `IReportGenerator` → PDF / HTML / Excel runtime seçimi |
| **Repository Pattern** | `IGenericRepository<T>` + özelleşmiş repo'lar |
| **Unit of Work** | `IUnitOfWork` / `UnitOfWork` |
| **Service Layer** | `ICaseService`, `IEvidenceService`, vb. |
| **Strategy Pattern** | `PdfReportGenerator`, `HtmlReportGenerator`, `ExcelReportGenerator` |
| **Observer Pattern** | `CaseEventPublisher`, `AuditLogObserver`, `NotificationObserver` |
| **Dependency Injection** | `Program.cs` üzerinden tüm servisler |

---

## Proje Klasör Yapısı

```
DFIR.CaseManagement/
├── Auth/            JWT ayarları, token servisi, refresh token store, izinler
├── Controllers/     7 controller (Auth, Cases, Evidence, Custody, Malware, Report, Dashboard)
├── Data/            AppDbContext, DbSeeder, SyntheticDataSeeder
├── DTOs/            Veri transfer objeleri
├── Entities/        Domain modelleri (User, Case, Evidence, vb.) + Enum'lar
├── Interfaces/      Tüm soyutlamalar
├── Middleware/      GlobalExceptionMiddleware, RequestLoggingMiddleware
├── Migrations/      EF Core migration'ları
├── Repositories/    GenericRepository, özelleşmiş repo'lar, UnitOfWork
├── Services/        Servis katmanı
│   ├── Observers/   Observer Pattern (publisher + gözlemciler)
│   └── Strategies/  Strategy Pattern (rapor üreticileri)
├── Validators/      FluentValidation doğrulayıcıları
└── wwwroot/         Frontend (HTML / CSS / JS)
    ├── css/site.css
    └── js/ api.js, app.js
```

---

## Kurulum ve Çalıştırma

### Ön Gereksinimler

- .NET 8 SDK
- SQL Server (SSMS 21 veya Express)

### Adımlar

```bash
# Projeyi klonla / aç
cd "DFIR.CaseManagement"

# Connection string'i düzenle (appsettings.json)
# "Server=LAPTOP-XXXX\MSSQLSERVER01" kısmını kendi instance adınla değiştir

# Çalıştır — migrasyon ve seed otomatik yapılır
dotnet run
```

Uygulama başladığında:
- Veritabanı otomatik oluşturulur
- Tablolar migrate edilir
- Varsayılan kullanıcılar eklenir
- Demo vakalar ve deliller eklenir

### Erişim

| URL | Açıklama |
|---|---|
| `http://localhost:5293` | Frontend (Login sayfası) |
| `http://localhost:5293/swagger` | Swagger API belgesi |

> Port numarası `launchSettings.json`'a göre değişebilir.

---

## Kullanıcı Hesapları

Uygulama ilk çalıştığında aşağıdaki hesaplar otomatik olarak veritabanına eklenir:

| Kullanıcı Adı | E-posta | Parola | Rol | Yetkiler |
|---|---|---|---|---|
| `admin` | admin@dfir.local | `Admin123!` | Admin | Tüm yetkiler |
| `analyst` | analyst@dfir.local | `Analyst123!` | Analyst | Okuma + Yazma (silme hariç) |
| `viewer` | viewer@dfir.local | `Viewer123!` | Viewer | Yalnızca okuma |

### Rol İzin Tablosu

| İzin | Admin | Analyst | Viewer |
|---|---|---|---|
| cases:write | ✅ | ✅ | ❌ |
| cases:delete | ✅ | ❌ | ❌ |
| evidence:write | ✅ | ✅ | ❌ |
| evidence:delete | ✅ | ❌ | ❌ |
| custody:write | ✅ | ✅ | ❌ |
| malware:write | ✅ | ✅ | ❌ |
| reports:generate | ✅ | ✅ | ❌ |
| users:manage | ✅ | ❌ | ❌ |

---

## Sentetik (Demo) Veriler

Uygulama ilk açıldığında aşağıdaki kurgusal veriler otomatik olarak eklenir:

### Vakalar (8 Adet)

| Vaka No | Başlık | Durum | Öncelik |
|---|---|---|---|
| DFIR-2025-001 | Kurumsal Ağ Sızma Olayı | InProgress | Critical |
| DFIR-2025-002 | Fidye Yazılımı Saldırısı - Üretim Tesisi | InProgress | Critical |
| DFIR-2025-003 | İçeriden Veri Sızdırma - İK Veritabanı | Closed | High |
| DFIR-2025-004 | Phishing Kampanyası - Yönetici Hesapları | Open | High |
| DFIR-2025-005 | Web Uygulama Güvenlik İhlali | InProgress | Critical |
| DFIR-2025-006 | Mobil Cihaz Adli Analizi - Suç Soruşturması | InProgress | Medium |
| DFIR-2025-007 | Kripto Madencilik Botnet Tespiti | Closed | Medium |
| DFIR-2025-008 | Tedarik Zinciri Saldırısı - Yazılım Güncellemesi | Open | Critical |

### Deliller (14 Adet)

Dell PowerEdge sunucu, Cisco Catalyst ağ cihazı, Lenovo ThinkPad, HP EliteBook, Siemens SCADA HMI, SanDisk USB bellek, Apple iPhone, Samsung Galaxy, VMware VMDK, IBM Power System ve diğerleri.

### Malware Analizleri (8 Adet)

| Dosya | Risk | Seviye |
|---|---|---|
| lockbit3_dropper.exe | 92.4 | Critical |
| update_installer_v4.2.1.msi | 87.1 | Critical |
| xmrig_miner_svc.dll | 78.5 | High |
| keylogger_chrome_ext.crx | 71.3 | High |
| spearphish_macro.xlsm | 58.7 | Medium |
| sqli_webshell.php | 62.1 | Medium |
| wifi_capture_util.exe | 34.2 | Low |
| system_health_check.sh | 8.5 | Clean |

---

## API Endpoint Özeti

### Auth (`/api/auth`)
- `POST /login` — Giriş, JWT + refresh token döner
- `POST /refresh` — Refresh token ile yeni token çifti
- `GET /me` — Mevcut kullanıcı bilgisi
- `POST /register` — Yeni kullanıcı (Admin only)
- `GET /users` — Kullanıcı listesi (Admin only)
- `DELETE /users/{id}` — Kullanıcı sil (Admin only)

### Cases (`/api/cases`)
`GET /` · `GET /{id}` · `POST /` · `PUT /{id}` · `DELETE /{id}` · `PATCH /{id}/stage`

### Evidence (`/api/evidence`)
`GET /` · `GET /case/{id}` · `GET /{id}` · `POST /` · `PUT /{id}` · `DELETE /{id}` · `POST /{id}/verify`

### Diğer
- `/api/custody` — Zincir kayıtları
- `/api/malware` — Malware analizi
- `/api/reports/case/{id}/generate?format=pdf|xlsx|html` — Rapor export
- `/api/dashboard` — İstatistikler

---

## Swagger'da JWT Kullanımı

1. `POST /api/auth/login` ile giriş yap
2. Dönen `accessToken` değerini kopyala
3. Swagger sayfasında sağ üst **"Authorize"** butonuna tıkla
4. Token'ı yapıştır → **Authorize**
5. Artık tüm korumalı endpoint'leri test edebilirsin

---

## Lisans

Bu proje eğitim amaçlı geliştirilmiştir. Tüm veriler kurgusaldır.
