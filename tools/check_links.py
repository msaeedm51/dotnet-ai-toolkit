#!/usr/bin/env python3
"""
Checks every relative markdown link in the repository resolves to a real file.
Ignores http(s)/mailto links and pure same-file anchors (#foo). For a link with a path
AND an anchor (foo.md#bar), only the path portion is checked -- anchor correctness isn't
verified.

Usage: python tools/check_links.py
"""
import re
import sys
from pathlib import Path

ROOT = Path(__file__).resolve().parent.parent
LINK_PATTERN = re.compile(r"\[[^\]]*\]\(([^)]+)\)")
SKIP_DIRS = {".git"}


def iter_markdown_files():
    for path in ROOT.rglob("*.md"):
        if any(part in SKIP_DIRS for part in path.parts):
            continue
        yield path


def main():
    errors = []
    checked = 0

    for md_file in iter_markdown_files():
        text = md_file.read_text(encoding="utf-8", errors="replace")
        for match in LINK_PATTERN.finditer(text):
            target = match.group(1).strip()

            if target.startswith(("http://", "https://", "mailto:", "#")):
                continue
            if "{{" in target:
                continue  # template fill-in placeholder, not a real path
            if target.startswith("<") and target.endswith(">"):
                target = target[1:-1]

            # Strip anchor
            path_part = target.split("#", 1)[0]
            if not path_part:
                continue

            checked += 1
            resolved = (md_file.parent / path_part).resolve()
            if not resolved.exists():
                rel_md = md_file.relative_to(ROOT).as_posix()
                errors.append(f"{rel_md}: broken link -> '{target}' (resolved: {resolved})")

    if errors:
        print(f"Checked {checked} relative links. {len(errors)} broken:\n")
        for e in errors:
            print(f"  - {e}")
        sys.exit(1)

    print(f"OK: {checked} relative links across all markdown files resolve correctly.")


if __name__ == "__main__":
    main()
