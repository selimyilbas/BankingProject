## VakıfBank Banking Application

Modern bir full-stack bankacılık uygulaması. Çoklu para birimli hesaplar, para yatırma/transfer, döviz kurları ve temiz mimari ile uçtan uca bir örnek yapı sunar.

- .NET 8 (Clean Architecture, DDD, Repository + Unit of Work)
- Angular 17+ (Standalone Components – NgModule yok)
- SQL Server 2022 (Docker)

---

### İçindekiler

- **Özellikler ve Kurallar**
- **Mimari ve Proje Yapısı**
- **Kurulum ve Çalıştırma**
- **Konfigürasyon (appsettings, CORS, dış servisler)**
- **Domain Kuralları**
- **API Tasarımı (ApiResponse) ve Uç Noktalar**
- **Frontend Yapısı**
- **Test Kullanıcıları**
- **Geliştirme Notları ve Kısıtlar**
- **English Version (at the bottom)**

---

## Özellikler ve Kurallar

- **Müşteri Yönetimi**: Kayıt, güncelleme, TCKN ile arama, hesaplarla birlikte görüntüleme
- **Hesaplar (TL, EUR, USD)**: Otomatik hesap numarası, bakiye görüntüleme, durum değiştirme
- **İşlemler**: Para yatırma ve işlem geçmişi, tarih aralığı ve sayfalama
- **Transfer**: Hesap kimliği veya hesap numarasıyla; döviz dönüşümlü
- **Döviz Kurları**: Anlık kurlar, opsiyonel VakıfBank sandbox entegrasyonu
- **Standart Yanıt**: Tüm endpoint’ler `ApiResponse<T>` döner (JSON anahtarları lowercase: `success`, `message`, `data`, `errors`)

Notlar:
- Şifreler test amaçlı düz metin olarak saklanır. `Encryption:Key` verilirse AES‑GCM ile şifreleme etkinleşir ve girişte “lazy migration” yapılır.
- CORS, Angular `http://localhost:4200` için açıktır.

---

## Mimari ve Proje Yapısı

```
BankingProject/
├── BankingApp/                      # Backend (.NET)
│   ├── BankingApp.API/             # Web API
│   │   ├── Controllers/            # Auth, Customer, Account, Transaction, Transfer, ExchangeRate
│   │   ├── Program.cs              # Giriş noktası (Swagger, CORS, DI)
│   │   └── appsettings.json        # Konfigürasyon
│   ├── BankingApp.Application/     # Uygulama katmanı (DTO, Services, Mappings)
│   ├── BankingApp.Domain/          # Domain (Entities, Interfaces)
│   ├── BankingApp.Infrastructure/  # EF Core, DbContext, Repositories
│   └── BankingApp.Common/          # Ortak bileşenler
├── BankingApp.UI/                  # Frontend (Angular 17+)
│   └── src/app/{components,services,models}
├── database/                       # SQL betikleri
└── docker-compose.yml              # SQL Server 2022 (1433)
```

Teknik başlıklar:
- Swagger sadece Development’ta aktiftir (`http://localhost:5115/swagger`).
- AutoMapper profilleri `Application` katmanında tanımlıdır.
- Repository + Unit of Work deseni `Infrastructure` içinde uygulanır.

---

## Kurulum ve Çalıştırma

Önkoşullar:
- .NET 8 SDK, Node.js 18+, Docker Desktop

1) Veritabanını başlatın (proje kök dizininden)
```bash
docker-compose up -d
```

2) Backend’i çalıştırın
```bash
cd BankingApp/BankingApp.API
dotnet restore
dotnet run
```
API: `http://localhost:5115` (Swagger: `/swagger`)

3) Frontend’i çalıştırın (ayrı bir terminalde, proje kökünden)
```bash
cd BankingApp.UI
npm install
ng serve --open
```
UI: `http://localhost:4200`

Docker SQL Server konteyner adı: `banking-sqlserver` (port `1433`).

---

## Konfigürasyon

`BankingApp/BankingApp.API/appsettings.json` önemli alanlar:

- ConnectionStrings.DefaultConnection:
```
Server=localhost,1433;Database=BankingDB;User Id=sa;Password=Selim@123456789;TrustServerCertificate=True;MultipleActiveResultSets=true
```
- CORS: `Program.cs` içinde `http://localhost:4200` kökenine izin verilir.
- Encryption (opsiyonel):
  - `Encryption:Key` (Base64) ve `Encryption:Version` (örn. `v1`)
  - Anahtar girilirse `AesEncryptionService` kaydedilir ve login sırasında düz metin şifreler şifrelenir.
