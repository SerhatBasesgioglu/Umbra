#!/usr/bin/env bash

set -euo pipefail
IFS=$'\n\t'

PROJECT="Umbra.Poc"
SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
REPO_ROOT="$(cd "$SCRIPT_DIR/.." && pwd)"

SRC_DIR="$REPO_ROOT/src"

dotnet restore "$SRC_DIR/$PROJECT" /p:Configuration=Release
dotnet build "$SRC_DIR/$PROJECT" --no-restore -c Release
dotnet publish "$SRC_DIR/$PROJECT" --no-build -c Release -o "$REPO_ROOT/publish/$PROJECT"

docker build -t umbra-metrics:latest --build-arg PUBLISH_PATH="./publish/$PROJECT" -f "$SRC_DIR/$PROJECT/Dockerfile" "$REPO_ROOT"
