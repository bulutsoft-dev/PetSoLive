FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
USER $APP_UID
WORKDIR /app
EXPOSE 8080
EXPOSE 8081

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["Petsolive.API/Petsolive.API.csproj", "Petsolive.API/"]
COPY ["PetSoLive.Data/PetSoLive.Data.csproj", "PetSoLive.Data/"]
COPY ["PetSoLive.Core/PetSoLive.Core.csproj", "PetSoLive.Core/"]
COPY ["PetSoLive.Business/PetSoLive.Business.csproj", "PetSoLive.Business/"]
RUN dotnet restore "Petsolive.API/Petsolive.API.csproj"
COPY . .
WORKDIR "/src/Petsolive.API"
RUN dotnet build "./Petsolive.API.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./Petsolive.API.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Petsolive.API.dll"]
