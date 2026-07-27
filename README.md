\# MikroProje ERP



ASP.NET Core ve Clean Architecture kullanÄ±larak geliÅŸtirilen ERP backend projesi.



\## ğŸš€ KullanÄ±lan Teknolojiler



\- ASP.NET Core Web API (.NET 9)

\- Entity Framework Core

\- SQL Server

\- Clean Architecture

\- CQRS (MediatR)

\- FluentValidation

\- AutoMapper



\## ğŸ“¦ ModÃ¼ller



\- Cari Hesap YÃ¶netimi

\- ÃœrÃ¼n YÃ¶netimi

\- Stok Hareketleri

\- SatÄ±ÅŸ YÃ¶netimi



\## ğŸ“Œ Ã–zellikler



\- Soft Delete

\- Optimistic Concurrency (RowVersion)

\- Repository Pattern

\- Transaction Management

\- Validation Pipeline

\- Pagination

\- Result Pattern



\## ğŸ› ï¸ Kurulum



```bash

git clone https://github.com/canertanik/MikroProje.git

```



Connection string'i `appsettings.json` dosyasÄ±nda dÃ¼zenleyin.



ArdÄ±ndan:



```bash

dotnet restore

dotnet ef database update

dotnet run --project MikroProje.API

```


## ?? Docker ile Çalıştırma

Proje, Docker ve Docker Compose kullanılarak tek bir komutla ayağa kaldırılabilir. 

### Ön Koşullar
- **Docker Desktop** (veya eşdeğer bir Docker ortamı) sisteminizde kurulu ve çalışır durumda olmalıdır.

### Adım Adım Kurulum

1. Repository'yi klonlayın:
   `ash
   git clone https://github.com/canertanik/MikroProje.git
   cd MikroProje
   `

2. .env dosyasını oluşturun:
   Kök dizindeki .env.example dosyasının adını .env olarak değiştirin veya kopyalayın. İçerisindeki SQL Server SA şifresini kendinize göre güncelleyin.
   `env
   SA_PASSWORD=SizinÇokGüçlüŞifreniz123!
   ASPNETCORE_ENVIRONMENT=Development
   `
   *Not: SQL Server güçlü bir şifre gerektirir (En az 8 karakter, büyük harf, küçük harf, rakam ve özel karakter).*

3. Konteynerleri başlatın:
   `ash
   docker compose up --build -d
   `

4. Servislere Erişin:
   - **API Adresi:** http://localhost:8080
   - **Swagger (Development):** http://localhost:8080/swagger
   - **Health Check:** http://localhost:8080/health

5. Logları Görüntüleme:
   `ash
   docker compose logs -f api
   `

6. Sistemi Durdurma:
   `ash
   docker compose down
   `

7. Sistemi Verilerle Birlikte Tamamen Silme (DİKKAT!):
   Veritabanı (mikroproje-sql-data volume) dahil tüm container ve verileri silmek için:
   `ash
   docker compose down -v
   `

### Sık Karşılaşılan Hatalar
- **SQL Server Password Hatası:** Veritabanı ayağa kalkmıyorsa, docker compose logs sqlserver komutuyla hataya bakın. Çoğunlukla SA_PASSWORD kurallara uymadığında bu hata alınır. Lütfen şifrenizin büyük harf, küçük harf, rakam ve sembol içerdiğinden emin olun.
- **Port Çakışması:** Eğer 8080 veya 1433 portları başka bir uygulama tarafından kullanılıyorsa, docker-compose.yml içindeki ports eşleştirmelerini 8081:8080 şeklinde değiştirebilirsiniz.

