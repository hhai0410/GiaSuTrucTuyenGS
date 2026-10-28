. "$PSScriptRoot/Common.ps1"
& $dotnet test tests/GiaSuOnline.IntegrationTests/GiaSuOnline.IntegrationTests.csproj -m:1 --logger 'trx;LogFileName=sprint1.trx'
Assert-Command
