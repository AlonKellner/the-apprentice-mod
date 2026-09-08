#!/bin/bash
# Generate/refresh the Godot .import sidecars for the mod's art, then report anything unimported.
#
# Every PNG under TheUnderstudy/ needs a committed <file>.import sidecar or the export preset will not
# pack it — the asset silently does not ship. The sidecar must be produced by the SAME Godot build the
# game uses (MegaDot 4.5.1); a newer editor writes a pack format the game rejects at mod-load, which
# fails long after the build succeeded.
#
# Usage:
#   scripts/import-art.sh          # import, then list assets still missing a sidecar
#   scripts/import-art.sh --check  # report only, import nothing (for CI / pre-commit)
set -euo pipefail

ROOT="$(cd "$(dirname "${BASH_SOURCE[0]}")/.." && pwd)"
cd "$ROOT"

ASSETS="$ROOT/TheUnderstudy"
GODOT="${GodotPath:-$HOME/Applications/MegaDot.app/Contents/MacOS/Godot}"

step() { printf '\n\033[1;36m==> %s\033[0m\n' "$1"; }
die()  { printf '\033[1;31mERROR: %s\033[0m\n' "$1" >&2; exit 1; }

if [ "${1:-}" != "--check" ]; then
  [ -x "$GODOT" ] || die "Godot (MegaDot) not found at $GODOT — set GodotPath, see Directory.Build.props."
  step "Importing assets with $("$GODOT" --version 2>/dev/null | head -1)"
  # Godot reliably writes the sidecars and then crashes on headless shutdown; the exit code is noise.
  "$GODOT" --headless --import --path "$ROOT" >/dev/null 2>&1 || true
fi

step "Checking .import coverage under TheUnderstudy/"
missing=0
while IFS= read -r -d '' img; do
  if [ ! -f "$img.import" ]; then
    printf '  \033[1;33mno .import\033[0m  %s\n' "${img#$ROOT/}"
    missing=$((missing + 1))
  fi
done < <(find "$ASSETS" -type f \( -iname '*.png' -o -iname '*.jpg' -o -iname '*.svg' -o -iname '*.webp' \) -print0)

# Source files must never reach the pack: export_filter is "all_resources", so a .kra/.psd/.blend sitting
# in an un-ignored folder ships inside the .pck. A .gdignore in the folder is what keeps it out.
step "Checking source folders are hidden from the pack"
leaks=0
while IFS= read -r -d '' src; do
  # A .gdignore makes Godot skip the whole subtree, so any ancestor up to the project root counts.
  dir="$(dirname "$src")"; ignored=0; probe="$dir"
  while [ "$probe" != "/" ] && [ "${#probe}" -ge "${#ROOT}" ]; do
    [ -f "$probe/.gdignore" ] && { ignored=1; break; }
    probe="$(dirname "$probe")"
  done
  if [ "$ignored" -eq 0 ]; then
    printf '  \033[1;31mwould ship\033[0m  %s  (add %s/.gdignore)\n' "${src#$ROOT/}" "${dir#$ROOT/}"
    leaks=$((leaks + 1))
  fi
done < <(find "$ASSETS" "$ROOT/art" -type f \( -iname '*.kra' -o -iname '*.psd' -o -iname '*.blend' -o -iname '*.aseprite' -o -iname '*.xcf' \) -print0 2>/dev/null)

printf '\n'
[ "$missing" -eq 0 ] && printf '  all images have .import sidecars\n' || printf '  \033[1;33m%d image(s) missing a sidecar — commit them alongside the PNG\033[0m\n' "$missing"
[ "$leaks" -eq 0 ]   && printf '  no source files would ship\n'       || printf '  \033[1;31m%d source file(s) would ship in the pack\033[0m\n' "$leaks"

[ "$missing" -eq 0 ] && [ "$leaks" -eq 0 ] || exit 1
