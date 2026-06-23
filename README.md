# Fansly Streamer.bot Extension

A Streamer.bot extension that adds support for Fansly chat events including messages, tips, subscriptions, room updates, moderation events, goals, settings updates, and session-level websocket events.

## Features

- **Chat Messages** - Fansly chat messages
- **Tips** - Fansly tips
- **Subscriptions** - Fansly subscriptions
- **Room Events** - Chat room updates, moderation, goals, and settings changes
- **Session Events** - Session verification, ping responses, and websocket error events
- **Send Messages** - Send a Fansly chat message from a Streamer.bot action

## Docs

- [Setup](./docs/setup.md)
- [Installation](./docs/setup/installation.md)
- [Chatroom Setup](./docs/setup/chatroom.md)
- [Message Sending Setup](./docs/setup/message-sending.md)
- [Troubleshooting](./docs/troubleshooting.md)
- [Triggers](./docs/triggers.md)
- [Protocol Notes](./docs/protocol.md)
- [Examples](./docs/examples/README.md)

## Source Files

- `onOpen.cs` - Registers Fansly custom triggers and joins the websocket chatroom
- `onMessage.cs` - Parses incoming websocket events, normalizes typed protocol payloads, and dispatches Streamer.bot triggers
- `sendMessage.cs` - Sends a Fansly chat message with the HTTP API

## Validation

This repo includes a C# test project under `tests/` and a GitHub Actions workflow named `tests` that runs `dotnet test` against raw websocket message fixtures.

## License

This project is provided as-is for use with Streamer.bot and Fansly integration.
