# Troubleshooting

## No Fansly events are showing up in Streamer.bot

If chat messages, tips, or subscription events are not showing up:

1. Make sure you imported the latest release bundle.
2. Make sure the imported Fansly actions are enabled.
3. Make sure `fanslyChatroomId` exists in `Variables -> Persisted Globals`.
4. Make sure `fanslyChatroomId` contains the correct chatroom id.
5. Make sure `Auto Type` is unchecked for `fanslyChatroomId`.
6. Open `Servers/Clients -> Custom WebSocket Clients`.
7. Right-click the Fansly websocket client.
8. Click `Disconnect`.
9. Right-click it again.
10. Click `Connect`.

Check the Streamer.bot log for:

- `[Fansly] onOpen Init`
- `[Fansly] onMessage`
- `[Fansly] Processing event type: X`

If you do not see `[Fansly] onOpen Init`, the import may be incomplete or the relevant action may be disabled.

## I copied the Management Session URL, but I cannot find the token

If you do not see the token in Chrome DevTools:

1. Make sure you opened the Management Session URL in an incognito window.
2. Make sure you opened the actual `https://fansly.com/managementsession/claim/...` URL.
3. Wait for the Fansly page to fully load after opening that URL.
4. Open `Application -> Local Storage -> https://fansly.com`.
5. Check whether `session_active_session` exists.
6. Click `session_active_session` once and look in the lower panel for the `token` row.
7. If it still is not there, refresh the page once and check again.

If you open the claim URL in your normal logged-in browser window instead of incognito, you are much more likely to confuse your normal session with the Management Session.

## Sending messages does not work

If `Fansly Send Message` runs but nothing is sent:

1. Make sure the bundled `Fansly Send Message` action is enabled.
2. Make sure `fanslyAuthToken` exists in `Variables -> Persisted Globals`.
3. Make sure `fanslyAuthToken` contains the claimed Management Session token, not your normal Fansly login session.
4. Make sure `fanslyChatroomId` is set correctly.
5. Make sure `Auto Type` is unchecked for both globals.

Check the Streamer.bot log for:

- `[Fansly] sendMessage Init`
- `[Fansly] Failed to send message: ...`
- `[Fansly] Sent message to chatroom X: message`

Common causes:

- Wrong `fanslyChatroomId`
- Wrong token
- Token copied from the wrong browser window
- Bundled send action is disabled

## I get a 403 or another HTTP error when sending

If the send action logs a `403`, `401`, or another HTTP error:

1. Re-do the Management Session setup from the beginning.
2. Create a fresh Management Session on Fansly.
3. Claim it in a new incognito window.
4. Copy the token again from `session_active_session`.
5. Replace the old `fanslyAuthToken` value in Streamer.bot.

If that still fails, verify that:

1. You copied the `token` row value, not another field.
2. You are sending to the correct chatroom id.
3. The imported `Fansly Send Message` action is enabled.

## I get compile errors in Streamer.bot

If an imported Fansly action shows compile errors:

1. Open the action in Streamer.bot.
2. Double-click the `Execute C# Code` sub-action.
3. Click `Find Refs`.
4. Click `Compile`.

If the error mentions `System.Net.Http`, `HttpClient`, `HttpRequestMessage`, or `StringContent`:

1. In the same C# editor window, open the `References` pane.
2. Right-click in `References`.
3. Add `System.Net.Http`.
4. Click `Compile` again.
5. Save the action.

This is the most common compile issue for the bundled `Fansly Send Message` action.

## My example action does nothing

If an example action does not run:

1. Make sure the example action itself is enabled.
2. Make sure the bundled `Fansly Send Message` action is also enabled.
3. If the example uses a Fansly trigger, confirm the matching inbound event is working first.
4. For `Hello World`, use the action's `Test Trigger`.

The examples in the release bundle call `Fansly Send Message`.
If that action is disabled, the examples will appear to do nothing.

## My token is visible in Streamer.bot

If you can still see the token in plain text:

1. Open `Settings -> Variables`.
2. Enable `Mask Variables Containing`.
3. Add `fanslyAuthToken`.
4. Save the settings.
5. Close the Variable Viewer if it is open.
6. Open the Variable Viewer again.

Masking helps reduce accidental leaks while streaming, recording, or screensharing.
