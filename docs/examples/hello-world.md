# Hello World

This example sends a simple test message to your Fansly chat.

This example action is included in the release bundle.
If you import it from the bundle, make sure the action is enabled in Streamer.bot.

## Goal

Create one action that sends:

`Hello from Streamer.bot`

## Action

Create a new action:

1. Open the `Actions` tab.
2. Right-click in the actions list.
3. Click `Add`.
4. Set the action name to `Fansly Hello World`.
5. Save the action.

Add this trigger to the action:

1. Select the `Fansly Hello World` action.
2. Right-click in the `Triggers` pane.
3. Add `Core -> Test`.

Add these sub-actions:

1. Right-click in the `Sub-Actions` pane.
2. Add `Core -> Arguments -> Set Argument`.
3. Set:
   - Name: `message`
   - Value: `Hello from Streamer.bot`
   - `Auto Type`: unchecked
4. Right-click in the `Sub-Actions` pane again.
5. Add `Core -> Actions -> Run Action`.
6. Set the action to `Fansly Send Message`.

## Test

Run the `Test Trigger` for the `Fansly Hello World` action from Streamer.bot.

If everything is configured correctly, the message should appear in your Fansly chat.
