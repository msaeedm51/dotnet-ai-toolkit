#!/usr/bin/env bash
# Bootstraps a consumer project after vendoring this toolkit at .ai-dotnet/.
# Run from the consumer project's ROOT (the directory that CONTAINS .ai-dotnet/), e.g.:
#
#   ./.ai-dotnet/tools/init-project.sh claude-code
#
# Usage: init-project.sh <adapter>
#   adapter: claude-code | chatgpt | cursor | copilot | windsurf | generic
#
# What it does:
#   1. Creates .ai-dotnet/config.yaml from config/config.example.yaml if it doesn't exist.
#   2. Generates the chosen platform's pointer/entry-point file(s), per adapters/<adapter>.md.
#      It never copies toolkit content -- only a short pointer into .ai-dotnet/, matching
#      what's documented in the adapter file.
# It does not overwrite an existing entry-point file -- if one exists, it prints what it
# would have written and exits, so you don't lose project-specific customizations.

set -euo pipefail

ADAPTER="${1:-}"
TOOLKIT_DIR=".ai-dotnet"

if [[ -z "$ADAPTER" ]]; then
  echo "Usage: $0 <claude-code|chatgpt|cursor|copilot|windsurf|generic>" >&2
  exit 1
fi

if [[ ! -d "$TOOLKIT_DIR" ]]; then
  echo "Error: $TOOLKIT_DIR not found. Run this from your project root, after vendoring" >&2
  echo "the toolkit at $TOOLKIT_DIR (see README.md#installing-in-a-net-project)." >&2
  exit 1
fi

# --- Step 1: config.yaml -----------------------------------------------------------------

CONFIG_TARGET="$TOOLKIT_DIR/config.yaml"
if [[ -f "$CONFIG_TARGET" ]]; then
  echo "Kept existing $CONFIG_TARGET (not overwritten)."
else
  cp "$TOOLKIT_DIR/config/config.example.yaml" "$CONFIG_TARGET"
  echo "Created $CONFIG_TARGET from config.example.yaml -- fill in your project's actual"
  echo "architecture and stack (schema: $TOOLKIT_DIR/config/config.schema.json)."
fi

# --- Step 2: platform entry-point file(s) -------------------------------------------------

write_if_absent() {
  local path="$1"
  local content="$2"
  if [[ -f "$path" ]]; then
    echo "Kept existing $path (not overwritten) -- see $TOOLKIT_DIR/adapters/$ADAPTER.md for the recommended content."
    return
  fi
  mkdir -p "$(dirname "$path")"
  printf '%s\n' "$content" > "$path"
  echo "Created $path"
}

POINTER_CORE="This project uses the dotnet-ai-toolkit, vendored at .ai-dotnet/.

Read .ai-dotnet/AGENTS.md and .ai-dotnet/RULES.md for operating principles and
precedence before non-trivial tasks. Read .ai-dotnet/config.yaml for this project's
actual architecture and stack; if it's missing, follow
.ai-dotnet/workflows/project-discovery.md first instead of guessing.

Select relevant agents/skills via .ai-dotnet/agents/INDEX.yaml and
.ai-dotnet/skills/INDEX.yaml rather than loading everything.

Never invent an API, class, database table, or configuration value -- verify it exists,
or ask.

Project-specific overrides (if any) go below this line -- they take precedence over the
toolkit's generic defaults, per .ai-dotnet/RULES.md."

case "$ADAPTER" in
  claude-code)
    write_if_absent "CLAUDE.md" "# CLAUDE.md

$POINTER_CORE"
    ;;
  copilot)
    write_if_absent ".github/copilot-instructions.md" "$POINTER_CORE"
    ;;
  cursor)
    write_if_absent ".cursor/rules/000-toolkit-core.mdc" "---
description: dotnet-ai-toolkit operating principles -- always active
alwaysApply: true
---

$POINTER_CORE"
    echo "See $TOOLKIT_DIR/adapters/cursor.md to add category-specific auto-attached rules."
    ;;
  windsurf)
    write_if_absent ".windsurf/rules/toolkit-core.md" "---
trigger: always_on
---

$POINTER_CORE"
    echo "See $TOOLKIT_DIR/adapters/windsurf.md to add category-specific glob rules."
    ;;
  chatgpt|generic)
    write_if_absent ".ai-dotnet-pointer.md" "$POINTER_CORE"
    echo "See $TOOLKIT_DIR/adapters/$ADAPTER.md -- this platform likely needs the pointer"
    echo "content pasted manually (into a Custom GPT's instructions, a system prompt, or"
    echo "each conversation) rather than an auto-loaded file."
    ;;
  *)
    echo "Unknown adapter '$ADAPTER'. Expected one of: claude-code, chatgpt, cursor, copilot, windsurf, generic" >&2
    exit 1
    ;;
esac

echo "Done. Next: read $TOOLKIT_DIR/adapters/$ADAPTER.md for anything the generated file doesn't cover."
