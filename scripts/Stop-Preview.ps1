$ErrorActionPreference = 'Stop'
$projectRoot = Split-Path $PSScriptRoot -Parent
$pidFile = Join-Path $projectRoot '.local/web.pid'
if (!(Test-Path -LiteralPath $pidFile)) { Write-Host 'Không có bản xem thử chạy nền được ghi nhận.'; return }
$previewProcessId = [int](Get-Content -LiteralPath $pidFile)
$parent = Get-CimInstance Win32_Process -Filter "ProcessId=$previewProcessId"
$expectedRuntime = Join-Path $projectRoot '.tools/dotnet/dotnet.exe'
if (!$parent -or $parent.ExecutablePath -ne $expectedRuntime) {
    Write-Host 'Bản xem thử đã dừng hoặc mã tiến trình không còn khớp. Không dừng chương trình khác.'
    return
}
$expectedWeb = Join-Path $projectRoot 'src/GiaSuOnline.Web/bin/Debug/net10.0/GiaSuOnline.Web.exe'
$children = Get-CimInstance Win32_Process -Filter "ParentProcessId=$previewProcessId"
foreach ($child in $children) {
    if ($child.ExecutablePath -eq $expectedWeb) { Stop-Process -Id $child.ProcessId }
}
Stop-Process -Id $previewProcessId
Write-Host 'Đã dừng bản xem thử chạy nền. Có thể dùng Start.ps1 hoặc Test.ps1.'
