param([string]$Application = (Join-Path (Split-Path $PSScriptRoot -Parent) 'dist\GearPower.exe'))
$ErrorActionPreference = 'Stop'
$applicationPath = (Resolve-Path -LiteralPath $Application).Path
$testDirectory = Join-Path ([IO.Path]::GetTempPath()) ('GearPower-history-tests-' + [Guid]::NewGuid().ToString('N'))
New-Item -ItemType Directory -Path $testDirectory | Out-Null
Copy-Item -LiteralPath $applicationPath -Destination (Join-Path $testDirectory ([IO.Path]::GetFileName($applicationPath)))
$compiler = 'C:\Windows\Microsoft.NET\Framework64\v4.0.30319\csc.exe'
$runner = Join-Path $testDirectory 'HistoryIntegrationTests.exe'
& $compiler /nologo /target:exe /langversion:5 ('/r:' + $applicationPath) ('/out:' + $runner) (Join-Path $PSScriptRoot 'HistoryIntegrationTests.cs')
if ($LASTEXITCODE -ne 0) { throw 'History test compilation failed' }
& $runner (Join-Path $testDirectory 'data')
if ($LASTEXITCODE -ne 0) { throw 'History integration tests failed' }
Write-Output ('Isolated test files: ' + $testDirectory)
