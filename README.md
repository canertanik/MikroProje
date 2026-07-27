[![MikroProje CI](https://github.com/canertanik/MikroProje/actions/workflows/ci.yml/badge.svg)](https://github.com/canertanik/MikroProje/actions/workflows/ci.yml)

\# MikroProje ERP



ASP.NET Core ve Clean Architecture kullanılarak geliştirilen ERP backend projesi.



\## 🚀 Kullanılan Teknolojiler



\- ASP.NET Core Web API (.NET 9)

\- Entity Framework Core

\- SQL Server

\- Clean Architecture

\- CQRS (MediatR)

\- FluentValidation

\- AutoMapper



\## 📦 Modüller



\- Cari Hesap Yönetimi

\- Ürün Yönetimi

\- Stok Hareketleri

\- Satış Yönetimi



\## 📌 Özellikler



\- Soft Delete

\- Optimistic Concurrency (RowVersion)

\- Repository Pattern

\- Transaction Management

\- Validation Pipeline

\- Pagination

\- Result Pattern



\## 🛠️ Kurulum



```bash

git clone https://github.com/canertanik/MikroProje.git

```



Connection string'i `appsettings.json` dosyasında düzenleyin.



Ardından:



```bash

dotnet restore

dotnet ef database update

dotnet run --project MikroProje.API

```


## ?? Docker ile �al��t�rma

Proje, Docker ve Docker Compose kullan�larak tek bir komutla aya�a kald�r�labilir. 

### �n Ko�ullar
- **Docker Desktop** (veya e�de�er bir Docker ortam�) sisteminizde kurulu ve �al���r durumda olmal�d�r.

### Ad�m Ad�m Kurulum

1. Repository'yi klonlay�n:
   `ash
   git clone https://github.com/canertanik/MikroProje.git
   cd MikroProje
   `

2. .env dosyas�n� olu�turun:
   K�k dizindeki .env.example dosyas�n�n ad�n� .env olarak de�i�tirin veya kopyalay�n. ��erisindeki SQL Server SA �ifresini kendinize g�re g�ncelleyin.
   `env
   SA_PASSWORD=Sizin�okG��l��ifreniz123!
   ASPNETCORE_ENVIRONMENT=Development
   `
   *Not: SQL Server g��l� bir �ifre gerektirir (En az 8 karakter, b�y�k harf, k���k harf, rakam ve �zel karakter).*

3. Konteynerleri ba�lat�n:
   `ash
   docker compose up --build -d
   `

4. Servislere Eri�in:
   - **API Adresi:** http://localhost:8080
   - **Swagger (Development):** http://localhost:8080/swagger
   - **Health Check:** http://localhost:8080/health

5. Loglar� G�r�nt�leme:
   `ash
   docker compose logs -f api
   `

6. Sistemi Durdurma:
   `ash
   docker compose down
   `

7. Sistemi Verilerle Birlikte Tamamen Silme (D�KKAT!):
   Veritaban� (mikroproje-sql-data volume) dahil t�m container ve verileri silmek i�in:
   `ash
   docker compose down -v
   `

### S�k Kar��la��lan Hatalar
- **SQL Server Password Hatas�:** Veritaban� aya�a kalkm�yorsa, docker compose logs sqlserver komutuyla hataya bak�n. �o�unlukla SA_PASSWORD kurallara uymad���nda bu hata al�n�r. L�tfen �ifrenizin b�y�k harf, k���k harf, rakam ve sembol i�erdi�inden emin olun.
- **Port �ak��mas�:** E�er 8080 veya 1433 portlar� ba�ka bir uygulama taraf�ndan kullan�l�yorsa, docker-compose.yml i�indeki ports e�le�tirmelerini 8081:8080 �eklinde de�i�tirebilirsiniz.


