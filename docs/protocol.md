# Protocol Notes

## Fansly Chat Websocket

Websocket URL:

- `wss://chatws.fansly.com/?v=3`

### Envelope types handled by the extension

- `0` - Error event
- `1` - Session verified event
- `2` - Ping response event
- `10000` - Service event
- `10001` - Bulk wrapper containing more websocket envelopes

### ChatRoom service event types handled by the extension

- `4` - Chat room updated
- `10` - Chat room message
- `20` - Messages removed
- `30` - Moderation event
- `50` - Goal created
- `51` - Goal updated
- `53` - Subscription alert
- `54` - Settings updated

### Session verify request (client to server)

Used when `fanslyAuthToken` is available. This is required for private inbound messages.

```json
{ "t": 1, "d": "{\"token\":\"<fansly token>\",\"v\":3}" }
```

### Session verified event (server to client)

```json
{
  "t": 1,
  "d": "{\"v\":3}"
}
```

### Ping response event (server to client)

```json
{
  "t": 2,
  "d": "{\"ts\":1775199202949}"
}
```

### Normal chat message (server to client)

```json
{
  "t": 10000,
  "d": "{\"serviceId\":46,\"event\":\"{\\\"type\\\":10,\\\"chatRoomMessage\\\":{\\\"chatRoomId\\\":\\\"408830844350771200\\\",\\\"senderId\\\":\\\"407996034761891840\\\",\\\"content\\\":\\\"test\\\",\\\"type\\\":0,\\\"private\\\":0,\\\"attachments\\\":[],\\\"accountFlags\\\":1,\\\"metadata\\\":\\\"{\\\\\\\"senderIsCreator\\\\\\\":true,\\\\\\\"senderIsStaff\\\\\\\":false}\\\",\\\"chatRoomAccountId\\\":\\\"407996034761891840\\\",\\\"id\\\":\\\"797163507798781952\\\",\\\"createdAt\\\":1751552950379,\\\"embeds\\\":[],\\\"usernameColor\\\":\\\"#00ff19\\\",\\\"username\\\":\\\"ZerGo0_Bot\\\",\\\"displayname\\\":\\\"ZerGo0_Bot\\\"}}\"}"
}
```

### Private normal chat message (server to client)

```json
{
  "t": 10000,
  "d": "{\"serviceId\":46,\"event\":\"{\\\"type\\\":10,\\\"chatRoomMessage\\\":{\\\"chatRoomId\\\":\\\"408830844350771200\\\",\\\"senderId\\\":\\\"281038385793998848\\\",\\\"content\\\":\\\"private test 123\\\",\\\"type\\\":0,\\\"private\\\":1,\\\"attachments\\\":[],\\\"accountFlags\\\":6,\\\"metadata\\\":\\\"{\\\\\\\"senderIsCreator\\\\\\\":false,\\\\\\\"senderIsStaff\\\\\\\":false,\\\\\\\"senderIsFollowing\\\\\\\":true,\\\\\\\"senderSubscription\\\\\\\":{\\\\\\\"tierId\\\\\\\":\\\\\\\"795201999690801152\\\\\\\",\\\\\\\"tierColor\\\\\\\":\\\\\\\"#F73838\\\\\\\",\\\\\\\"tierName\\\\\\\":\\\\\\\"Plus\\\\\\\"}}\\\",\\\"chatRoomAccountId\\\":\\\"407996034761891840\\\",\\\"id\\\":\\\"896343079546531840\\\",\\\"createdAt\\\":1775199202949,\\\"embeds\\\":[],\\\"usernameColor\\\":\\\"#0066ff\\\",\\\"username\\\":\\\"ZerGo0\\\",\\\"displayname\\\":\\\"ZerGo0\\\"}}\"}"
}
```

### Tip message (server to client)

```json
{
  "t": 10000,
  "d": "{\"serviceId\":46,\"event\":\"{\\\"type\\\":10,\\\"chatRoomMessage\\\":{\\\"chatRoomId\\\":\\\"408830844350771200\\\",\\\"senderId\\\":\\\"281038385793998848\\\",\\\"content\\\":\\\"tip test\\\",\\\"type\\\":0,\\\"private\\\":0,\\\"attachments\\\":[{\\\"contentType\\\":7,\\\"contentId\\\":\\\"797163797902008322\\\",\\\"metadata\\\":\\\"{\\\\\\\"amount\\\\\\\":100}\\\",\\\"chatRoomMessageId\\\":\\\"797163798283694080\\\"}],\\\"accountFlags\\\":6,\\\"messageTip\\\":null,\\\"metadata\\\":\\\"{\\\\\\\"senderIsCreator\\\\\\\":false,\\\\\\\"senderIsStaff\\\\\\\":false,\\\\\\\"senderIsFollowing\\\\\\\":true,\\\\\\\"senderSubscription\\\\\\\":{\\\\\\\"tierId\\\\\\\":\\\\\\\"795201999690801152\\\\\\\",\\\\\\\"tierColor\\\\\\\":\\\\\\\"#F73838\\\\\\\",\\\\\\\"tierName\\\\\\\":\\\\\\\"Plus\\\\\\\"}}\\\",\\\"chatRoomAccountId\\\":\\\"407996034761891840\\\",\\\"id\\\":\\\"797163798283694080\\\",\\\"createdAt\\\":1751553019637,\\\"embeds\\\":[],\\\"usernameColor\\\":\\\"#0066ff\\\",\\\"username\\\":\\\"ZerGo0\\\",\\\"displayname\\\":\\\"ZerGo0\\\"}}\"}"
}
```

