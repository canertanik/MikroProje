$appDir = "C:\Users\caner44\Desktop\MikroProje\MikroProje.Application\Features"
$testDir = "C:\Users\caner44\Desktop\MikroProje\MikroProje.Tests\Handlers"

$appHandlers = Get-ChildItem -Path $appDir -Recurse -Filter *Handler.cs
$testHandlers = Get-ChildItem -Path $testDir -Recurse -Filter *HandlerTests.cs

$testNames = $testHandlers | Select-Object -ExpandProperty BaseName | ForEach-Object { $_ -replace 'Tests$', '' }

foreach ($handler in $appHandlers) {
    if ($testNames -contains $handler.BaseName) { continue }

    $lines = Get-Content $handler.FullName
    
    $nsLine = $lines | Where-Object { $_ -match "^namespace\s+(MikroProje\.Application\.Features\.(.+?)\.(Commands|Queries).*);" }
    if (-not $nsLine) { continue }
    $nsLine -match "^namespace\s+(MikroProje\.Application\.Features\.(.+?)\.(Commands|Queries).*);" | Out-Null
    $featureName = $matches[2]
    
    $handlerName = $handler.BaseName
    
    # Get constructor parameters
    $ctorLine = $lines | Where-Object { $_ -match "public $handlerName\((.+)\)" }
    $mocks = @()
    $ctorArgs = @()
    if ($ctorLine) {
        $ctorLine -match "public $handlerName\((.+)\)" | Out-Null
        $argsStr = $matches[1]
        $args = $argsStr -split "," | ForEach-Object { $_.Trim() }
        
        foreach ($arg in $args) {
            $parts = $arg -split " "
            $type = $parts[0]
            $name = $parts[1]
            if ($type -match "IMapper") {
                $ctorArgs += "Mapper"
            } elseif ($type -match "IConfiguration") {
                $mocks += "[Type]IConfiguration"
                $ctorArgs += "_mockIConfiguration.Object"
            } elseif ($type -match "IApplicationDbContext") {
                $mocks += "[Type]IApplicationDbContext"
                $ctorArgs += "_mockIApplicationDbContext.Object"
            } elseif ($type -match "IExcelExportService") {
                $mocks += "[Type]IExcelExportService"
                $ctorArgs += "_mockIExcelExportService.Object"
            } elseif ($type -match "IPdfExportService") {
                $mocks += "[Type]IPdfExportService"
                $ctorArgs += "_mockIPdfExportService.Object"
            } elseif ($type -match "ITokenService|ICurrentUserService") {
                $mocks += "[Type]$type"
                $ctorArgs += "_mock$type.Object"
            } else {
                $mocks += "[Type]$type"
                $ctorArgs += "_mock$type.Object"
            }
        }
    }
    
    $targetDir = Join-Path $testDir $featureName
    if (-not (Test-Path $targetDir)) {
        New-Item -ItemType Directory -Path $targetDir | Out-Null
    }
    
    $testFile = Join-Path $targetDir "$($handlerName)Tests.cs"
    
    $content = @"
using FluentAssertions;
using Moq;
using Moq.EntityFrameworkCore;
using MikroProje.Application.Interfaces;
using MikroProje.Application.Common.Excel;
using MikroProje.Application.Common.Pdf;
using Microsoft.Extensions.Configuration;
using MikroProje.Tests.Common;

namespace MikroProje.Tests.Handlers.$featureName;

public class $($handlerName)Tests : TestBase
{
"@
    foreach ($mock in $mocks) {
        $mockType = $mock -replace '\[Type\]', ''
        $content += "`r`n    private readonly Mock<$mockType> _mock$mockType;"
    }
    
    $content += "`r`n    private readonly MikroProje.Application.Features.$featureName.Commands.*.$handlerName _handler; // Namespace may need fix" -replace '\*', 'TODO'
    
    $content += @"

    public $($handlerName)Tests()
    {
"@
    foreach ($mock in $mocks) {
        $mockType = $mock -replace '\[Type\]', ''
        $content += "`r`n        _mock$mockType = new Mock<$mockType>();"
    }
    
    $ctorArgsStr = $ctorArgs -join ", "
    $content += @"

        _handler = new MikroProje.Application.Features.$featureName.TODO.$handlerName($ctorArgsStr);
    }

    [Fact]
    public async Task Handle_ShouldExecuteSuccessfully()
    {
        // Arrange (TODO)

        // Act (TODO)

        // Assert (TODO)
    }
}
"@

    Set-Content $testFile -Value $content -Encoding UTF8
    Write-Host "Generated $($handlerName)Tests.cs"
}
