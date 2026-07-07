# ==============================================================================
# dev-start.ps1 — CareOps Local Dev Starter
# Jalankan infra via Docker, semua service via dotnet run di terminal terpisah
# ==============================================================================
# Usage:
#   .\scripts\dev-start.ps1          # start infra + semua service + frontend
#   .\scripts\dev-start.ps1 -Stop    # stop semua + matikan infra
#   .\scripts\dev-start.ps1 -Infra   # hanya start infra (postgres, rabbitmq, keycloak)
# ==============================================================================

param(
    [switch]$Stop,
    [switch]$Infra
)

$Root = Split-Path $PSScriptRoot -Parent

function Write-Step($msg) { Write-Host "`n==> $msg" -ForegroundColor Cyan }
function Write-Ok($msg)   { Write-Host "    [OK] $msg" -ForegroundColor Green }
function Write-Warn($msg) { Write-Host "    [!!] $msg" -ForegroundColor Yellow }

# ─── STOP ──────────────────────────────────────────────────────────────────────
if ($Stop) {
    Write-Step "Stopping all dotnet processes (CareOps services)..."
    Get-Process -Name "dotnet" -ErrorAction SilentlyContinue | ForEach-Object {
        $cmdline = (Get-WmiObject Win32_Process -Filter "ProcessId=$($_.Id)").CommandLine
        if ($cmdline -match "PatientService|AppointmentService|PharmacyService|BillingService|NotificationService|HospitalGateway") {
            $_.Kill()
            Write-Ok "Killed PID $($_.Id)"
        }
    }
    Write-Step "Stopping Node (frontend)..."
    Get-Process -Name "node" -ErrorAction SilentlyContinue | ForEach-Object {
        $_.Kill()
        Write-Ok "Killed node PID $($_.Id)"
    }
    Write-Step "Stopping Podman infra..."
    podman compose -f "$Root\docker-compose.infra.yml" down
    Write-Ok "Done."
    exit 0
}

# ─── CHECK PREREQUISITES ───────────────────────────────────────────────────────
Write-Step "Checking prerequisites..."

if (-not (Get-Command dotnet -ErrorAction SilentlyContinue)) {
    Write-Error ".NET SDK not found. Install from https://dot.net"
    exit 1
}
Write-Ok ".NET SDK: $(dotnet --version)"

if (-not (Get-Command podman -ErrorAction SilentlyContinue)) {
    Write-Error "Podman not found. Install from https://podman.io"
    exit 1
}
Write-Ok "Podman: $(podman --version)"

if (-not (Get-Command npm -ErrorAction SilentlyContinue)) {
    Write-Warn "npm not found — frontend akan dilewati. Install Node.js dari https://nodejs.org"
    $SkipFrontend = $true
} else {
    Write-Ok "npm: $(npm --version)"
    $SkipFrontend = $false
}

# ─── START INFRA ───────────────────────────────────────────────────────────────
Write-Step "Starting infrastructure (PostgreSQL, RabbitMQ, Keycloak)..."
Push-Location $Root
podman compose -f docker-compose.infra.yml up -d
Pop-Location
Write-Ok "Infrastructure containers started."

# ─── WAIT FOR POSTGRES ─────────────────────────────────────────────────────────
Write-Step "Waiting for PostgreSQL to be ready..."
$retries = 0
do {
    Start-Sleep -Seconds 3
    $result = podman exec hospital_postgres pg_isready -U postgres 2>&1
    $retries++
    if ($retries -gt 20) { Write-Error "PostgreSQL not ready after 60s. Aborting."; exit 1 }
} while ($result -notmatch "accepting connections")
Write-Ok "PostgreSQL ready."

# ─── WAIT FOR RABBITMQ ─────────────────────────────────────────────────────────
Write-Step "Waiting for RabbitMQ to be ready..."
$retries = 0
do {
    Start-Sleep -Seconds 4
    $result = podman exec hospital_rabbitmq rabbitmq-diagnostics -q ping 2>&1
    if ($LASTEXITCODE -eq 0) { break }
    $retries++
    if ($retries -gt 25) { Write-Error "RabbitMQ not ready after 100s. Aborting."; exit 1 }
} while ($true)
Write-Ok "RabbitMQ ready. Management UI: http://localhost:15672 (guest/guest)"

if ($Infra) {
    Write-Step "Infra only mode. Keycloak starting in background (may take ~30s)."
    Write-Ok "Keycloak: http://localhost:8080 (admin/admin)"
    Write-Ok "Done."
    exit 0
}

