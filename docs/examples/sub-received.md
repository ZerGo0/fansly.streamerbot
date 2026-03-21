# Sub Received

This example listens for the `Fansly Subscription` custom trigger and sends a friendly subscription message back into chat.

This example action is included in the release bundle.
If you import it from the bundle, make sure the action is enabled in Streamer.bot.

## Goal

When a subscription is received, send a message like:

`Thanks ZerGo0 for subscribing at the Plus tier!`

## Trigger

Create a new action:

1. Open the `Actions` tab.
2. Right-click in the actions list.
3. Click `Add`.
4. Set the action name to `Fansly Sub Reply`.
5. Save the action.

Add this trigger to the action:

1. Select the `Fansly Sub Reply` action.
2. Right-click in the `Triggers` pane.
3. Add `Custom -> Fansly -> Fansly Subscription`.

## Action

Add these sub-actions:

1. Right-click in the `Sub-Actions` pane.
2. Add `Core -> Arguments -> Set Argument`.
3. Set:
   - Name: `message`
   - Value: `Thanks %userName% for subscribing at the %tierName% tier!`
   - `Auto Type`: unchecked
4. Right-click in the `Sub-Actions` pane again.
5. Add `Core -> Actions -> Run Action`.
6. Set the action to `Fansly Send Message`.

## Available Variables Used

- `%userName%`
- `%tierName%`

## Optional More Detailed Version

If you want a more detailed message, use:

`Thanks %userName% for the %tierName% sub! Streak: %streak% days, total: %totalDays% days.`

## Notes

- `%months%` is also available if you prefer a rough month count over total days.
