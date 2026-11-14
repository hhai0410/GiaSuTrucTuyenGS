param([switch]$SqlServer, [string]$SqlServerName = 'LOQ\SQLEXPRESS')
. "$PSScriptRoot/Common.ps1"
$previousSqlTest = $env:GS_SQL_TEST
$previousSqlServerName = $env:GS_SQL_TEST_SERVER
try {
    if ($SqlServer) { $env:GS_SQL_TEST = '1'; $env:GS_SQL_TEST_SERVER = $SqlServerName }
    & $dotnet test tests/GiaSuOnline.IntegrationTests/GiaSuOnline.IntegrationTests.csproj -m:1 --logger 'trx;LogFileName=regression.trx'
    Assert-Command
} finally { $env:GS_SQL_TEST = $previousSqlTest; $env:GS_SQL_TEST_SERVER = $previousSqlServerName }
