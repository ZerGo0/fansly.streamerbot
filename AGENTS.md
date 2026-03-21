# AGENTS.md

This file provides guidance to Codex CLI when working with code in this repository.

## Project Overview
- Name: Fansly Streamer.bot Extension
- Description: A small Streamer.bot integration that joins a Fansly chat websocket, turns inbound events into custom Streamer.bot triggers, and optionally sends chat messages through Fansly's HTTP API.
- Key notes:
  - The repository is flat. The main code surface is the three root C# snippets: `onOpen.cs`, `onMessage.cs`, and `sendMessage.cs`.
  - This repo does not contain a `.csproj`, solution file, package manifest, or CI workflow. Real compile/runtime validation happens when the snippets are imported into Streamer.bot.

## Repository-Wide Rules
- Keep the custom trigger names and ids aligned across `onOpen.cs`, `onMessage.cs`, `README.md`, and `docs/triggers.md`.
- Keep persisted global variable names aligned across code and docs. The repo currently documents and uses `fanslyChatroomId`, `fanslyAuthToken`, and `fanslyLastSentMessage`.
- Treat `docs/protocol.md` as a captured reference for observed Fansly payloads and request headers. If payload parsing or send headers change, update that file in the same change.
- Do not add repo-level build tooling or package manifests unless the user explicitly asks for that setup.

## High-Level Architecture
- `onOpen.cs`: runs when the websocket connects, registers the `Fansly Message`, `Fansly Tip`, and `Fansly Subscription` custom triggers, reads `fanslyChatroomId`, joins the chatroom, and starts the websocket ping loop.
- `onMessage.cs`: parses websocket payloads, distinguishes single vs bulk events, maps chat and subscription payloads into Streamer.bot trigger arguments, and dispatches `fanslyMessage`, `fanslyTip`, and `fanslySubscription`.
- `sendMessage.cs`: reads the outbound message plus auth/chatroom values, sends the HTTP request to Fansly, and stores `fanslyLastSentMessage`.
- `docs/`: operator-facing setup, trigger reference, troubleshooting, protocol notes, and example actions.
- Canonical sources of truth:
  - `docs/triggers.md` for the exposed trigger variables.
  - `docs/protocol.md` for captured websocket and HTTP payload shapes.
  - `docs/setup/message-sending.md` and `docs/setup/chatroom.md` for required persisted globals and operator setup.

## Repository Workflow
- This repository ships source snippets and documentation, not a runnable local application.
- Changes to inbound event parsing usually require synchronized updates across `onMessage.cs`, `docs/triggers.md`, and any affected troubleshooting or example docs.
- Changes to outbound message sending usually require synchronized updates across `sendMessage.cs`, `docs/setup/message-sending.md`, `docs/protocol.md`, and relevant troubleshooting steps.

## Project Guidelines
### Root Streamer.bot snippets
- Language: C#
- Framework/runtime: Streamer.bot inline C# using the `CPH` API
- Package manager: none in-repo
- Important packages/APIs: `Newtonsoft.Json` in `onMessage.cs` and `sendMessage.cs`; `System.Net.Http` in `sendMessage.cs`; Streamer.bot `CPH` globals, args, logging, websocket, and trigger APIs throughout
- Checks:
  - `git diff --check`
  - No repo-local compiler, linter, or formatter command exists. If compile validation is needed, it must be done by importing the edited snippets into Streamer.bot outside this repository.
- Rules / conventions:
  - Preserve the existing `CPHInline` entrypoint shape used by Streamer.bot imports.
  - Prefer extending the existing parsing helpers and trigger argument builders in `onMessage.cs` instead of adding parallel parsing paths.
  - When adding or renaming trigger args, update the matching docs in the same change.
- Useful files:
  - `onOpen.cs`
  - `onMessage.cs`
  - `sendMessage.cs`
  - `docs/triggers.md`
  - `docs/protocol.md`

### Documentation
- Language: Markdown
- Framework/runtime: static repository docs
- Package manager: none in-repo
- Checks:
  - `git diff --check`
- Rules / conventions:
  - Keep setup instructions specific to Streamer.bot's UI labels and persisted global names used by the code.
  - Keep example actions consistent with the bundled `Fansly Send Message` action name documented in the repo.
- Useful files:
  - `README.md`
  - `docs/setup.md`
  - `docs/setup/installation.md`
  - `docs/setup/chatroom.md`
  - `docs/setup/message-sending.md`
  - `docs/troubleshooting.md`
  - `docs/examples/README.md`

## Key Architectural Patterns
- Websocket handling is split by lifecycle: `onOpen.cs` owns registration and room join; `onMessage.cs` owns payload parsing and trigger dispatch.
- Trigger dispatch is dictionary-based. Preserve the documented variable names because Streamer.bot actions consume them by string key.
- Metadata and attachment parsing in `onMessage.cs` already centralize Fansly-specific translation. Extend those helpers instead of duplicating JSON traversal inline.
- Outbound sending in `sendMessage.cs` prefers action args first (`message`, `rawInput`, `chatRoomId`, `authToken`) and falls back to persisted globals. Preserve that override order unless the user asks to change it.

- **ALWAYS** at the end of your turn, ask a follow-up question for the next logical step (**DON'T** ask questions like "Should I run tests?" or "Should I lint?", only ask questions that are relevant to the task at hand)
