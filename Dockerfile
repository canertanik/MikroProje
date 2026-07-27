FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

# Copy sln and csproj files for restore caching
COPY MikroProje.sln ./
COPY MikroProje.Domain/MikroProje.Domain.csproj MikroProje.Domain/
COPY MikroProje.Application/MikroProje.Application.csproj MikroProje.Application/
COPY MikroProje.Infrastructure/MikroProje.Infrastructure.csproj MikroProje.Infrastructure/
COPY MikroProje.Persistence/MikroProje.Persistence.csproj MikroProje.Persistence/
COPY MikroProje.API/MikroProje.API.csproj MikroProje.API/

RUN dotnet restore

# Copy all source code
COPY . .
WORKDIR /src/MikroProje.API
RUN dotnet publish -c Release -o /app/publish /p:UseAppHost=false

# Runtime Image
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS final
WORKDIR /app
COPY --from=build /app/publish .

# Install curl for health check
USER root
RUN apt-get update && apt-get install -y curl && rm -rf /var/lib/apt/lists/*

# Run as non-root user for security
USER app

# Expose port
EXPOSE 8080
ENV ASPNETCORE_URLS=http://+:8080
ENV ASPNETCORE_ENVIRONMENT=Development

ENTRYPOINT ["dotnet", "MikroProje.API.dll"]
