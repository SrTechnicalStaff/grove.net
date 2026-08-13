param(
    [string]$Configuration = "Release"
)

$ErrorActionPreference = "Stop"
$repositoryRoot = Split-Path -Parent $PSScriptRoot
$applicationProject = Join-Path $repositoryRoot "src/GroveApp/GroveApp.csproj"
$unitTestProject = Join-Path $repositoryRoot "tests/GroveApp.Tests/GroveApp.Tests.csproj"
$uiTestProject = Join-Path $repositoryRoot "tests/GroveApp.UiTests/GroveApp.UiTests.csproj"
$publishDirectory = Join-Path $repositoryRoot "GroveApp-Release"
$executablePath = Join-Path $publishDirectory "GroveApp.exe"

function Invoke-DotNet {
    param([Parameter(ValueFromRemainingArguments = $true)][string[]]$Arguments)

    & dotnet @Arguments
    if ($LASTEXITCODE -ne 0) {
        throw "dotnet $($Arguments -join ' ') failed with exit code $LASTEXITCODE"
    }
}

Invoke-DotNet build $applicationProject --configuration $Configuration
Invoke-DotNet test $unitTestProject --configuration $Configuration
Invoke-DotNet publish $applicationProject --configuration $Configuration --output $publishDirectory

if (-not (Test-Path -LiteralPath $executablePath -PathType Leaf)) {
    throw "The canonical release executable was not produced at $executablePath"
}

$env:GROVE_RELEASE_EXE = $executablePath
Invoke-DotNet test $uiTestProject --configuration $Configuration

$hash = Get-FileHash -LiteralPath $executablePath -Algorithm SHA256
$manifest = [ordered]@{
    commit = (git -C $repositoryRoot rev-parse HEAD).Trim()
    configuration = $Configuration
    executable = $executablePath
    sha256 = $hash.Hash
    verifiedAtUtc = [DateTime]::UtcNow.ToString("O")
}

$manifest | ConvertTo-Json | Set-Content -LiteralPath (Join-Path $publishDirectory "build-manifest.json") -Encoding utf8
