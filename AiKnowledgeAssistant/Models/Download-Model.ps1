$modelsDir = Join-Path -Path $PSScriptRoot -ChildPath "Models"

if (-Not (Test-Path -Path $modelsDir)) {
    New-Item -ItemType Directory -Path $modelsDir | Out-Null
    Write-Host "Utworzono folder: $modelsDir" -ForegroundColor Green
}

Write-Host "Pobieranie pliku model.onnx (to może zająć chwilę, waży kilkaset MB)..." -ForegroundColor Cyan
$modelUrl = "https://huggingface.co/BAAI/bge-reranker-base/resolve/main/onnx/model.onnx?download=true"
$modelOutPath = Join-Path -Path $modelsDir -ChildPath "model.onnx"
Invoke-WebRequest -Uri $modelUrl -OutFile $modelOutPath

Write-Host "Pobieranie pliku tokenizer.json..." -ForegroundColor Cyan
$tokenizerUrl = "https://huggingface.co/BAAI/bge-reranker-base/resolve/main/tokenizer.json?download=true"
$tokenizerOutPath = Join-Path -Path $modelsDir -ChildPath "tokenizer.json"
Invoke-WebRequest -Uri $tokenizerUrl -OutFile $tokenizerOutPath

Write-Host "Gotowe! Wszystkie pliki zostały pobrane do folderu Models." -ForegroundColor Green