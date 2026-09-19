# Adapter: ChatGPT

ChatGPT spans several surfaces with very different tool-access levels — from the plain web
chat (no filesystem access at all) to a Custom GPT with Retrieval/Actions, to an
agentic/coding-focused mode with a sandboxed filesystem, to a developer's own application
built on the API. Treat filesystem access as the deciding factor for which approach below
applies, per [`AGENTS.md §4`](../AGENTS.md#4-tool-agnostic-design).

## If ChatGPT has filesystem/repo access

(An agentic coding mode with a sandboxed working directory, or a Custom GPT Action wired to
your repo.) Use the same approach as the Claude Code adapter: point it at
`.ai-dotnet/AGENTS.md` and `.ai-dotnet/RULES.md` and let it read `INDEX.yaml` files to
select skills/agents. If the surface supports a project-level instructions file, put the
same short pointer content shown in [`claude-code.md`](claude-code.md)'s `CLAUDE.md` example
there.

## If ChatGPT has no filesystem access (plain chat)

Two options, in order of preference:

**Custom GPT with Retrieval.** Create a Custom GPT, set its Instructions field to the
pointer content below, and upload `AGENTS.md`, `RULES.md`, and the specific `skills/`/
`rules/`/`agents/` files most relevant to your project as Knowledge files. Retrieval lets the
GPT search across them per-request, giving a rough approximation of selective loading (not
as precise as glob/trigger matching, but far better than one giant system prompt).

```text
GPT Instructions:

You are assisting with a .NET project that uses the dotnet-ai-toolkit. Your knowledge
files include AGENTS.md, RULES.md, and a subset of its agents/skills/rules. Before
answering a non-trivial question:
1. Identify which agent (from AGENTS.md's roster) and which skills fit the request.
2. Search your knowledge files for the matching content before answering from general
   knowledge.
3. If the project's actual code isn't visible to you, ask the user to paste the relevant
   file(s) rather than assuming its contents.
4. State assumptions explicitly, per AGENTS.md's anti-hallucination rules.
```

**Manual per-conversation loading.** For a one-off question, paste the specific
`agents/<agent>.md` and `skills/<category>/<skill>.md` files relevant to the task directly
into the conversation, along with the actual project code being discussed — this is the
most reliable option when there's no persistent knowledge/retrieval mechanism, at the cost
of doing the selection yourself instead of letting the model do it.

## If building on the OpenAI API directly

Put a condensed version of `AGENTS.md` (the global behavior rules and the selection
algorithm) in the system message, and wire a tool/function that lets the model read files
from `.ai-dotnet/` on demand (`read_file(path)`), rather than embedding the entire toolkit
in the system prompt. This mirrors Claude Code's approach but requires you to implement the
file-reading tool yourself.

## Precedence notes

Same precedence system as [`RULES.md`](../RULES.md) — OpenAI's own usage policies and
safety behavior sit above anything in the toolkit, per that file's level 1.

## Limitations

- Retrieval-based Custom GPTs approximate selective loading via search relevance, not exact
  trigger matching — expect occasional over- or under-inclusion of skill content.
- Without filesystem access, ChatGPT cannot verify claims about the actual codebase (file
  existence, current schema, installed packages) — per
  [`AGENTS.md §3`](../AGENTS.md#3-anti-hallucination-rules), it should ask for the relevant
  file content rather than assume, and you should expect to paste more context than with a
  filesystem-capable platform.
