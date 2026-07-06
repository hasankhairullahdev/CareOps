# ==============================================================================
# dev-stop.ps1 — Stop semua CareOps services + infra
# ==============================================================================

function Write-Step($msg) { Write-Host "`n==> $msg" -ForegroundColor Cyan }
function Write-Ok($msg)   { Write-Host "    [OK] $msg" -ForegroundColor Green }

# ─── Stop dotnet processes ────────────────────────────────────────────────────
Write-Step "Stopping dotnet services..."
$killed = 0
Get-Process -Name "dotnet" -ErrorAction SilentlyContinue | ForEach-Object {
    $cmdline = (Get-WmiObject Win32_Process -Filter "ProcessId=$($_.Id)").CommandLine
    if ($cmdline -match "PatientService|AppointmentService|PharmacyService|NotificationService|HospitalGateway|BillingService") {
        $_.Kill()
        $killed++
    }
}
if ($killed -gt 0) { Write-Ok "Killed $killed dotnet process(es)" }
else               { Write-Ok "No dotnet services were running" }

# ─── Stop Podman infra ────────────────────────────────────────────────────────
Write-Step "Stopping Podman infrastructure..."
$Root = Split-Path $PSScriptRoot -Parent
podman compose -f "$Root\docker-compose.infra.yml" down 2>&1 | Out-Null
Write-Ok "Postgres, RabbitMQ, Keycloak stopped"

Write-Host ""
Write-Host "All CareOps services stopped." -ForegroundColor Green
