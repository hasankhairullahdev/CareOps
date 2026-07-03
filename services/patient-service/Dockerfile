FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS base
WORKDIR /app
EXPOSE 5001

FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

COPY ["src/PatientService.Api/PatientService.Api.csproj", "src/PatientService.Api/"]
COPY ["src/PatientService.Application/PatientService.Application.csproj", "src/PatientService.Application/"]
COPY ["src/PatientService.Domain/PatientService.Domain.csproj", "src/PatientService.Domain/"]
COPY ["src/PatientService.Infrastructure/PatientService.Infrastructure.csproj", "src/PatientService.Infrastructure/"]

RUN dotnet restore "src/PatientService.Api/PatientService.Api.csproj"

COPY . .

RUN dotnet build "src/PatientService.Api/PatientService.Api.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "src/PatientService.Api/PatientService.Api.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "PatientService.Api.dll"]
