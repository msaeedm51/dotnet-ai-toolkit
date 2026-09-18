# Prompt: Refactoring

## Purpose
Change code structure without changing behavior, following
[`workflows/refactoring.md`](../../workflows/refactoring.md)'s safety-net-first discipline.

## When to Use
Any structural cleanup, extraction, or reorganization with no intended behavior change.

## Loads
Workflow: [`refactoring`](../../workflows/refactoring.md). Agent:
[`dotnet-developer`](../../agents/dotnet-developer.md).

## Parameters
- `{{TARGET}}` — the code/area being refactored.
- `{{GOAL}}` — what the refactor should achieve (readability, testability, removing
  duplication, preparing for an upcoming change).

## Prompt Template
```
Load the .NET engineering toolkit and follow workflows/refactoring.md for:

Target: {{TARGET}}
Goal: {{GOAL}}

Before changing anything:
1. Identify the current behavior, including edge cases -- don't refactor based on what
   the code is "supposed to" do without verifying what it actually does.
2. Identify every caller of what's being changed.
3. Identify existing test coverage. If it's insufficient for the risk level, add
   characterization tests that pin down current behavior BEFORE restructuring.

Then refactor incrementally, running tests after each step. Preserve behavior exactly --
if you find a genuine bug along the way, stop and tell me rather than silently fixing it
as part of this "pure" refactor. Avoid unrelated changes (reformatting untouched code,
unrelated renames) riding along in the same diff.

Confirm at the end that all pre-existing tests still pass with their original
assertions unchanged.
```

## Expected Output
A structural change with identical external behavior, all pre-existing tests passing
unmodified, and an explicit call-out of any bug discovered along the way (not silently
fixed).

## Related
[`prompts/testing/test-generation.md`](../testing/test-generation.md) for adding
characterization tests first.