# ─── START BACKEND SERVICES ────────────────────────────────────────────────────
$Services = @(
    @{ Name = "patient-service";      Path = "services\patient-service\src\PatientService.Api";         Port = 5001; Url = "http://localhost:5001" },
    @{ Name = "appointment-service";  Path = "services\appointment-service\src\AppointmentService.Api"; Port = 5002; Url = "http://localhost:5002" },
    @{ Name = "pharmacy-service";     Path = "services\pharmacy-service\src\PharmacyService.Api";       Port = 5003; Url = "http://localhost:5003" },
    @{ Name = "billing-service";      Path = "services\billing-service\src\BillingService.Api";         Port = 5004; Url = "http://localhost:5004" },
    @{ Name = "notification-service"; Path = "services\notification-service\src\NotificationService";   Port = 5005; Url = "http://localhost:5005" },
    @{ Name = "api-gateway";          Path = "gateway\HospitalGateway";                                 Port = 5000; Url = "http://localhost:5000" }
)

Write-Step "Starting backend services in separate windows..."

foreach ($svc in $Services) {
    $fullPath = Join-Path $Root $svc.Path
    $title    = "CareOps | $($svc.Name) :$($svc.Port)"
    $cmd      = "& { `$host.UI.RawUI.WindowTitle = '$title'; Set-Location '$fullPath'; `$env:ASPNETCORE_ENVIRONMENT = 'Development'; dotnet run }"

    Start-Process powershell -ArgumentList @("-NoExit", "-Command", $cmd) -WindowStyle Normal
    Write-Ok "Started $($svc.Name) -> $($svc.Url)"
    Start-Sleep -Milliseconds 800
}

# ─── START FRONTEND ────────────────────────────────────────────────────────────
if (-not $SkipFrontend) {
    Write-Step "Starting frontend (Next.js)..."
    $frontendPath  = Join-Path $Root "frontend"
    $frontendTitle = "CareOps | frontend :3000"

    # Pastikan .env.local ada
    $envLocal   = Join-Path $frontendPath ".env.local"
    $envExample = Join-Path $frontendPath ".env.example"
    if (-not (Test-Path $envLocal)) {
        if (Test-Path $envExample) {
            Copy-Item $envExample $envLocal
            Write-Warn ".env.local belum ada — dicopy dari .env.example. Sesuaikan isinya!"
        } else {
            Write-Warn ".env.local tidak ditemukan. Frontend mungkin gagal boot."
        }
    }

    $cmd = "& { `$host.UI.RawUI.WindowTitle = '$frontendTitle'; Set-Location '$frontendPath'; npm run dev }"
    Start-Process powershell -ArgumentList @("-NoExit", "-Command", $cmd) -WindowStyle Normal
    Write-Ok "Started frontend -> http://localhost:3000"
}

# ─── WAIT & HEALTH CHECK ───────────────────────────────────────────────────────
Write-Step "Waiting 15s for services to boot..."
Start-Sleep -Seconds 15

Write-Step "Health checks (backend)..."
foreach ($svc in $Services) {
    try {
        $resp = Invoke-WebRequest -Uri "$($svc.Url)/health" -TimeoutSec 5 -ErrorAction Stop
        Write-Ok "$($svc.Name) -> $($resp.StatusCode) HEALTHY"
    } catch {
        Write-Warn "$($svc.Name) -> not ready yet (still booting?)"
    }
}

# ─── SUMMARY ───────────────────────────────────────────────────────────────────
Write-Host ""
Write-Host "══════════════════════════════════════════════════════" -ForegroundColor Cyan
Write-Host "  CareOps Dev Environment Running" -ForegroundColor Cyan
Write-Host "══════════════════════════════════════════════════════" -ForegroundColor Cyan
Write-Host ""
Write-Host "  Frontend           http://localhost:3000" -ForegroundColor White
Write-Host "  API Gateway        http://localhost:5000" -ForegroundColor White
Write-Host "  Patient Service    http://localhost:5001/swagger" -ForegroundColor White
Write-Host "  Appointment Svc    http://localhost:5002/swagger" -ForegroundColor White
Write-Host "  Pharmacy Svc       http://localhost:5003/swagger" -ForegroundColor White
Write-Host "  Billing Svc        http://localhost:5004/swagger" -ForegroundColor White
Write-Host "  Notification Svc   http://localhost:5005/swagger" -ForegroundColor White
Write-Host ""
Write-Host "  Keycloak Admin     http://localhost:8080  (admin/admin)" -ForegroundColor DarkYellow
Write-Host "  RabbitMQ UI        http://localhost:15672 (guest/guest)" -ForegroundColor DarkYellow
Write-Host "  PostgreSQL         localhost:5432          (postgres/postgres)" -ForegroundColor DarkYellow
Write-Host ""
Write-Host "  To stop all:  .\scripts\dev-start.ps1 -Stop" -ForegroundColor Gray
Write-Host "══════════════════════════════════════════════════════" -ForegroundColor Cyan
