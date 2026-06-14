# Chatroom Setup

1. Head over to your [Fansly chatroom](https://fansly.com/creator/streaming).
2. Click the 3 dots next to the chat input.
3. Click `Copy Chat Url`.
4. Copy the id at the end of the url.

Example:

- `https://fansly.com/chatroom/408830844350771200`
- Chatroom id: `408830844350771200`

Create a persisted global in Streamer.bot:

1. Open the `Variables` tab.
2. Go to `Persisted Globals`.
3. Right-click and choose `Add Variable`.
4. Set:
   - Name: `fanslyChatroomId` (the name is case sensitive)
   - Value: your chatroom id
   - `Auto Type`: unchecked
5. Save the variable.

Reconnect the configured Fansly websocket client in Streamer.bot:

1. Open the `Servers/Clients` tab.
2. Go to `Custom WebSocket Clients`.
3. Find the Fansly websocket client.
4. Right-click it.
5. Click `Disconnect`.
6. Right-click it again.
7. Click `Connect`.

That joins your chatroom for inbound events.
