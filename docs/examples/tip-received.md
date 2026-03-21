# Tip Received

This example listens for the `Fansly Tip` custom trigger and sends a friendly thank-you message back into chat.

This example action is included in the release bundle.
If you import it from the bundle, make sure the action is enabled in Streamer.bot.

## Goal

When a tip is received, send a message like:

`Thanks ZerGo0 for the $5.00 tip!`

## Trigger

Create a new action:

1. Open the `Actions` tab.
2. Right-click in the actions list.
3. Click `Add`.
4. Set the action name to `Fansly Tip Reply`.
5. Save the action.

Add this trigger to the action:

1. Select the `Fansly Tip Reply` action.
2. Right-click in the `Triggers` pane.
3. Add `Custom -> Fansly -> Fansly Tip`.

## Action

Add these sub-actions:

1. Right-click in the `Sub-Actions` pane.
2. Add `Core -> Arguments -> Set Argument`.
3. Set:
   - Name: `message`
   - Value: `Thanks %userName% for the %tipAmountDollars% tip!`
   - `Auto Type`: unchecked
4. Right-click in the `Sub-Actions` pane again.
5. Add `Core -> Actions -> Run Action`.
6. Set the action to `Fansly Send Message`.

## Available Variables Used

- `%userName%`
- `%tipAmountDollars%`

## Notes

- `Fansly Tip` is fired in addition to the normal `Fansly Message` event when a tip attachment is present.
- If you want a different tone, only change the `message` argument value.

Example alternatives:

- `Huge thanks to %userName% for tipping %tipAmountDollars%!`
- `%userName% just tipped %tipAmountDollars% - thank you!`
