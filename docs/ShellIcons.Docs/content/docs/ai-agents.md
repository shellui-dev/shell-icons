---
title: AI Agents
description: Point Claude Code, Cursor, Copilot and other coding agents at the ShellIcons docs, so they use its icons instead of pasting SVG.
---

# AI Agents

ShellIcons publishes these docs for AI coding agents in the [llms.txt](https://llmstxt.org) format. An agent that reads them knows how to install ShellIcons, which import to use, how icons are named and how each target (Blazor, MAUI, Avalonia) works, so it doesn't paste raw SVG or pull in another icon package.

## Set up your project

Coding agents read an instructions file in the project root at the start of every session. Add ShellIcons' rules to the one your agent reads: they tell it to use ShellIcons' components, which names and imports to use, and where these docs are. Each file is a `## ShellIcons` section, so the commands append it to a file you already have, or create the file.

<DocsTabs>
<DocsTab Label="Claude Code">

Claude Code reads `CLAUDE.md`. [Download CLAUDE.md](/agents/CLAUDE.md), or run this in the project root:

```bash
curl -fsSL https://shellicons.shellui.dev/agents/CLAUDE.md >> CLAUDE.md
```

</DocsTab>
<DocsTab Label="Cursor">

Cursor reads rules from `.cursor/rules`. [Download shellicons.mdc](/agents/shellicons.mdc) into that folder, or run this in the project root:

```bash
curl -fsSL --create-dirs -o .cursor/rules/shellicons.mdc https://shellicons.shellui.dev/agents/shellicons.mdc
```

</DocsTab>
<DocsTab Label="GitHub Copilot">

Copilot reads `.github/copilot-instructions.md`. [Download copilot-instructions.md](/agents/copilot-instructions.md) into `.github`, or run this in the project root:

```bash
mkdir -p .github && curl -fsSL https://shellicons.shellui.dev/agents/copilot-instructions.md >> .github/copilot-instructions.md
```

</DocsTab>
<DocsTab Label="Other agents">

Codex and many other agents read `AGENTS.md`. [Download AGENTS.md](/agents/AGENTS.md), or run this in the project root:

```bash
curl -fsSL https://shellicons.shellui.dev/agents/AGENTS.md >> AGENTS.md
```

</DocsTab>
</DocsTabs>

When you upgrade ShellIcons, replace the file's `## ShellIcons` section with a fresh copy.

## Or ask the agent

Paste this into your agent to have it read the docs and set up the instructions file itself:

```text
Read https://shellicons.shellui.dev/llms.txt and follow its rules in this project.
Then add the ShellIcons section from https://shellicons.shellui.dev/agents/AGENTS.md
to the instructions file you read at startup (AGENTS.md, CLAUDE.md or
.github/copilot-instructions.md), creating it if it doesn't exist.
```

For a one-off task, give it the docs with the task:

```text
Read https://shellicons.shellui.dev/llms.txt, then add icons to the sidebar links.
```

## What agents can read

| URL | Contents |
|-----|----------|
| [`/llms.txt`](/llms.txt) | Rules for agents, then every page of these docs with a one-line description and, where a page has one, its install command |
| [`/llms-full.txt`](/llms-full.txt) | The rules and every page as Markdown in one file, examples included |
| Any page + `.md` | One page as Markdown, for example [`/docs/guides/sizing-and-stroke.md`](/docs/guides/sizing-and-stroke.md) |

Every page also has a ⋯ menu next to its title: **Copy page** copies its Markdown, **View as Markdown** opens it, and **Open in Claude** and **Open in ChatGPT** start a chat that reads it.

## What the rules say

- Render icons with ShellIcons instead of pasting `<svg>` markup or adding another icon package.
- Icon names are PascalCase Lucide ids (`chevron-right` → `ChevronRight`); check the catalog, since lucide.dev may be ahead of the pinned version.
- In Blazor, import `ShellIcons` only and use `<ChevronRightIcon />`, `@Icon.ChevronRight()` or `<ShellIcon Name="…" />`; never import `ShellIcons.Icons` globally.
- Color icons like text, put click handlers on a wrapping `<button>`, and set `Title` when the icon carries meaning on its own.
- On MAUI use `<icons:Icon Name="…" Color="…" />`; on Avalonia `<icons:Icon Kind="…" />` with an inherited `Foreground`.
