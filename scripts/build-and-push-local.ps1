param(
    [Parameter(Mandatory=$true)]
    [string]$AcrName
)

Set-Location -Path $PSScriptRoot\..\

# Build
Write-Host "Building events-api image..."
docker build -t events-api:local -f Dockerfile .

# Login to ACR
Write-Host "Logging in to ACR: $AcrName"
az acr login --name $AcrName

# Tag and push
$fullTag = "$($AcrName).azurecr.io/events-api:latest"
Write-Host "Tagging as $fullTag"
docker tag events-api:local $fullTag
Write-Host "Pushing $fullTag"
docker push $fullTag

Write-Host "Done. Image pushed to $fullTag"