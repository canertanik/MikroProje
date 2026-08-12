FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS base
USER app
WORKDIR /app
EXPOSE 8080

ENV ASPNETCORE_URLS=http://+:8080

FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src

# Copy csproj and restore as distinct layers
COPY ["MikroProje.API/MikroProje.API.csproj", "MikroProje.API/"]
COPY ["MikroProje.Application/MikroProje.Application.csproj", "MikroProje.Application/"]
COPY ["MikroProje.Domain/MikroProje.Domain.csproj", "MikroProje.Domain/"]
COPY ["MikroProje.Infrastructure/MikroProje.Infrastructure.csproj", "MikroProje.Infrastructure/"]
COPY ["MikroProje.Persistence/MikroProje.Persistence.csproj", "MikroProje.Persistence/"]
RUN dotnet restore "./MikroProje.API/MikroProje.API.csproj"

# Copy everything else and build
COPY . .
WORKDIR "/src/MikroProje.API"
RUN dotnet build "./MikroProje.API.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./MikroProje.API.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "MikroProje.API.dll"]
