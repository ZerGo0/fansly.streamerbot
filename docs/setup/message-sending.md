# Message Sending Setup

The release bundle already includes the `Fansly Send Message` action.
You do not need to create that action manually from `sendMessage.cs`.
After import, make sure the bundled action is enabled in Streamer.bot.

> [!CAUTION]
> Do not use your normal Fansly session token for Streamer.bot.
> A normal session has full access to your account.
> If you import a malicious Streamer.bot action or C# snippet, it could use that full-access session to do things on your Fansly account.
> A Management Session is the safer option because it is a separate session you create specifically for this purpose.

## Create a Fansly Management Session

Do this in your normal Chrome window, where you are already logged in to Fansly:

1. Open `https://fansly.com/creator/management`.
2. Create a new Management Session.
3. Give it a name you will recognize later, for example `Streamer.bot`.
4. Leave all permissions turned off.
5. Save it.
6. Copy the Management Session URL.

The URL should look like this:

- `https://fansly.com/managementsession/claim/...`

That URL is not the token yet.
It is only the claim link that Fansly uses to create the actual session.

## Claim the Management Session in Chrome Incognito

Do this in a new incognito window so you keep it separate from your main Fansly login:

1. In Chrome, open an incognito window.
2. Shortcut:
   - Windows: `Ctrl+Shift+N`
   - Linux: `Ctrl+Shift+N`
   - Mac: `Cmd+Shift+N`
3. Make sure this is a real incognito window, not your normal Chrome window.
4. Paste the Management Session URL into the address bar.
5. Press `Enter`.
6. Wait for the Fansly page to fully load.

When you open that claim URL, Fansly's frontend automatically claims the Management Session for you.
You do not need to manually call any API.

## Copy the Claimed Session Token in Chrome

Stay in the same incognito tab after the page has loaded:

1. Open Chrome DevTools.
2. Shortcut:
   - Windows: `Ctrl+Shift+I`
   - Linux: `Ctrl+Shift+I`
   - Mac: `Cmd+Option+I`
3. Click the `Application` tab at the top of DevTools.
4. If you do not see `Application`, click the `>>` button and select `Application`.
5. In the left sidebar, find the `Storage` section.
6. Under `Storage`, click `Local Storage`.
7. Click `https://fansly.com`.
8. In the table on the right, find the row with the key `session_active_session`.
9. Click the `session_active_session` row once.
10. Chrome will show the parsed value in the lower panel.
11. In the lower panel, find the `token` row.
12. Right-click the `token` row.
13. Click `Copy value`.
14. The copied value is the claimed Management Session token.

If you cannot find the token:

1. Make sure you are still on a `fansly.com` page in that same incognito window.
2. Make sure the page finished loading after you opened the claim URL.
3. Make sure you selected `https://fansly.com` under `Local Storage`.
4. Check that the `session_active_session` row exists.
5. If it still does not appear, refresh the page once and check again.

## Save the Token in Streamer.bot

The copied value is the claimed Management Session token that Streamer.bot should use for message sending.

Create another persisted global in Streamer.bot:

1. Open the `Variables` tab.
2. Go to `Persisted Globals`.
3. Right-click and choose `Add Variable`.
4. Set:
   - Name: `fanslyAuthToken`
   - Value: your claimed Fansly Management Session token
   - `Auto Type`: unchecked
5. Save the variable.

## Mask the Token in Streamer.bot

Streamer.bot can mask sensitive variables in the Variable Viewer so they do not show up in plain text.

> [!WARNING]
> You should do this so the token is less likely to leak if you open Streamer.bot while streaming, recording, screensharing, or troubleshooting with someone else watching.

Set that up now:

1. Open `Settings` in Streamer.bot.
2. Open the `Variables` tab.
3. Enable `Mask Variables Containing`.
4. Add `fanslyAuthToken` to the keyword list.
5. Save the settings.
6. If the Variable Viewer was already open, close it and open it again.

After that, variables whose names contain `fanslyAuthToken` will be masked in the Variable Viewer.

After that, you can close the incognito window if you want.
