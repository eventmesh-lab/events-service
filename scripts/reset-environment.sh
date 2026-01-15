#!/usr/bin/env bash
set -euo pipefail

# Reset local repo and Docker resources for this service (non-destructive global)
# Usage: ./scripts/reset-environment.sh

ROOT="$(cd "$(dirname "$0")/.." && pwd)"
cd "$ROOT"

echo "==> Updating git repository"
git fetch --all
git reset --hard origin/$(git rev-parse --abbrev-ref HEAD)
git clean -fdx

echo "==> Stopping and removing compose resources for this service"
if command -v docker-compose >/dev/null 2>&1; then
  docker-compose down --rmi all --volumes --remove-orphans
else
  docker compose down --rmi all --volumes --remove-orphans
fi

echo "==> Pruning unused Docker resources (images, volumes, networks, build cache)"
docker image prune -a -f
docker volume prune -f
docker network prune -f
docker builder prune -a -f

echo "==> Pulling images and bringing the stack up"
if command -v docker-compose >/dev/null 2>&1; then
  docker-compose pull || true
  docker-compose up --build --force-recreate -d
else
  docker compose pull || true
  docker compose up --build --force-recreate --pull always --renew-anon-volumes -d
fi

echo "==> Done. The service should be recreated from fresh images/builds."
