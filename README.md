# Fansly Streamer.bot Extension

**Last Updated: 07/03/2025**

A Streamer.bot extension that adds support for Fansly chat events including messages, tips, and subscriptions.

## Features

- **Chat Messages** - Receive and process all Fansly chat messages
- **Tips** - Detect and handle tip events with amount information
- **Subscriptions** - Track new subscriptions with tier and streak information
- **Rich Metadata** - Access user roles (creator, staff, subscriber) and subscription details

## Prerequisites

- [Streamer.bot](https://streamer.bot/) v0.2.0 or higher
- .NET Framework 4.8 (included with Windows)
- A Fansly account with chat access

## Installation

1. **Download the Extension**

   - Clone this repository or download the `onMessage.cs` file

2. **Import into Streamer.bot**

   - Open Streamer.bot
   - Go to `Actions` → `Sub-Actions` → Right-click → `Add Sub-Action` → `C# Code` → `Execute C# Code`
   - Copy the entire contents of `onMessage.cs` into the code editor
   - Click `Compile` to verify there are no errors
   - Click `Save and Compile`

3. **Set up WebSocket Connection**

   - You'll need a separate WebSocket client to connect to Fansly's chat
   - WebSocket URL: `wss://chatws.fansly.com/?v=3`
   - When connected, send a join message:
     ```json
     { "t": 46001, "d": "{\"chatRoomId\":\"YOUR_CHAT_ROOM_ID\"}" }
     ```

4. **Configure Message Forwarding**
   - Set up your WebSocket client to forward received messages to Streamer.bot
   - Pass the raw WebSocket message as the `message` argument to the C# code

## Usage

The extension registers three custom triggers in Streamer.bot:

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

**Additional Variables:**

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

## Example Actions

### Tip Alert

1. Create a new Action
2. Set trigger to `Fansly Tip`
3. Add sub-actions for:
   - Play sound effect
   - Show on-screen alert
   - Send chat message: `Thank you %user% for the %tipAmountFormatted% tip!`

## Troubleshooting

### Debug Logging

The extension includes debug logging. Check the Streamer.bot logs for:

- `[Fansly] onMessage Init` - Extension initialized
- `[Fansly] Processing event type: X` - Event type received
- `[Fansly] Chat - username: message` - Chat message processed
- `[Fansly] Tip - username: $X.XX` - Tip processed
- `[Fansly] Subscription - username: tier` - Subscription processed

## Development

### Message Format

The extension expects WebSocket messages in Fansly's format:

```json
{
  "t": 10000,
  "d": "{\"serviceId\":46,\"event\":\"{...}\"}"
}
```

## License

This project is provided as-is for use with Streamer.bot and Fansly integration.
