[CmdletBinding(PositionalBinding=$false)]
param(
    [string] $Version,
    [string] $BuildNumber,
    [bool] $CreatePackages,
    [bool] $RunTests = $true,
    [string] $PullRequestNumber
)

function CalculateVersion() {
    return $Version
}

Write-Host "Run Parameters:" -ForegroundColor Cyan
Write-Host "Version: $Version"
Write-Host "CreatePackages: $CreatePackages"
Write-Host "RunTests: $RunTests"
Write-Host "Base Version: $(CalculateVersion)"

$packageOutputFolder = "$PSScriptRoot\.nupkgs"
$projectsToBuild =
    'Hydrogen.Prometheus.Client'

$testsToRun =
    'Hydrogen.Prometheus.Client.Tests'

if (!$Version) {
    Write-Host "ERROR: You must supply a -Version . `
  Use -Version `"4.0.0`" for explicit version specification" -ForegroundColor Yellow
    Exit 1
}

if ($PullRequestNumber) {
    Write-Host "Building for a pull request (#$PullRequestNumber), skipping packaging." -ForegroundColor Yellow
    $CreatePackages = $false
}

if ($RunTests) {   
    dotnet restore /ConsoleLoggerParameters:Verbosity=Quiet
    foreach ($project in $testsToRun) {
        Write-Host "Running tests: $project (all frameworks)" -ForegroundColor "Magenta"
        Push-Location ".\tests\$project"

        dotnet xunit
        if ($LastExitCode -ne 0) { 
            Write-Host "Error with tests, aborting build." -Foreground "Red"
            Pop-Location
            Exit 1
        }

        Write-Host "Tests passed!" -ForegroundColor "Green"
        Pop-Location
    }
}

if ($CreatePackages) {
    mkdir -Force $packageOutputFolder | Out-Null
    Write-Host "Clearing existing $packageOutputFolder..." -NoNewline
    Get-ChildItem $packageOutputFolder | Remove-Item
    Write-Host "done." -ForegroundColor "Green"

    Write-Host "Building all packages" -ForegroundColor "Green"
}

foreach ($project in $projectsToBuild) {
    Write-Host "Working on $project`:" -ForegroundColor "Magenta"
    
    Push-Location ".\src\$project"

    $semVer = CalculateVersion
    $targets = "Restore"

    Write-Host "  Restoring " -NoNewline -ForegroundColor "Magenta"
    if ($CreatePackages) {
        $targets += ";Pack"
        Write-Host "and packing " -NoNewline -ForegroundColor "Magenta"
    }
    Write-Host "$project... (Version:" -NoNewline -ForegroundColor "Magenta"
    Write-Host $semVer -NoNewline -ForegroundColor "Cyan"
    Write-Host ")" -ForegroundColor "Magenta"
    

    dotnet msbuild "/t:$targets" "/p:Configuration=Release" "/p:Version=$semVer" "/p:PackageOutputPath=$packageOutputFolder" "/p:CI=true"

    Pop-Location

    Write-Host "Done."
    Write-Host ""
}