# Library Template

A minimal, buildable .NET 8 class library with an xUnit test project — the starting point
for a reusable library (a NuGet package, a shared internal library) rather than a runnable
service.

## Structure

```
Library.sln
src/Library.Core/       -- the library itself
tests/Library.Core.Tests/  -- xUnit tests
```

`src/Library.Core/Result.cs` is a working example (the
[`result-pattern`](../../../skills/architecture/result-pattern.md) skill's type) so the
template isn't empty — replace it with your library's actual contents.

## Using this template

1. Copy this folder to your new project's location and rename `Library`/`Library.Core`
   throughout (solution file, project files, namespaces) to your library's actual name.
2. Vendor the toolkit at `.ai-dotnet/` per the root [README.md](../../../README.md#installing-in-a-net-project)
   and fill in `.ai-dotnet/config.yaml` with `project.type: library`.
3. Delete `Result.cs`/`ResultTests.cs` once you've replaced them with real content, or keep
   the pattern if it fits your library.

## Verify it builds

```bash
dotnet test
```

Applicable toolkit rules: [`rules/csharp.md`](../../../rules/csharp.md),
[`rules/testing.md`](../../../rules/testing.md),
[`rules/general.md`](../../../rules/general.md). This template does not need
[`rules/architecture.md`](../../../rules/architecture.md)'s layering rules — a single-project
library has no layers to enforce.
