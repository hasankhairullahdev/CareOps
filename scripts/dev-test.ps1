# ==============================================================================
# dev-test.ps1 — CareOps API Smoke Test
# Jalankan setelah dev-start.ps1 — tanpa auth (endpoint health & read-only)
# ==============================================================================
# Usage: .\scripts\dev-test.ps1
# ==============================================================================

$base = "http://localhost"

function Write-Step($msg)    { Write-Host "`n── $msg" -ForegroundColor Cyan }
function Test-Pass($msg)     { Write-Host "  ✓ $msg" -ForegroundColor Green }
function Test-Fail($msg, $e) { Write-Host "  ✗ $msg — $e" -ForegroundColor Red }

function Invoke-Test($label, $url, $expectedCode = 200) {
    try {
        $r = Invoke-WebRequest -Uri $url -TimeoutSec 8 -ErrorAction Stop
        if ($r.StatusCode -eq $expectedCode) {
            Test-Pass "$label → $($r.StatusCode)"
        } else {
            Test-Fail $label "Expected $expectedCode, got $($r.StatusCode)"
        }
    } catch {
        $code = $_.Exception.Response.StatusCode.value__
        if ($code -eq $expectedCode) {
            Test-Pass "$label → $code (expected)"
        } else {
            Test-Fail $label $_.Exception.Message
        }
    }
}

function Invoke-Post($label, $url, $body) {
    try {
        $r = Invoke-RestMethod -Uri $url -Method Post `
            -ContentType "application/json" `
            -Body ($body | ConvertTo-Json -Depth 5) `
            -ErrorAction Stop
        Test-Pass "$label → created (ID: $($r.id ?? $r.prescriptionId ?? 'ok'))"
        return $r
    } catch {
        $msg = $_.ErrorDetails.Message ?? $_.Exception.Message
        Test-Fail $label $msg
        return $null
    }
}

# ─── HEALTH CHECKS ────────────────────────────────────────────────────────────
Write-Step "Health Checks"
Invoke-Test "patient-service /health"      "$base:5001/health"
Invoke-Test "appointment-service /health"  "$base:5002/health"
Invoke-Test "pharmacy-service /health"     "$base:5003/health"
Invoke-Test "notification-service /health" "$base:5005/health"
Invoke-Test "api-gateway /health"          "$base:5000/health"

# ─── SWAGGER DOCS AVAILABLE ───────────────────────────────────────────────────
Write-Step "Swagger / OpenAPI"
Invoke-Test "patient-service swagger"     "$base:5001/swagger/v1/swagger.json"
Invoke-Test "appointment-service swagger" "$base:5002/swagger/v1/swagger.json"
Invoke-Test "pharmacy-service swagger"    "$base:5003/swagger/v1/swagger.json"

# ─── PATIENT SERVICE ──────────────────────────────────────────────────────────
Write-Step "Patient Service (no auth — expect 401)"
Invoke-Test "GET /patients requires auth"   "$base:5001/patients" 401
Invoke-Test "GET /patients/{id} needs auth" "$base:5001/patients/00000000-0000-0000-0000-000000000001" 401

# ─── APPOINTMENT SERVICE ──────────────────────────────────────────────────────
Write-Step "Appointment Service (no auth — expect 401)"
Invoke-Test "GET /appointments requires auth"      "$base:5002/appointments" 401
Invoke-Test "GET /appointments/{id} needs auth"    "$base:5002/appointments/00000000-0000-0000-0000-000000000001" 401

# ─── PHARMACY SERVICE ─────────────────────────────────────────────────────────
Write-Step "Pharmacy Service (no auth — expect 401)"
Invoke-Test "GET /pharmacy/inventory requires auth" "$base:5003/pharmacy/inventory" 401

# ─── NOTIFICATION SERVICE ─────────────────────────────────────────────────────
Write-Step "Notification Service (open endpoint)"
Invoke-Test "GET /notifications/{userId}" "$base:5005/notifications/test-user-id" 200

# ─── API GATEWAY ROUTING ──────────────────────────────────────────────────────
Write-Step "API Gateway Routing (no auth — expect 401 forwarded)"
Invoke-Test "Gateway /api/patients proxied"     "$base:5000/api/patients" 401
Invoke-Test "Gateway /api/appointments proxied" "$base:5000/api/appointments" 401
Invoke-Test "Gateway /api/pharmacy proxied"     "$base:5000/api/pharmacy/inventory" 401

# ─── RABBITMQ MANAGEMENT API ──────────────────────────────────────────────────
Write-Step "RabbitMQ Queues (check via management API)"
try {
    $cred = [Convert]::ToBase64String([Text.Encoding]::ASCII.GetBytes("guest:guest"))
    $queues = Invoke-RestMethod -Uri "http://localhost:15672/api/queues" `
        -Headers @{ Authorization = "Basic $cred" } -ErrorAction Stop
    Test-Pass "RabbitMQ connected — $($queues.Count) queue(s) found"
    if ($queues.Count -gt 0) {
        $queues | ForEach-Object { Write-Host "     · $($_.name) (msgs: $($_.messages))" -ForegroundColor Gray }
    }
} catch {
    Test-Fail "RabbitMQ management API" $_.Exception.Message
}

# ─── SUMMARY ──────────────────────────────────────────────────────────────────
Write-Host ""
Write-Host "══════════════════════════════════════════════════════" -ForegroundColor Cyan
Write-Host "  Smoke test complete." -ForegroundColor Cyan
Write-Host "  For full auth testing, use Swagger UI with a token." -ForegroundColor Gray
Write-Host "  Get token: POST http://localhost:8080/realms/careops/protocol/openid-connect/token" -ForegroundColor Gray
Write-Host "  Body: grant_type=password&client_id=careops-frontend&username=admin@hospital.com&password=Admin123!" -ForegroundColor Gray
Write-Host "══════════════════════════════════════════════════════" -ForegroundColor Cyan
