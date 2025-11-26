#!/usr/bin/env bash
set -euo pipefail

if [ -z "${1-}" ]; then
  echo "Usage: $0 <ACR_NAME>"
  exit 1
fi

ACR_NAME="$1"

echo "Building events-api image..."
docker build -t events-api:local -f Dockerfile .

echo "Logging in to ACR: $ACR_NAME"
az acr login --name "$ACR_NAME"

FULL_TAG="$ACR_NAME.azurecr.io/events-api:latest"
echo "Tagging as $FULL_TAG"
docker tag events-api:local "$FULL_TAG"
echo "Pushing $FULL_TAG"
docker push "$FULL_TAG"

echo "Done. Image pushed to $FULL_TAG"