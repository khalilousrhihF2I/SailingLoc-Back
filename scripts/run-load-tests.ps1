<#
.SYNOPSIS
    Lance les tests de montée en charge (load/stress tests) via NBomber.

.DESCRIPTION
    Exécute les tests NBomber un par un.
    Les rapports HTML/CSV/MD sont auto-générés par NBomber dans ./reports/

.EXAMPLE
    .\run-load-tests.ps1
    .\run-load-tests.ps1 -TestName "LoadTest_GetBoats_Sustained"
    .\run-load-tests.ps1 -All
#>

param(
    [string]$TestName,
    [switch]$All
)

$ErrorActionPreference = "Stop"

$projectRoot = Split-Path -Parent $PSScriptRoot
$testProject = Join-Path $projectRoot "src\Tests\Tests.csproj"

Write-Host "============================================" -ForegroundColor Cyan
Write-Host "  SailingLoc API — Load Test Runner         " -ForegroundColor Cyan
Write-Host "============================================" -ForegroundColor Cyan
Write-Host ""
Write-Host "⚠  PRÉREQUIS : L'API doit tourner localement" -ForegroundColor Yellow
Write-Host "   Lancez : dotnet run --project src/Api     " -ForegroundColor Yellow
Write-Host ""

if ($TestName) {
    Write-Host "Exécution du test : $TestName" -ForegroundColor Green
    & dotnet test $testProject `
        --filter "FullyQualifiedName~$TestName" `
        --configuration Release `
        --no-build `
        -- xunit.skipFactor=0
} elseif ($All) {
    Write-Host "Exécution de TOUS les tests de charge..." -ForegroundColor Green
    Write-Host "(Cela peut prendre plusieurs minutes)" -ForegroundColor DarkGray
    & dotnet test $testProject `
        --filter "FullyQualifiedName~LoadTests" `
        --configuration Release `
        -- xunit.skipFactor=0
} else {
    Write-Host "Tests de charge disponibles :" -ForegroundColor Yellow
    Write-Host ""
    Write-Host "  === Endpoint Tests ===" -ForegroundColor Cyan
    Write-Host "  1. LoadTest_GetBoats_Sustained        (50 req/s, 30s)"
    Write-Host "  2. LoadTest_GetBoatById_Sustained      (100 req/s, 30s)"
    Write-Host "  3. LoadTest_GetDestinations_Sustained   (80 req/s, 30s)"
    Write-Host "  4. LoadTest_GetReviews_Sustained       (60 req/s, 30s)"
    Write-Host "  5. LoadTest_CheckAvailability_Sustained (40 req/s, 30s)"
    Write-Host ""
    Write-Host "  === Pattern Tests ===" -ForegroundColor Cyan
    Write-Host "  6. SpikeTest_GetBoats                  (pic 200 req/s)"
    Write-Host "  7. StressTest_GradualIncrease          (20→500 req/s)"
    Write-Host "  8. MixedScenario_RealisticTraffic      (multi-endpoint, 60s)"
    Write-Host "  9. EnduranceTest_5Minutes               (30 req/s, 5 min)"
    Write-Host ""
    Write-Host "  === Authenticated Tests ===" -ForegroundColor Cyan
    Write-Host "  10. LoadTest_Login                     (20 req/s, 30s)"
    Write-Host "  11. LoadTest_CreateBooking              (10 req/s, 20s)"
    Write-Host "  12. LoadTest_CreateReview               (15 req/s, 20s)"
    Write-Host "  13. LoadTest_BookingWorkflow            (5 req/s, 30s)"
    Write-Host "  14. ConcurrencyTest_50Users             (50 users, 60s)"
    Write-Host "  15. ScalingTest_RampUpUsers             (10→200 users)"
    Write-Host ""
    Write-Host "Usage :" -ForegroundColor Yellow
    Write-Host "  .\run-load-tests.ps1 -TestName 'LoadTest_GetBoats_Sustained'"
    Write-Host "  .\run-load-tests.ps1 -All"
    Write-Host ""
    Write-Host "Note : Les tests ont un [Skip] par défaut." -ForegroundColor DarkGray
    Write-Host "Pour les activer, retirez le Skip='...' de l'attribut [Fact]" -ForegroundColor DarkGray
    Write-Host "ou utilisez la variable d'environnement RUN_LOAD_TESTS=true" -ForegroundColor DarkGray
}

Write-Host ""
Write-Host "Rapports NBomber disponibles dans : ./reports/" -ForegroundColor Green
