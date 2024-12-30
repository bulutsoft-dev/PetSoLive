FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
USER $APP_UID
WORKDIR /app
EXPOSE 8080
EXPOSE 8081

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["PetSoLive.Web/PetSoLive.Web.csproj", "PetSoLive.Web/"]
COPY ["PetSoLive.Business/PetSoLive.Business.csproj", "PetSoLive.Business/"]
COPY ["PetSoLive.Core/PetSoLive.Core.csproj", "PetSoLive.Core/"]
COPY ["PetSoLive.Data/PetSoLive.Data.csproj", "PetSoLive.Data/"]
RUN dotnet restore "PetSoLive.Web/PetSoLive.Web.csproj"
COPY . .
WORKDIR "/src/PetSoLive.Web"
RUN dotnet build "PetSoLive.Web.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "PetSoLive.Web.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "PetSoLive.Web.dll"]
