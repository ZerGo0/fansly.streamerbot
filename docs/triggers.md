# Triggers

The extension registers custom triggers in Streamer.bot under `Triggers > Custom > Fansly`.

## Common variables

All triggers include these shared variables:

- `platform` - Always `Fansly`
- `eventType` - Normalized event type used by the parser
- `timestamp` - Timestamp assigned to the processed event
- `chatRoomId` - Chat room ID when the event belongs to a room, otherwise an empty string for session-level events
- `rawJson` - Raw websocket payload that produced the trigger

## Fansly Message

Triggered for every chat message, including private messages and tip messages.

Available variables:

- `user` - Username
- `userName` - Username
- `displayName` - Display name
- `userId` - User's ID
- `message` - Message content
- `messageId` - Unique message ID
- `usernameColor` - Hex color of username
- `isCreator` - Whether user is the creator
- `isStaff` - Whether user is staff
- `isFollowing` - Whether user is following
- `isSubscriber` - Whether user is subscribed
- `subscriberTier` - Subscription tier name
- `subscriberTierColor` - Hex color of tier
- `subscriberTierId` - Subscription tier ID
- `isPrivate` - Whether the message was private
- `accountFlags` - Raw Fansly account flags for the sender
- `messageType` - Raw Fansly message type

## Fansly Tip

Triggered when a message contains a tip attachment.

Available variables:

- All `Fansly Message` variables
- `tipAmount` - Raw tip amount in Fansly balance units
- `tipAmountDollars` - Tip amount formatted as dollars

## Fansly Subscription

Triggered for subscription alerts.

Available variables:

- `user` - Username
- `userName` - Username
- `displayName` - Display name
- `userId` - Subscriber ID
- `tier` - Subscription tier name
- `tierName` - Subscription tier name
- `tierColor` - Hex color of tier
- `tierId` - Subscription tier ID
- `streak` - Subscription streak in days
- `totalDays` - Total subscription days
- `months` - Approximate months subscribed
- `usernameColor` - Hex color of username
- `alertId` - Subscription alert ID

## Fansly Session Verified

Triggered when the websocket session verification succeeds.

Available variables:

- `version` - Verified websocket protocol version
- `payloadJson` - Parsed session verification payload as JSON

## Fansly Error

Triggered for websocket error envelopes.

Available variables:

- `code` - Fansly error code
- `message` - Fansly error message
- `payloadJson` - Parsed error payload as JSON

## Fansly Ping Response

Triggered for websocket ping response envelopes.

Available variables:

- `payloadJson` - Parsed ping response payload as JSON

## Fansly Chat Room Updated

Triggered when Fansly sends chat room update event `type = 4`.

Available variables:

- `accountId` - Room owner account ID
- `flags` - Raw chat room flags
- `payloadJson` - Parsed chat room payload as JSON

## Fansly Messages Removed

Triggered when Fansly sends chat room message removal event `type = 20`.

Available variables:

- `moderationId` - Moderation event ID
- `accountId` - Account whose messages were removed
- `moderationType` - Normalized moderation type such as `ban`, `timeout`, or `unban`
- `durationMs` - Duration in milliseconds when present
- `durationSeconds` - Duration in seconds when present
- `payloadJson` - Parsed moderation payload as JSON

## Fansly Moderation

Triggered when Fansly sends chat moderation event `type = 30`.

Available variables:

- `moderationId` - Moderation event ID
- `accountId` - Moderated account ID
- `moderationType` - Normalized moderation type such as `ban`, `timeout`, or `unban`
- `moderationTypeId` - Raw Fansly moderation type ID
- `durationMs` - Duration in milliseconds when present
- `durationSeconds` - Duration in seconds when present
- `payloadJson` - Parsed moderation payload as JSON

## Fansly Goal Created

Triggered when Fansly sends goal creation event `type = 50`.

Available variables:

- `goalId` - Goal ID
- `accountId` - Goal owner account ID
- `label` - Goal label
- `description` - Goal description
- `currentAmount` - Current goal amount in Fansly balance units
- `currentAmountDollars` - Current goal amount formatted as dollars
- `goalAmount` - Goal target amount in Fansly balance units
- `goalAmountDollars` - Goal target amount formatted as dollars
- `status` - Raw goal status
- `version` - Goal version
- `deletedAt` - Deleted timestamp when present
- `isDeleted` - Whether the goal payload marks the goal as deleted
- `payloadJson` - Parsed goal payload as JSON

## Fansly Goal Updated

Triggered when Fansly sends goal update event `type = 51`.

Available variables:

- All `Fansly Goal Created` variables

## Fansly Settings Updated

Triggered when Fansly sends settings update event `type = 54`.

Available variables:

- `slowmodeMs` - Slow mode duration in milliseconds
- `slowmodeSeconds` - Slow mode duration in seconds
- `isSlowmodeEnabled` - Whether slow mode is currently enabled
- `payloadJson` - Parsed settings payload as JSON
