#!/usr/bin/env python3
"""
Validates the toolkit's self-consistency:
  1. Every path in agents/INDEX.yaml and skills/INDEX.yaml resolves to a real file.
  2. Every agents/*.md and skills/**/*.md file is registered in the matching INDEX.yaml
     (no orphan content the selective-loading system can't discover).
  3. related_skills / related_agents references point at ids that actually exist.
  4. related_rules paths resolve to real files.
  5. No vendor-platform names appear in core knowledge directories (agents/, skills/,
     rules/, workflows/, checklists/, prompts/, templates/) -- those belong only in adapters/.

Exit code is non-zero if any check fails; failures are printed grouped by check.
"""
import re
import sys
from pathlib import Path

try:
    import yaml
except ImportError:
    print("PyYAML is required: pip install pyyaml", file=sys.stderr)
    sys.exit(2)

ROOT = Path(__file__).resolve().parent.parent

VENDOR_TERMS = [
    "Claude", "ChatGPT", "Copilot", "Cursor", "Windsurf", "Gemini", "Anthropic", "OpenAI",
]
CORE_DIRS = [
    "agents", "skills", "rules", "workflows", "checklists", "prompts", "templates",
]
VENDOR_PATTERN = re.compile(r"\b(" + "|".join(re.escape(t) for t in VENDOR_TERMS) + r")\b")
SUPPRESS_MARK = "vendor-name-ok"


def load_index(path: Path):
    if not path.exists():
        return []
    with path.open("r", encoding="utf-8") as f:
        data = yaml.safe_load(f) or []
    if not isinstance(data, list):
        raise ValueError(f"{path} must contain a YAML list")
    return data


def check_index_paths(index_path: Path, entries, errors):
    seen_ids = set()
    seen_paths = set()
    for entry in entries:
        entry_id = entry.get("id")
        entry_path = entry.get("path")
        if not entry_id or not entry_path:
            errors.append(f"{index_path}: entry missing 'id' or 'path': {entry}")
            continue
        if entry_id in seen_ids:
            errors.append(f"{index_path}: duplicate id '{entry_id}'")
        seen_ids.add(entry_id)
        if entry_path in seen_paths:
            errors.append(f"{index_path}: duplicate path '{entry_path}'")
        seen_paths.add(entry_path)
        full = ROOT / entry_path
        if not full.exists():
            errors.append(f"{index_path}: '{entry_id}' points at missing file '{entry_path}'")
    return seen_ids, seen_paths


def check_orphans(dir_path: Path, indexed_paths, errors, glob="**/*.md"):
    for md_file in dir_path.glob(glob):
        rel = md_file.relative_to(ROOT).as_posix()
        if rel.endswith("INDEX.yaml"):
            continue
        if rel not in indexed_paths:
            errors.append(f"orphan file not registered in INDEX.yaml: {rel}")


def check_cross_references(agent_entries, skill_entries, errors):
    agent_ids = {e.get("id") for e in agent_entries if e.get("id")}
    skill_ids = {e.get("id") for e in skill_entries if e.get("id")}
    for entry in agent_entries:
        for skill_id in entry.get("loads_skills", []) or []:
            if skill_id not in skill_ids:
                errors.append(f"agents/INDEX.yaml: '{entry.get('id')}' loads_skills references unknown skill '{skill_id}'")
    for entry in skill_entries:
        for rel_skill in entry.get("related_skills", []) or []:
            if rel_skill not in skill_ids:
                errors.append(f"skills/INDEX.yaml: '{entry.get('id')}' related_skills references unknown skill '{rel_skill}'")
        for rel_agent in entry.get("related_agents", []) or []:
            if rel_agent not in agent_ids:
                errors.append(f"skills/INDEX.yaml: '{entry.get('id')}' related_agents references unknown agent '{rel_agent}'")
        for rel_rule in entry.get("related_rules", []) or []:
            if not (ROOT / rel_rule).exists():
                errors.append(f"skills/INDEX.yaml: '{entry.get('id')}' related_rules references missing file '{rel_rule}'")


def check_vendor_neutrality(errors):
    for dirname in CORE_DIRS:
        dir_path = ROOT / dirname
        if not dir_path.exists():
            continue
        for md_file in dir_path.glob("**/*.md"):
            with md_file.open("r", encoding="utf-8") as f:
                for lineno, line in enumerate(f, start=1):
                    if SUPPRESS_MARK in line:
                        continue
                    match = VENDOR_PATTERN.search(line)
                    if match:
                        rel = md_file.relative_to(ROOT).as_posix()
                        errors.append(
                            f"{rel}:{lineno}: vendor-specific term '{match.group(1)}' in core "
                            f"content (only adapters/ may name platforms)"
                        )


def main():
    errors = []

    agent_entries = load_index(ROOT / "agents" / "INDEX.yaml")
    skill_entries = load_index(ROOT / "skills" / "INDEX.yaml")

    _, agent_paths = check_index_paths(ROOT / "agents/INDEX.yaml", agent_entries, errors)
    _, skill_paths = check_index_paths(ROOT / "skills/INDEX.yaml", skill_entries, errors)

    agents_dir = ROOT / "agents"
    if agents_dir.exists():
        for md_file in agents_dir.glob("*.md"):
            rel = md_file.relative_to(ROOT).as_posix()
            if rel not in agent_paths:
                errors.append(f"orphan file not registered in agents/INDEX.yaml: {rel}")

    skills_dir = ROOT / "skills"
    if skills_dir.exists():
        check_orphans(skills_dir, skill_paths, errors)

    check_cross_references(agent_entries, skill_entries, errors)
    check_vendor_neutrality(errors)

    if errors:
        print(f"Validation failed with {len(errors)} issue(s):\n")
        for e in errors:
            print(f"  - {e}")
        sys.exit(1)

    print(f"OK: {len(agent_entries)} agents, {len(skill_entries)} skills indexed and consistent.")


if __name__ == "__main__":
    main()
