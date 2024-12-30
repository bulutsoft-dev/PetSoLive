# Temel ASP.NET Core çalışma zamanı
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 5000
EXPOSE 5001

# Build işlemleri
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src

# Projeleri kopyala ve restore et
COPY ["PetSoLive.Web/PetSoLive.Web.csproj", "PetSoLive.Web/"]
COPY ["PetSoLive.Business/PetSoLive.Business.csproj", "PetSoLive.Business/"]
COPY ["PetSoLive.Core/PetSoLive.Core.csproj", "PetSoLive.Core/"]
COPY ["PetSoLive.Data/PetSoLive.Data.csproj", "PetSoLive.Data/"]
RUN dotnet restore "PetSoLive.Web/PetSoLive.Web.csproj"

# Uygulama kodunu kopyala ve build et
COPY . .
WORKDIR "/src/PetSoLive.Web"
RUN dotnet build "PetSoLive.Web.csproj" -c $BUILD_CONFIGURATION -o /app/build

# Publish aşaması
FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "PetSoLive.Web.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

# Çalıştırma aşaması
FROM base AS final
WORKDIR /app

# Publish edilmiş dosyaları kopyala
COPY --from=publish /app/publish .

# Giriş noktası
ENTRYPOINT ["dotnet", "PetSoLive.Web.dll"]
