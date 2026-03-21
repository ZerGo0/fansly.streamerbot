# Triggers

The extension registers three custom triggers in Streamer.bot under `Triggers > Custom > Fansly`.

## Fansly Message

Triggered for every chat message.

Available variables:

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

## Fansly Tip

Triggered when a message contains a tip.

Available variables:

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
- `tipAmount` - Raw tip amount
- `tipAmountDollars` - Formatted tip amount

## Fansly Subscription

Triggered for new subscriptions.

Available variables:

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
