# Adapter: Generic / Any Other Platform

For any AI coding assistant not covered by a dedicated adapter — a newer tool, an internal
company assistant, or one this toolkit hasn't caught up to yet. Also covers installation
alternatives to the git-submodule approach in the root [README.md](../README.md).

## Installing without a git submodule

The root README recommends a submodule pinned to a tag. Two alternatives:

**git subtree** — copies the toolkit's history into your repo instead of referencing it
externally. Simpler for contributors who don't want to think about submodule init/update,
at the cost of a heavier repo and a slightly different update flow:

```bash
git subtree add --prefix=.ai-dotnet https://github.com/<org>/dotnet-ai-toolkit.git v1.0.0 --squash

# later, to update:
git subtree pull --prefix=.ai-dotnet https://github.com/<org>/dotnet-ai-toolkit.git v1.1.0 --squash
```

**Manual sync** — for environments that can't use git submodules/subtrees at all (some
corporate mono repo setups). Clone the toolkit separately and copy `.ai-dotnet/` in with a
small script, tracking the version copied from in a comment at the top of
`.ai-dotnet/VERSION`:

```bash
#!/usr/bin/env bash
TOOLKIT_VERSION="v1.0.0"
rm -rf .ai-dotnet
git clone --depth 1 --branch "$TOOLKIT_VERSION" https://github.com/<org>/dotnet-ai-toolkit.git .ai-dotnet
rm -rf .ai-dotnet/.git
echo "$TOOLKIT_VERSION" > .ai-dotnet/VERSION
```

Re-run this script deliberately to update, the same way you'd bump a submodule pin — check
[`CHANGELOG.md`](../CHANGELOG.md) between versions first.

## Adapting to a new platform

Most AI coding assistants have *some* mechanism for project-level instructions — a config
file the platform auto-loads, a system prompt field, or at minimum a place to paste context
manually. The pattern is the same regardless of the specific mechanism:

1. **Identify the platform's persistent-instructions mechanism**, if any (a file, a
   settings field, an uploaded document). If there isn't one, instructions have to be
   supplied fresh each conversation — note that in your adapter.
2. **Write a short pointer**, not a copy: point at `.ai-dotnet/AGENTS.md` and
   `.ai-dotnet/RULES.md`, following the same minimal content shown in
   [`claude-code.md`](claude-code.md)'s `CLAUDE.md` example. Never paste the full content of
   `AGENTS.md`/skills/rules into the platform's own config — that's what goes stale.
3. **Determine tool capabilities** per
   [`AGENTS.md §4`](../AGENTS.md#4-tool-agnostic-design): does this platform have filesystem
   read access to the repo? Shell/build execution? Git access? Document what's available so
   the pointer file can tell the assistant what to do when a capability is missing (ask the
   user to paste a file, rather than guessing its contents).
4. **Determine selective-loading mechanism**: if the platform supports glob-scoped or
   conditionally-activated instructions (like Cursor/Windsurf/Copilot's path-scoped files),
   map `skills/INDEX.yaml` categories onto them the way
   [`cursor.md`](cursor.md)/[`windsurf.md`](windsurf.md) do. If it only supports one
   always-loaded instructions file, rely on the assistant reading `INDEX.yaml` itself at
   request time (works only if it has filesystem access) or on manual per-conversation
   loading (see the ChatGPT adapter's fallback section for the no-filesystem-access case).
5. **Write the new adapter file** following this structure: entry-point file(s), selective
   loading approach, precedence notes, limitations. Submit it per
   [`CONTRIBUTING.md`](../CONTRIBUTING.md) so future users of that platform don't have to
   redo this.

## Minimal pointer template (works everywhere with any persistent-instructions mechanism)

```
This project uses the dotnet-ai-toolkit, vendored at .ai-dotnet/.

Read .ai-dotnet/AGENTS.md and .ai-dotnet/RULES.md for operating principles and
precedence before non-trivial tasks. Read .ai-dotnet/config.yaml for this project's
actual architecture and stack; if it's missing, follow
.ai-dotnet/workflows/project-discovery.md first instead of guessing.

Select relevant agents/skills via .ai-dotnet/agents/INDEX.yaml and
.ai-dotnet/skills/INDEX.yaml rather than loading everything.

Never invent an API, class, database table, or configuration value -- verify it exists,
or ask.
```

## No persistent instructions mechanism at all

Paste the template above (or the relevant `agents/<agent>.md` +
`skills/<category>/<skill>.md` files directly) at the start of each conversation, along
with the actual code/config being discussed. This is the lowest-capability case per
[`AGENTS.md §4`](../AGENTS.md#4-tool-agnostic-design) — no different in principle from
working with a platform that has no filesystem access, just without even a saved pointer
file to shorten the setup each time.

## Precedence notes

[`RULES.md`](../RULES.md)'s precedence system applies as-is; level 1 (system/platform
safety constraints) is whatever that specific platform enforces, which this toolkit has no
visibility into and never asks to be bypassed.
