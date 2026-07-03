FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS base
WORKDIR /app
EXPOSE 5003

FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src
COPY ["src/PharmacyService.Api/PharmacyService.Api.csproj", "src/PharmacyService.Api/"]
COPY ["src/PharmacyService.Application/PharmacyService.Application.csproj", "src/PharmacyService.Application/"]
COPY ["src/PharmacyService.Domain/PharmacyService.Domain.csproj", "src/PharmacyService.Domain/"]
COPY ["src/PharmacyService.Infrastructure/PharmacyService.Infrastructure.csproj", "src/PharmacyService.Infrastructure/"]
RUN dotnet restore "src/PharmacyService.Api/PharmacyService.Api.csproj"
COPY . .
RUN dotnet build "src/PharmacyService.Api/PharmacyService.Api.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "src/PharmacyService.Api/PharmacyService.Api.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "PharmacyService.Api.dll"]
