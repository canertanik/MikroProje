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

