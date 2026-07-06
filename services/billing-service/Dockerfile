FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS base
WORKDIR /app
EXPOSE 5004

FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src
COPY ["src/BillingService.Api/BillingService.Api.csproj", "src/BillingService.Api/"]
COPY ["src/BillingService.Application/BillingService.Application.csproj", "src/BillingService.Application/"]
COPY ["src/BillingService.Domain/BillingService.Domain.csproj", "src/BillingService.Domain/"]
COPY ["src/BillingService.Infrastructure/BillingService.Infrastructure.csproj", "src/BillingService.Infrastructure/"]
RUN dotnet restore "src/BillingService.Api/BillingService.Api.csproj"
COPY . .
RUN dotnet build "src/BillingService.Api/BillingService.Api.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "src/BillingService.Api/BillingService.Api.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "BillingService.Api.dll"]
