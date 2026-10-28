$ErrorActionPreference = 'Stop'
$projectRoot = Split-Path $PSScriptRoot -Parent
Set-Location -LiteralPath $projectRoot
if (!(Test-Path '.tools/dotnet/dotnet.exe') -and !(Get-Command dotnet -ErrorAction SilentlyContinue)) {
    New-Item -ItemType Directory -Force '.tools' | Out-Null
    Invoke-WebRequest 'https://dot.net/v1/dotnet-install.ps1' -OutFile '.tools/dotnet-install.ps1'
    & '.tools/dotnet-install.ps1' -Version '10.0.403' -InstallDir (Join-Path $projectRoot '.tools/dotnet') -NoPath
    if ($LASTEXITCODE -ne 0) { throw 'Không tải được .NET SDK.' }
}
. "$PSScriptRoot/Common.ps1"
& $dotnet restore GiaSuOnline.sln --disable-parallel
Assert-Command
& $dotnet build GiaSuOnline.sln --no-restore -m:1
Assert-Command
if (!(Test-Path '.tools/ef/dotnet-ef.exe')) {
    & $dotnet tool install dotnet-ef --version 10.0.14 --tool-path .tools/ef
    Assert-Command
}
& '.tools/ef/dotnet-ef.exe' database update --project src/GiaSuOnline.Web --no-build
Assert-Command
& $dotnet dev-certs https --trust
Assert-Command
Write-Host 'Sẵn sàng. Chạy .\scripts\Start.ps1 để mở web. Máy khác cần sửa tên SQL Server trong appsettings.json hoặc dùng biến môi trường.'
