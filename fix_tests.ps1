# Script para corrigir todos os testes

# Função para adicionar loggerMock a cada instanciação de middleware
function Add-LoggerToMiddleware {
    param (
        [string]$FilePath,
        [string]$MiddlewareType
    )
    
    $content = Get-Content $FilePath -Raw
    
    # Remove $([Environment]::NewLine) se existir
    $content = $content -replace '\$\(\[Environment\]::NewLine\)\s*', "`n        "
    
    # Adiciona loggerMock antes de new MiddlewareType
    $pattern = "(var\s+\w+\s+=\s+CreateNotifyMock\(\);)\s*(var\s+middleware\s+=\s+new\s+$MiddlewareType\()"
    $replacement = "`$1`n        var loggerMock = CreateLoggerMock();`n        `$2"
    $content = $content -replace $pattern, $replacement
    
    # Adiciona loggerMock.Object aos construtores
    $pattern = "(new\s+$MiddlewareType\([^)]+)\)"
    $content = $content -replace $pattern, { 
        param($match)
        $value = $match.Groups[1].Value
        if ($value -notmatch 'loggerMock\.Object') {
            if ($value -match ',') {
                "$value, loggerMock.Object)"
            } else {
                "$value, loggerMock.Object)"
            }
        } else {
            $match.Value
        }
    }
    
    Set-Content $FilePath -Value $content -NoNewline
}

# Corrigir DomainExceptionMiddlewareTests
Add-LoggerToMiddleware -FilePath "src\EBL.FIG.Common.Middleware.Tests\ExceptionHandling\DomainExceptionMiddlewareTests.cs" -MiddlewareType "DomainExceptionMiddleware"

# Corrigir GlobalExceptionMiddlewareTests
Add-LoggerToMiddleware -FilePath "src\EBL.FIG.Common.Middleware.Tests\ExceptionHandling\GlobalExceptionMiddlewareTests.cs" -MiddlewareType "GlobalExceptionMiddleware"

# Corrigir HttpProtocolExceptionMiddlewareTests
Add-LoggerToMiddleware -FilePath "src\EBL.FIG.Common.Middleware.Tests\ExceptionHandling\HttpProtocolExceptionMiddlewareTests.cs" -MiddlewareType "HttpProtocolExceptionMiddleware"

# Corrigir JsonExceptionMiddlewareTests
$content = Get-Content "src\EBL.FIG.Common.Middleware.Tests\ExceptionHandling\JsonExceptionMiddlewareTests.cs" -Raw
$content = $content -replace '\$\(\[Environment\]::NewLine\)\s*', "`n        "
Set-Content "src\EBL.FIG.Common.Middleware.Tests\ExceptionHandling\JsonExceptionMiddlewareTests.cs" -Value $content -NoNewline

Write-Host "Correções aplicadas com sucesso!"
