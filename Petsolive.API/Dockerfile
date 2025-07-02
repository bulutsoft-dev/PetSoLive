FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 8080

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY ["Petsolive.API/Petsolive.API.csproj", "Petsolive.API/"]
COPY ["PetSoLive.Business/PetSoLive.Business.csproj", "PetSoLive.Business/"]
COPY ["PetSoLive.Data/PetSoLive.Data.csproj", "PetSoLive.Data/"]
COPY ["PetSoLive.Core/PetSoLive.Core.csproj", "PetSoLive.Core/"]
RUN dotnet restore "Petsolive.API/Petsolive.API.csproj"
COPY . .
WORKDIR "/src/Petsolive.API"
RUN dotnet publish "Petsolive.API.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "Petsolive.API.dll"]
