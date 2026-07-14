#!/bin/bash
# Build, publish (PCK export) and upload The Understudy to the Steam Workshop in one command.
#
# Usage:
#   scripts/publish-workshop.sh              # test -> publish -> assemble -> upload
#   scripts/publish-workshop.sh "change note"  # also update workshop.json changeNote first
#   SKIP_TESTS=1 scripts/publish-workshop.sh # skip the test gate
#
# Requires Steam to be running and logged in (the ModUploader talks to Steamworks).
set -euo pipefail

ROOT="$(cd "$(dirname "${BASH_SOURCE[0]}")/.." && pwd)"
cd "$ROOT"

DOTNET="$HOME/.dotnet/dotnet"
WS="$ROOT/workshop/TheUnderstudyWIP"
CONTENT="$WS/content"
UPLOADER="$ROOT/tools/mod-uploader"
CARDS_JSON="$ROOT/TheUnderstudy/localization/eng/cards.json"
PCK="$ROOT/publish/TheUnderstudy.pck"
DLL="$ROOT/.godot/mono/temp/bin/Debug/TheUnderstudy.dll"

step() { printf '\n\033[1;36m==> %s\033[0m\n' "$1"; }
die()  { printf '\033[1;31mERROR: %s\033[0m\n' "$1" >&2; exit 1; }

# 0. Preconditions -----------------------------------------------------------
[ -x "$DOTNET" ] || die "dotnet not found at $DOTNET"
pgrep -x steam_osx >/dev/null || die "Steam is not running/logged in — start Steam first."
[ -x "$UPLOADER/ModUploader" ] || die "ModUploader missing at $UPLOADER/ModUploader (re-download per workshop/README.md)."
xattr -dr com.apple.quarantine "$UPLOADER" 2>/dev/null || true

# Optional changeNote update (first arg) -------------------------------------
if [ "${1:-}" != "" ]; then
  step "Updating workshop changeNote"
  NOTE="$1" perl -pi -e 's/("changeNote":\s*")(?:[^"\\]|\\.)*(")/$1.$ENV{NOTE}.$2/e' "$WS/workshop.json"
fi

# 1. Test gate ---------------------------------------------------------------
if [ "${SKIP_TESTS:-0}" != "1" ]; then
  step "Running tests"
  "$DOTNET" test "$ROOT/TheUnderstudy.Tests/TheUnderstudy.Tests.csproj" --nologo -clp:ErrorsOnly \
    || die "tests failed — aborting upload."
fi

# 2. Publish (re-export PCK) then build the shipping AnyCPU Debug DLL ---------
# The GodotPublish target ends with a benign headless EditorSettings warning that can make
# `dotnet publish` return non-zero AFTER the pack is written, so don't hard-fail on its exit;
# verify the PCK was actually (re)produced from the current assets instead.
step "Publishing (PCK export via MegaDot)"
"$DOTNET" publish "$ROOT/TheUnderstudy.csproj" -clp:ErrorsOnly || true
[ -f "$PCK" ] || die "no PCK produced at $PCK"
[ "$PCK" -nt "$CARDS_JSON" ] || die "PCK is older than cards.json — PCK export did not run; see the publish output above."

step "Building shipping DLL (AnyCPU Debug top-level)"
"$DOTNET" build "$ROOT/TheUnderstudy.csproj" --nologo -clp:ErrorsOnly || die "DLL build failed."
[ -f "$DLL" ] || die "no DLL produced at $DLL"

# 3. Assemble content/ -------------------------------------------------------
step "Assembling $CONTENT"
mkdir -p "$CONTENT"
cp "$DLL"                      "$CONTENT/TheUnderstudy.dll"
cp "$PCK"                      "$CONTENT/TheUnderstudy.pck"
cp "$ROOT/TheUnderstudy.json" "$CONTENT/TheUnderstudy.json"
printf '   version %s\n' "$(grep -o '"version": *"[^"]*"' "$ROOT/TheUnderstudy.json")"

# 4. Upload ------------------------------------------------------------------
step "Uploading to Steam Workshop (item $(cat "$WS/mod_id.txt" 2>/dev/null))"
cd "$UPLOADER"
./ModUploader upload -w "$WS"

step "Done."
