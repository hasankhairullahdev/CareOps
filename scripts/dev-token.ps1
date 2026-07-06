# ==============================================================================
# dev-token.ps1 — Get Keycloak access token untuk testing
# ==============================================================================
# Usage:
#   .\scripts\dev-token.ps1                                   # default: admin
#   .\scripts\dev-token.ps1 -Role doctor                      # login sebagai doctor
#   .\scripts\dev-token.ps1 -Role pharmacist -CopyToClipboard # copy ke clipboard
# ==============================================================================

param(
    [ValidateSet("admin","receptionist","doctor","pharmacist","cashier","patient")]
    [string]$Role = "admin",
    [switch]$CopyToClipboard
)

$users = @{
    admin        = @{ username = "admin@hospital.com";       password = "Admin123!" }
    receptionist = @{ username = "reception@hospital.com";   password = "Admin123!" }
    doctor       = @{ username = "doctor@hospital.com";      password = "Admin123!" }
    pharmacist   = @{ username = "pharmacist@hospital.com";  password = "Admin123!" }
    cashier      = @{ username = "cashier@hospital.com";     password = "Admin123!" }
    patient      = @{ username = "patient@hospital.com";     password = "Admin123!" }
}

$user = $users[$Role]
$tokenUrl = "http://localhost:8080/realms/careops/protocol/openid-connect/token"

Write-Host "Getting token for role '$Role' ($($user.username))..." -ForegroundColor Cyan

try {
    $response = Invoke-RestMethod -Uri $tokenUrl -Method Post `
        -ContentType "application/x-www-form-urlencoded" `
        -Body "grant_type=password&client_id=careops-frontend&username=$($user.username)&password=$($user.password)" `
        -ErrorAction Stop

    $token = $response.access_token
    $expires = $response.expires_in

    Write-Host "`nToken (expires in ${expires}s):" -ForegroundColor Green
    Write-Host $token -ForegroundColor White

    if ($CopyToClipboard) {
        $token | Set-Clipboard
        Write-Host "`nToken copied to clipboard!" -ForegroundColor Yellow
    }

    Write-Host "`nUsage example:" -ForegroundColor Gray
    Write-Host "  curl http://localhost:5001/patients -H `"Authorization: Bearer `$TOKEN`"" -ForegroundColor Gray
    Write-Host "  Invoke-RestMethod http://localhost:5001/patients -Headers @{Authorization='Bearer $token'.Substring(0,30)+'...'}" -ForegroundColor Gray

    # Quick decode roles from JWT payload
    $parts = $token.Split('.')
    if ($parts.Count -ge 2) {
        $padding = 4 - ($parts[1].Length % 4)
        if ($padding -ne 4) { $parts[1] += '=' * $padding }
        $payload = [Text.Encoding]::UTF8.GetString([Convert]::FromBase64String($parts[1])) | ConvertFrom-Json
        $roles = $payload.realm_access.roles -join ", "
        Write-Host "`nRoles in token: $roles" -ForegroundColor DarkYellow
    }

    return $token
} catch {
    Write-Host "`nFailed to get token: $($_.Exception.Message)" -ForegroundColor Red
    Write-Host "Is Keycloak running? http://localhost:8080" -ForegroundColor Yellow
}
