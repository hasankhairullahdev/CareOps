FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS base
WORKDIR /app
EXPOSE 5002

FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

COPY ["src/AppointmentService.Api/AppointmentService.Api.csproj", "src/AppointmentService.Api/"]
COPY ["src/AppointmentService.Application/AppointmentService.Application.csproj", "src/AppointmentService.Application/"]
COPY ["src/AppointmentService.Domain/AppointmentService.Domain.csproj", "src/AppointmentService.Domain/"]
COPY ["src/AppointmentService.Infrastructure/AppointmentService.Infrastructure.csproj", "src/AppointmentService.Infrastructure/"]

RUN dotnet restore "src/AppointmentService.Api/AppointmentService.Api.csproj"

COPY . .

RUN dotnet build "src/AppointmentService.Api/AppointmentService.Api.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "src/AppointmentService.Api/AppointmentService.Api.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "AppointmentService.Api.dll"]
