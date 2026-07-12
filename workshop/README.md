# Publishing to the Steam Workshop

This mod is published to the Slay the Spire 2 Steam Workshop with **megacrit's
`ModUploader` CLI** (a Steamworks wrapper). It is _not_ published through the game or the
Alchyr template — the uploader is a separate download.

- **Live item:** `The Understudy WIP` — https://steamcommunity.com/sharedfiles/filedetails/?id=3763287462
- **Item ID:** `3763287462` (also stored in `TheUnderstudyWIP/mod_id.txt`)
- **Visibility:** `unlisted` (hidden from search; anyone with the link can Subscribe)

## Layout

```
workshop/
├── .gitignore                 # ignores per-upload build outputs + logs
├── README.md                  # this file
└── TheUnderstudyWIP/          # the upload "workspace"
    ├── workshop.json          # store metadata + visibility (TRACKED)
    ├── image.png              # Workshop thumbnail, must be <1MB (TRACKED)
    ├── mod_id.txt             # Steam item ID, written on first upload (TRACKED — do not delete)
    └── content/               # the actual mod files uploaded (build outputs, gitignored)
        ├── TheUnderstudy.json
        ├── TheUnderstudy.pck
        └── TheUnderstudy.dll  # NOTE: no .pdb in a release upload
```

The `ModUploader` binary itself lives in `tools/mod-uploader/` (gitignored — it's a ~14MB
platform binary; re-download it from the release below).

## One-time setup

1. Download the macOS arm64 build `ModUploader-osx-arm64.zip` from
   https://github.com/megacrit/sts2-mod-uploader/releases (v0.2.0 used here) into
   `tools/mod-uploader/` and unzip it.
2. Clear the Gatekeeper quarantine so the unsigned binary runs:
   ```
   xattr -dr com.apple.quarantine tools/mod-uploader
   ```

## Publishing / updating (the repeatable process)

Run from the repo root. **Steam must be running and logged in** — the uploader publishes
the item under the logged-in Steam account.

```bash
# 1. Rebuild the mod fresh (re-exports the PCK via MegaDot, copies to the game mods/ folder)
~/.dotnet/dotnet publish TheUnderstudy.csproj

# 2. Refresh the workspace content from the freshly-built artifacts (NO .pdb)
MODS="$HOME/Library/Application Support/Steam/steamapps/common/Slay the Spire 2/SlayTheSpire2.app/Contents/MacOS/mods"
cp "$MODS/TheUnderstudy.json" "$MODS/TheUnderstudy.pck" "$MODS/TheUnderstudy.dll" \
   workshop/TheUnderstudyWIP/content/

# 3. (optional) edit workshop.json — bump "changeNote", or flip "visibility" to "public"
#    when ready to release.

# 4. Upload (mod_id.txt makes this update the SAME item)
cd tools/mod-uploader
./ModUploader upload -w "$PWD/../../workshop/TheUnderstudyWIP"
```

On success the tool prints the item URL and opens it in Steam. On failure, read
`mod-uploader.log` in `tools/mod-uploader/`.

## `workshop.json` fields

| Field                | Notes |
|----------------------|-------|
| `title`              | Store title. |
| `description`        | Store description. |
| `visibility`         | `private` \| `friends_only` \| `unlisted` \| `public`. `unlisted` = link-only. |
| `changeNote`         | Shown to subscribers for this update. |
| `tags`               | Search tags. (`"Tools & APIs"` is reserved for tool/API mods.) |
| `dependencies`       | Workshop item IDs of required mods. `3737335127` = **BaseLib** (required). |
| `contentDescriptors` | Mature-content flags; empty here. |

Most fields can be set to `null` / omitted to leave them unchanged on re-upload.

## Sharing with testers

Send the item URL. Testers click **Subscribe** (and Subscribe to **BaseLib** if they don't
have it — the dependency prompts them), then launch STS2; "The Understudy" appears in
character select.
