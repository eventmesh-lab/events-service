<#
 .SYNOPSIS
   Reset local repo and Docker resources for this service (non-destructive global).

 .EXAMPLE
   .\scripts\reset-environment.ps1
#>
Set-StrictMode -Version Latest

$ScriptRoot = Split-Path -Parent $MyInvocation.MyCommand.Path
Set-Location (Join-Path $ScriptRoot "..")

Write-Host "==> Updating git repository"
git fetch --all
git reset --hard "$(git rev-parse --abbrev-ref HEAD)"
git clean -fdx

Write-Host "==> Stopping and removing compose resources for this service"
if (Get-Command docker-compose -ErrorAction SilentlyContinue) {
  docker-compose down --rmi all --volumes --remove-orphans
} else {
  docker compose down --rmi all --volumes --remove-orphans
}

Write-Host "==> Pruning unused Docker resources (images, volumes, networks, build cache)"
docker image prune -a -f
docker volume prune -f
docker network prune -f
docker builder prune -a -f

Write-Host "==> Pulling images and bringing the stack up"
if (Get-Command docker-compose -ErrorAction SilentlyContinue) {
  docker-compose pull
  docker-compose up --build --force-recreate -d
} else {
  docker compose pull
  docker compose up --build --force-recreate --pull always --renew-anon-volumes -d
}

Write-Host "==> Done. The service should be recreated from fresh images/builds."