### Subscription message (server to client)

```json
{
  "t": 10000,
  "d": "{\"serviceId\":46,\"event\":\"{\\\"type\\\":53,\\\"subAlert\\\":{\\\"chatRoomId\\\":\\\"408830844350771200\\\",\\\"senderId\\\":\\\"281038385793998848\\\",\\\"historyId\\\":\\\"797165341313605638\\\",\\\"subscriberId\\\":\\\"281038385793998848\\\",\\\"subscriptionTierId\\\":\\\"795201999690801152\\\",\\\"subscriptionTierName\\\":\\\"Plus\\\",\\\"subscriptionTierColor\\\":\\\"#F73838\\\",\\\"subscriptionStreak\\\":1,\\\"subscriptionTotalDays\\\":30,\\\"id\\\":\\\"797165385945198592\\\",\\\"usernameColor\\\":\\\"#0066ff\\\",\\\"username\\\":\\\"ZerGo0\\\",\\\"displayname\\\":\\\"ZerGo0\\\"}}\"}"
}
```

### Chat room updated event (server to client)

```json
{
  "t": 10000,
  "d": "{\"serviceId\":46,\"event\":\"{\\\"type\\\":4,\\\"chatRoom\\\":{\\\"id\\\":\\\"room-1\\\",\\\"accountId\\\":\\\"owner-1\\\",\\\"flags\\\":8}}\"}"
}
```

### Messages removed event (server to client)

```json
{
  "t": 10000,
  "d": "{\"serviceId\":46,\"event\":\"{\\\"type\\\":20,\\\"chatRoomBan\\\":{\\\"id\\\":\\\"ban-1\\\",\\\"chatRoomId\\\":\\\"room-1\\\",\\\"accountId\\\":\\\"user-1\\\",\\\"type\\\":1,\\\"duration\\\":0}}\"}"
}
```

### Moderation event (server to client)

```json
{
  "t": 10000,
  "d": "{\"serviceId\":46,\"event\":\"{\\\"type\\\":30,\\\"chatRoomBan\\\":{\\\"id\\\":\\\"ban-2\\\",\\\"chatRoomId\\\":\\\"room-1\\\",\\\"accountId\\\":\\\"user-2\\\",\\\"type\\\":2,\\\"duration\\\":600000}}\"}"
}
```

### Goal created event (server to client)

```json
{
  "t": 10000,
  "d": "{\"serviceId\":46,\"event\":\"{\\\"type\\\":50,\\\"chatRoomGoal\\\":{\\\"id\\\":\\\"goal-1\\\",\\\"chatRoomId\\\":\\\"room-1\\\",\\\"accountId\\\":\\\"owner-1\\\",\\\"status\\\":1,\\\"currentAmount\\\":1500,\\\"goalAmount\\\":5000,\\\"label\\\":\\\"New mic\\\",\\\"description\\\":\\\"Upgrade stream audio\\\",\\\"version\\\":1}}\"}"
}
```

### Goal updated event (server to client)

```json
{
  "t": 10000,
  "d": "{\"serviceId\":46,\"event\":\"{\\\"type\\\":51,\\\"chatRoomGoal\\\":{\\\"id\\\":\\\"goal-1\\\",\\\"chatRoomId\\\":\\\"room-1\\\",\\\"accountId\\\":\\\"owner-1\\\",\\\"status\\\":2,\\\"currentAmount\\\":5000,\\\"goalAmount\\\":5000,\\\"label\\\":\\\"New mic\\\",\\\"description\\\":\\\"Upgrade stream audio\\\",\\\"version\\\":2,\\\"deletedAt\\\":1775199202949}}\"}"
}
```

### Settings updated event (server to client)

```json
{
  "t": 10000,
  "d": "{\"serviceId\":46,\"event\":\"{\\\"type\\\":54,\\\"settings\\\":{\\\"chatRoomId\\\":\\\"room-1\\\",\\\"slowmode\\\":15000}}\"}"
}
```

### Bulk wrapper (server to client)

The extension also handles `t = 10001`, where `d` is a JSON string containing an array of complete websocket envelopes.

```json
{
  "t": 10001,
  "d": "[{\"t\":10000,\"d\":\"...\"},{\"t\":10000,\"d\":\"...\"}]"
}
```

### Join chat message (client to server)

```json
{ "t": 46001, "d": "{\"chatRoomId\":\"408830844350771200\"}" }
```

### Ping request (client to server)

The websocket client sends plain text `p` every 10 seconds to keep the connection alive.

## Fansly Chat Send Request

Endpoint:

- `POST https://apiv3.fansly.com/api/v1/chatroom/message?ngsw-bypass=true`

Headers:

- `accept: application/json, text/plain, */*`
- `accept-language: en-US,en;q=0.9`
- `authorization: <claimed Fansly Management Session token>`
- `origin: https://fansly.com`
- `referer: https://fansly.com/`
- `user-agent: Mozilla/5.0 (Macintosh; Intel Mac OS X 10_15_7) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/146.0.0.0 Safari/537.36`
- `fansly-client-id: 891628335845621760`

Body:

```json
{
  "chatRoomId": "408830844350771200",
  "content": "hello from streamer.bot"
}
```
