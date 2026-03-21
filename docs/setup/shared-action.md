# Shared Send Action

This repository stores the C# code that goes inside Streamer.bot `Execute C# Code` sub-actions.

That means [sendMessage.cs](../../sendMessage.cs) is not a standalone Streamer.bot action by itself.
It is the code payload for one shared action that you create once in Streamer.bot and then reuse from other actions.

Create one shared action called `Fansly Send Message`:

1. Open the `Actions` tab.
2. Right-click in the actions list.
3. Click `Add`.
4. Set the action name to `Fansly Send Message`.
5. Save the action.
6. Select the `Fansly Send Message` action.
7. Right-click in the `Sub-Actions` pane.
8. Add `Core -> C# -> Execute C# Code`.
9. Set the sub-action `Name` to `FanslySendMessage`.
10. Enable `Precompile on Application Start`.
11. Paste the contents of [sendMessage.cs](../../sendMessage.cs).
12. Click `Find Refs`.
13. Click `Compile`.
14. If you still get errors ending with `are you missing an assembly reference?`, open the `References` pane, right-click it, and add the missing reference manually.
15. For [sendMessage.cs](../../sendMessage.cs), the important reference is `System.Net.Http`.
16. Click `Save and Compile`.

## Action Arguments

The shared action reads these arguments:

- `message`
- `rawInput`
- `chatRoomId`
- `authToken`

Behavior:

- If `message` exists, it is used.
- If `message` does not exist, `rawInput` is used.
- If `chatRoomId` does not exist, the action falls back to `fanslyChatroomId`.
- If `authToken` does not exist, the action falls back to `fanslyAuthToken`.
- `fanslyAuthToken` should contain the claimed Management Session token, not your normal Fansly login session.

## Reuse From Other Flows

Do not paste the C# code into every action.

Instead, call the shared `Fansly Send Message` action from other actions.

Example pattern:

1. Select the action you want to edit.
2. Right-click in the `Sub-Actions` pane.
3. Add `Core -> Arguments -> Set Argument`
4. Set `message` to the text you want to send
5. Disable `Auto Type`
6. Right-click in the `Sub-Actions` pane again.
7. Add `Core -> Actions -> Run Action`
8. Select `Fansly Send Message`

## Example Flows

### Send a fixed message

1. `Set Argument`
2. Name: `message`
3. Value: `Hello from Streamer.bot`
4. `Auto Type`: unchecked
5. `Run Action`
6. Action: `Fansly Send Message`

### Send from a command

If your command puts the user text into `rawInput`, you can directly run `Fansly Send Message`.

The shared action will use `rawInput` when `message` is not set.

### Override the target chat room

1. `Set Argument`
2. Name: `chatRoomId`
3. Value: another Fansly chat room id
4. `Auto Type`: unchecked
5. `Set Argument`
6. Name: `message`
7. Value: the message text
8. `Auto Type`: unchecked
9. `Run Action`
10. Action: `Fansly Send Message`

## Troubleshooting

The extension includes debug logging. Check the Streamer.bot logs for:

- `[Fansly] onOpen Init`
- `[Fansly] onMessage`
- `[Fansly] Processing event type: X`
- `[Fansly] Chat - username: message`
- `[Fansly] Tip - username: $X.XX`
- `[Fansly] Subscription - username: tier`
- `[Fansly] Sent message to chatroom X: message`
