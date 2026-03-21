# Protocol Notes

## Fansly Chat Websocket

Websocket URL:

- `wss://chatws.fansly.com/?v=3`

### Normal chat message (server to client)

```json
{
  "t": 10000,
  "d": "{\"serviceId\":46,\"event\":\"{\\\"type\\\":10,\\\"chatRoomMessage\\\":{\\\"chatRoomId\\\":\\\"408830844350771200\\\",\\\"senderId\\\":\\\"407996034761891840\\\",\\\"content\\\":\\\"test\\\",\\\"type\\\":0,\\\"private\\\":0,\\\"attachments\\\":[],\\\"accountFlags\\\":1,\\\"metadata\\\":\\\"{\\\\\\\"senderIsCreator\\\\\\\":true,\\\\\\\"senderIsStaff\\\\\\\":false}\\\",\\\"chatRoomAccountId\\\":\\\"407996034761891840\\\",\\\"id\\\":\\\"797163507798781952\\\",\\\"createdAt\\\":1751552950379,\\\"embeds\\\":[],\\\"usernameColor\\\":\\\"#00ff19\\\",\\\"username\\\":\\\"ZerGo0_Bot\\\",\\\"displayname\\\":\\\"ZerGo0_Bot\\\"}}\"}"
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

### Join chat message (client to server)

```json
{ "t": 46001, "d": "{\"chatRoomId\":\"408830844350771200\"}" }
```

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
