$ErrorActionPreference = 'Stop'
$projectRoot = Split-Path $PSScriptRoot -Parent
Set-Location -LiteralPath $projectRoot
$env:DOTNET_CLI_HOME = Join-Path $projectRoot '.local'
$env:NUGET_PACKAGES = Join-Path $projectRoot '.tools/nuget'
$env:DOTNET_CLI_TELEMETRY_OPTOUT = '1'
$localRuntime = Join-Path $projectRoot '.tools/dotnet/dotnet.exe'
if (Test-Path -LiteralPath $localRuntime) {
    $dotnet = $localRuntime
    $env:DOTNET_ROOT = Split-Path $dotnet -Parent
    $env:PATH = "$env:DOTNET_ROOT;$env:PATH"
} elseif (Get-Command dotnet -ErrorAction SilentlyContinue) {
    $dotnet = (Get-Command dotnet).Source
} else {
    throw 'Chưa có .NET 10 SDK. Chạy scripts/Setup.ps1 trước.'
}
function Assert-Command {
    if ($LASTEXITCODE -ne 0) { throw "Bước trước thất bại (mã $LASTEXITCODE). Đọc thông báo phía trên trước khi tiếp tục." }
}