- Dış Servisler:
  - `VakifbankApi`: `BaseUrl`, `TokenUrl`, `ClientId`, `ClientSecret`, `UseVakifbankForFx` (varsayılan: false)
  - `ExchangeRateApi:ApiKey`: Üçüncü parti kur servisi anahtarı

Frontend API tabanı: `BankingApp.UI/src/app/services/api.ts` içinde `http://localhost:5115/api`.

---

## Domain Kuralları

- Para birimleri: yalnızca TL, EUR, USD
- Hesap numarası başlangıçları: TL → 1, EUR → 2, USD → 3
- TCKN: 11 haneli doğrulama (öğrenme amaçlı basit kontrol)

---

## API Tasarımı ve Standart Yanıt

Tüm endpoint’ler `ApiResponse<T>` döndürür. JSON anahtarları lowercase’dır.

```json
{
  "success": true,
  "message": "Operation successful",
  "data": { /* ... */ },
  "errors": []
}
``;

---

## Uç Noktalar (Özet)

Auth (`/api/auth`)
- POST `login`
- POST `register`

Customer (`/api/customer`)
- POST `` (oluştur)
- GET `{customerId}`
- GET `by-number/{customerNumber}`
- GET `by-tckn/{tckn}`
- GET `` (sayfalı liste; `pageNumber`, `pageSize`)
- GET `{customerId}/with-accounts`
- PUT `{customerId}`
- POST `{customerId}/change-password`
- POST `validate-tckn`

Account (`/api/account`)
- POST `` (Create)
- GET `{accountId}`
- GET `by-number/{accountNumber}`
- GET `customer/{customerId}`
- GET `balance/{accountNumber}`
- PUT `{accountId}/status` (aktif/pasif)

Transaction (`/api/transaction`)
- POST `deposit`
- GET `account/{accountId}`
- GET `account/{accountId}/date-range?startDate=...&endDate=...`
- GET `account/{accountId}/paged?pageNumber=1&pageSize=10`

Transfer (`/api/transfer`)
- POST `` (kimliklerle)
- POST `by-account-number` (numaralarla)
- GET `account/{accountId}`
- GET `customer/{customerId}`
- GET `{transferId}`
- POST `validate`
- POST `validate/by-account-number`

ExchangeRate (`/api/exchangerate`)
- GET `current?skipCache=false`
- GET `rate?fromCurrency=USD&toCurrency=TRY&skipCache=false`
- GET `vakifbank/today`
- POST `update`

Not: Swagger’dan tüm şema ve örnekleri inceleyebilirsiniz.

---

## Frontend Yapısı (Angular 17+)

- Standalone bileşenler (NgModule yok). Her bileşen `standalone: true`.
- Servisler: `src/app/services/*` (örn. `api.ts`, `account.service.ts`, `transfer.service.ts`)
- Modeller: `src/app/models/*` (örn. `api-response.model.ts`)
- API tabanı: `http://localhost:5115/api`

---

## Test Kullanıcıları

| TCKN | Şifre | Ad |
|------|-------|----|
| 12345678901 | 123456 | Ahmet Yılmaz |
| 98765432109 | 123456 | Ayşe Kaya |
| 11111111111 | 123456 | Test User |

---

## Geliştirme Notları ve Kısıtlar

- Bu proje öğrenme/test amaçlıdır; güvenlik basitleştirilmiştir.
- Şifre saklama düz metindir (opsiyonel AES şifreleme mevcuttur). Üretimde uygun hashing kullanılmalıdır.
- HTTPS, hız sınırlama, girdi doğrulama ve hataya dayanıklılık üretimde zorunludur.
- Rol bazlı yetkilendirme henüz eklenmemiştir.
- Döviz kurları servisinde önbellek ve geri dönüş mekanizması bulunur; `UseVakifbankForFx=false` varsayılanıdır.

---

### Ekran Görüntüleri (Örnek/Placeholder)

- Dashboard görünümü (placeholder)

![Dashboard](BankingApp.UI/src/assets/images/vakifbank-logo.jpg)

- Transfer görünümü (placeholder)

![Transfer](BankingApp.UI/src/assets/images/vakifbank-logo-sari-zemin.jpg)

Not: Proje ilerledikçe gerçek ekran görüntüleri ile güncellenecektir.

---

### Swagger’dan Otomatik Endpoint Dökümü (README’ye bağlamak için)

Aşağıdaki adımlarla canlı çalışan API’dan OpenAPI şemasını alıp Markdown döküm üretebilirsiniz:

1) OpenAPI şemasını indir
```bash
curl -s http://localhost:5115/swagger/v1/swagger.json -o openapi.json
```

2) Markdown çıktı üret (API.md)
```bash
npx swagger-markdown -i openapi.json -o API.md
```

Alternatif HTML doküm: 
```bash
npx @redocly/cli build-docs openapi.json -o docs.html
```

3) README’ye bağlantı ekleyin: `[Ayrıntılı API Dökümü](API.md)`

---

## English Version

### Overview

Full‑stack banking application demonstrating Clean Architecture, DDD, multi‑currency accounts, deposits, transfers (with FX), and live exchange rates.

- Backend: .NET 8, EF Core, Repository + Unit of Work, AutoMapper, Swagger
- Frontend: Angular 17+ (Standalone Components), HttpClient, Router
- Database: SQL Server 2022 (Docker, port 1433)

### Project Layout

```
BankingApp (API, Application, Domain, Infrastructure, Common)
BankingApp.UI (Angular app)
database (SQL scripts)
docker-compose.yml (SQL Server)
```

### Run locally

```bash
# DB
cd /Users/selimyilbas/Desktop/BankingProject
docker-compose up -d

# API
cd /Users/selimyilbas/Desktop/BankingProject/BankingApp/BankingApp.API
dotnet restore && dotnet run
# -> http://localhost:5115 (Swagger: /swagger)

# UI
cd /Users/selimyilbas/Desktop/BankingProject/BankingApp.UI
npm i && ng serve --open
# -> http://localhost:4200
```

### Configuration

- Connection string (SQL Server):
```
Server=localhost,1433;Database=BankingDB;User Id=sa;Password=Selim@123456789;TrustServerCertificate=True
```
- CORS: `http://localhost:4200`
- Optional AES encryption: set `Encryption:Key` and `Encryption:Version` in `appsettings.json`
- External APIs: `VakifbankApi.*`, `ExchangeRateApi.ApiKey`
- Frontend API base: `http://localhost:5115/api`

### Domain rules

- Currencies: TL, EUR, USD
- Account numbers: TL→1, EUR→2, USD→3 prefixes

### API design and endpoints

- Standard response wrapper `ApiResponse<T>` with lowercase JSON keys.
- Key endpoints:
  - Auth: POST `/api/auth/login`, `/api/auth/register`
  - Customer: CRUD, lookups by id/number/TCKN, password change, paged list
  - Account: create, get by id/number/customer, balance, status update
  - Transaction: deposit, account history, date‑range, paged
  - Transfer: create (by ids or account numbers), validate, history by account/customer, get by id
  - ExchangeRate: `current`, `rate`, `vakifbank/today`, `update`

### Test users

| TCKN | Password | Name |
|------|----------|------|
| 12345678901 | 123456 | Ahmet Yılmaz |
| 98765432109 | 123456 | Ayşe Kaya |
| 11111111111 | 123456 | Test User |

### Notes

- This is a learning project. Use proper password hashing, HTTPS, rate limiting, and input hardening for production. Authorization (roles) is not yet implemented.

### Screenshots (Placeholder)

- Dashboard (placeholder)

![Dashboard](BankingApp.UI/src/assets/images/vakifbank-logo.jpg)

- Transfer (placeholder)

![Transfer](BankingApp.UI/src/assets/images/vakifbank-logo-sari-zemin.jpg)

Will be replaced by real UI screenshots as the project evolves.

### Generate Swagger-based API Docs

```bash
# 1) Fetch OpenAPI JSON
curl -s http://localhost:5115/swagger/v1/swagger.json -o openapi.json

# 2) Produce Markdown (API.md)
npx swagger-markdown -i openapi.json -o API.md

# (Optional) Produce HTML docs
npx @redocly/cli build-docs openapi.json -o docs.html
```

Then link from README: `[Detailed API Docs](API.md)`

