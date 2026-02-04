#!/bin/bash

set -e

SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
CONFIG_DIR="$SCRIPT_DIR/config"
TARGET_DIR="$HOME/.config"

echo "Deploying config files from $CONFIG_DIR to $TARGET_DIR..."

mkdir -p "$TARGET_DIR"
cp -r "$CONFIG_DIR"/* "$TARGET_DIR/"

echo "✓ Config files deployed!"

