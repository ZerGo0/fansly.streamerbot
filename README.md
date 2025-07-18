# Fansly Streamer.bot Extension

A Streamer.bot extension that adds support for Fansly chat events including messages, tips, and subscriptions.

## Features

- **Chat Messages** - Fansly chat messages
- **Tips** - Fansly tips
- **Subscriptions** - Fansly subscriptions

## Installation

1. **Download the Extension**

   - Download the latest release from the [Releases](https://github.com/ZerGo0/fansly.streamerbot/releases/latest) page

2. **Import into Streamer.bot**

   - Open Streamer.bot
   - Open `Import`
   - Drag and drop the file on the `Import String` field
   - Click `Import`

3. **Set up your chatroom**

   - Head over to your [Fansly chatroom](https://fansly.com/creator/streaming)
   - Click the 3 dots next to the chat input > `Copy Chat Url` > copy the id at the end of the url (`https://fansly.com/chatroom/408830844350771200` > `408830844350771200`)
   - Go to `Variables` > `Persisted Globals` > Right-click > `Add Variable`:
     - Name: `fanslyChatroomId`
     - Value: `408830844350771200` (replace with your chatroom id)
     - Click `Save`
     - `Auto Type` should be unchecked
   - Go to `Servers/Clients` > `Websocket Client` > Right-click the fansly client > `Disconnect` > And `Connect` again
     - this joins your chatroom

4. **Double check the setup**

   - Check all new actions, double click the `Execute Code` actions and hit `Compile` to verify there are no errors
     - Please join the [Discord](https://discord.gg/SBWkgGcmfr) if you have any issues

## Usage

The extension registers three custom triggers in Streamer.bot (Triggers > Custom > Fansly):

### Fansly Message

Triggered for every chat message.

**Available Variables:**

- `user` - Username
- `userName` - Username
- `displayName` - Display name
- `userId` - User's ID
- `message` - Message content
- `messageId` - Unique message ID
- `chatRoomId` - Chat room ID
- `usernameColor` - Hex color of username
- `isCreator` - Whether user is the creator
- `isStaff` - Whether user is staff
- `isFollowing` - Whether user is following
- `isSubscriber` - Whether user is subscribed
- `subscriberTier` - Subscription tier name
- `subscriberTierColor` - Hex color of tier
- `subscriberTierId` - Subscription tier ID
- `platform` - Platform name
- `timestamp` - Timestamp of message

### Fansly Tip

Triggered when a message contains a tip.

**Available Variables:**

- `user` - Username
- `userName` - Username
- `displayName` - Display name
- `userId` - User's ID
- `message` - Message content
- `messageId` - Unique message ID
- `chatRoomId` - Chat room ID
- `usernameColor` - Hex color of username
- `isCreator` - Whether user is the creator
- `isStaff` - Whether user is staff
- `isFollowing` - Whether user is following
- `isSubscriber` - Whether user is subscribed
- `subscriberTier` - Subscription tier name
- `subscriberTierColor` - Hex color of tier
- `subscriberTierId` - Subscription tier ID
- `platform` - Platform name
- `timestamp` - Timestamp of message
- `tipAmount` - Tip amount in dollars
- `tipAmountFormatted` - Formatted tip amount (e.g., "$5.00")

### Fansly Subscription

Triggered for new subscriptions.

**Available Variables:**

- `user` - Username
- `userName` - Username
- `displayName` - Display name
- `userId` - User's ID
- `tier` - Subscription tier name
- `tierName` - Subscription tier name
- `tierColor` - Hex color of tier
- `tierId` - Subscription tier ID
- `streak` - Subscription streak in days
- `totalDays` - Total subscription days
- `months` - Approximate months subscribed
- `usernameColor` - Hex color of username
- `chatRoomId` - Chat room ID
- `alertId` - Alert ID
- `platform` - Platform name
- `timestamp` - Timestamp of subscription

## Troubleshooting

### Debug Logging

The extension includes debug logging. Check the Streamer.bot logs for:

- `[Fansly] onMessage Init` - Extension initialized
- `[Fansly] Processing event type: X` - Event type received
- `[Fansly] Chat - username: message` - Chat message processed
- `[Fansly] Tip - username: $X.XX` - Tip processed
- `[Fansly] Subscription - username: tier` - Subscription processed

## License

This project is provided as-is for use with Streamer.bot and Fansly integration.
