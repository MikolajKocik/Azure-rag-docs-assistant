$ErrorActionPreference = "Stop"

$modelsDir = Join-Path -Path $PSScriptRoot -ChildPath "Models"

if (-not (Test-Path -Path $modelsDir)) {
    New-Item -ItemType Directory -Path $modelsDir | Out-Null
    Write-Host "Created folder: $modelsDir" -ForegroundColor Green
}

$baseUrl = "https://huggingface.co/BAAI/bge-reranker-base/resolve/main"

$files = @(
    @{
        Name = "model.onnx"
        Url  = "$baseUrl/onnx/model.onnx?download=true"
    },
    @{
        Name = "sentencepiece.bpe.model"
        Url  = "$baseUrl/sentencepiece.bpe.model?download=true"
    },
    @{
        Name = "tokenizer.json"
        Url  = "$baseUrl/tokenizer.json?download=true"
    },
    @{
        Name = "tokenizer_config.json"
        Url  = "$baseUrl/tokenizer_config.json?download=true"
    },
    @{
        Name = "special_tokens_map.json"
        Url  = "$baseUrl/special_tokens_map.json?download=true"
    },
    @{
        Name = "config.json"
        Url  = "$baseUrl/config.json?download=true"
    }
)

foreach ($file in $files) {
    $outPath = Join-Path -Path $modelsDir -ChildPath $file.Name

    if (Test-Path -Path $outPath) {
        Write-Host "Skipping existing file: $($file.Name)" -ForegroundColor Yellow
        continue
    }

    Write-Host "Downloading $($file.Name)..." -ForegroundColor Cyan

    $tempPath = "$outPath.tmp"

    try {
        Invoke-WebRequest `
            -Uri $file.Url `
            -OutFile $tempPath `
            -UseBasicParsing

        Move-Item -Path $tempPath -Destination $outPath -Force

        Write-Host "Downloaded: $($file.Name)" -ForegroundColor Green
    }
    catch {
        if (Test-Path -Path $tempPath) {
            Remove-Item -Path $tempPath -Force
        }

        Write-Host "Failed to download: $($file.Name)" -ForegroundColor Red
        throw
    }
}

Write-Host ""
Write-Host "Done. Files are available in: $modelsDir" -ForegroundColor Green