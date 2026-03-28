<#
.SYNOPSIS
    Script de lancement des tests unitaires avec couverture de code et génération de rapports HTML.

.DESCRIPTION
    1. Lance tous les tests unitaires (exclut les tests de charge)
    2. Collecte la couverture de code via coverlet
    3. Génère un rapport HTML via ReportGenerator

.EXAMPLE
    .\run-tests-with-report.ps1
    .\run-tests-with-report.ps1 -SkipReport
    .\run-tests-with-report.ps1 -Filter "FullyQualifiedName~BoatServiceTests"
#>

param(
    [string]$Filter = "FullyQualifiedName!~LoadTests",
    [switch]$SkipReport,
    [string]$Configuration = "Debug"
)

$ErrorActionPreference = "Stop"

$projectRoot = Split-Path -Parent $PSScriptRoot
$testProject = Join-Path $projectRoot "src\Tests\Tests.csproj"
$coverageDir = Join-Path $projectRoot "TestResults\Coverage"
$reportDir   = Join-Path $projectRoot "TestResults\CoverageReport"

Write-Host "============================================" -ForegroundColor Cyan
Write-Host "  SailingLoc API — Test Runner with Reports " -ForegroundColor Cyan
Write-Host "============================================" -ForegroundColor Cyan
Write-Host ""

# ─── Nettoyage ───
Write-Host "[1/4] Nettoyage des résultats précédents..." -ForegroundColor Yellow
if (Test-Path $coverageDir) { Remove-Item $coverageDir -Recurse -Force }
if (Test-Path $reportDir)   { Remove-Item $reportDir -Recurse -Force }
New-Item -ItemType Directory -Path $coverageDir -Force | Out-Null
Write-Host "  ✓ Dossiers nettoyés" -ForegroundColor Green
Write-Host ""

# ─── Tests unitaires avec couverture ───
Write-Host "[2/4] Lancement des tests unitaires avec couverture..." -ForegroundColor Yellow
Write-Host "  Filtre : $Filter" -ForegroundColor DarkGray
Write-Host ""

$testArgs = @(
    "test"
    $testProject
    "--configuration", $Configuration
    "--filter", $Filter
    "--logger", "trx;LogFileName=TestResults.trx"
    "--results-directory", (Join-Path $projectRoot "TestResults")
    "--collect:""XPlat Code Coverage"""
    "--"
    "DataCollectionRunSettings.DataCollectors.DataCollector.Configuration.Format=cobertura"
    "DataCollectionRunSettings.DataCollectors.DataCollector.Configuration.Exclude=""[Tests*]*,[xunit*]*"""
    "DataCollectionRunSettings.DataCollectors.DataCollector.Configuration.ExcludeByAttribute=""GeneratedCodeAttribute,ExcludeFromCodeCoverageAttribute"""
)

& dotnet @testArgs

$exitCode = $LASTEXITCODE

# ─── Copie des fichiers de couverture ───
Write-Host ""
Write-Host "[3/4] Collecte des fichiers de couverture..." -ForegroundColor Yellow

$coverageFiles = Get-ChildItem -Path (Join-Path $projectRoot "TestResults") -Filter "coverage.cobertura.xml" -Recurse
if ($coverageFiles.Count -eq 0) {
    Write-Host "  ⚠ Aucun fichier de couverture trouvé" -ForegroundColor Red
} else {
    $i = 0
    foreach ($file in $coverageFiles) {
        $i++
        Copy-Item $file.FullName (Join-Path $coverageDir "coverage_$i.xml")
    }
    Write-Host "  ✓ $i fichier(s) de couverture collecté(s)" -ForegroundColor Green
}
Write-Host ""

# ─── Génération du rapport HTML ───
if (-not $SkipReport -and $coverageFiles.Count -gt 0) {
    Write-Host "[4/4] Génération du rapport HTML de couverture..." -ForegroundColor Yellow

    # Trouver ReportGenerator
    $reportGenTool = Get-Command reportgenerator -ErrorAction SilentlyContinue
    if (-not $reportGenTool) {
        Write-Host "  Installation de ReportGenerator en outil global..." -ForegroundColor DarkGray
        & dotnet tool install -g dotnet-reportgenerator-globaltool 2>$null
        $reportGenTool = Get-Command reportgenerator -ErrorAction SilentlyContinue
    }

    if ($reportGenTool) {
        $coverageXmls = (Get-ChildItem -Path $coverageDir -Filter "*.xml" | ForEach-Object { $_.FullName }) -join ";"

        & reportgenerator `
            "-reports:$coverageXmls" `
            "-targetdir:$reportDir" `
            "-reporttypes:Html;Badges;TextSummary" `
            "-assemblyfilters:+Core;+Infrastructure;+Api;-Tests" `
            "-classfilters:-*Migrations*"

        Write-Host ""
        Write-Host "  ✓ Rapport HTML généré : $reportDir\index.html" -ForegroundColor Green

        # Afficher le résumé texte
        $summaryFile = Join-Path $reportDir "Summary.txt"
        if (Test-Path $summaryFile) {
            Write-Host ""
            Write-Host "───── RÉSUMÉ DE COUVERTURE ─────" -ForegroundColor Cyan
            Get-Content $summaryFile | Write-Host
            Write-Host "────────────────────────────────" -ForegroundColor Cyan
        }
    } else {
        Write-Host "  ⚠ ReportGenerator non trouvé. Installez-le avec :" -ForegroundColor Red
        Write-Host "    dotnet tool install -g dotnet-reportgenerator-globaltool" -ForegroundColor DarkGray
    }
} else {
    Write-Host "[4/4] Génération de rapport ignorée" -ForegroundColor DarkGray
}

Write-Host ""
Write-Host "============================================" -ForegroundColor Cyan
if ($exitCode -eq 0) {
    Write-Host "  ✓ Tous les tests sont passés !" -ForegroundColor Green
} else {
    Write-Host "  ✗ Certains tests ont échoué (code: $exitCode)" -ForegroundColor Red
}
Write-Host "============================================" -ForegroundColor Cyan
Write-Host ""
Write-Host "Rapports disponibles :" -ForegroundColor Yellow
Write-Host "  TRX  : $(Join-Path $projectRoot 'TestResults\TestResults.trx')" -ForegroundColor DarkGray
Write-Host "  HTML  : $(Join-Path $reportDir 'index.html')" -ForegroundColor DarkGray
Write-Host "  Badges: $(Join-Path $reportDir 'badge_combined.svg')" -ForegroundColor DarkGray
Write-Host ""

exit $exitCode
