# Notes

## Fansly chat websocket message examples

Websocket Url: `wss://chatws.fansly.com/?v=3`

### Normal chat message (Server > Client)

```json
{
  "t": 10000,
  "d": "{\"serviceId\":46,\"event\":\"{\\\"type\\\":10,\\\"chatRoomMessage\\\":{\\\"chatRoomId\\\":\\\"408830844350771200\\\",\\\"senderId\\\":\\\"407996034761891840\\\",\\\"content\\\":\\\"test\\\",\\\"type\\\":0,\\\"private\\\":0,\\\"attachments\\\":[],\\\"accountFlags\\\":1,\\\"metadata\\\":\\\"{\\\\\\\"senderIsCreator\\\\\\\":true,\\\\\\\"senderIsStaff\\\\\\\":false}\\\",\\\"chatRoomAccountId\\\":\\\"407996034761891840\\\",\\\"id\\\":\\\"797163507798781952\\\",\\\"createdAt\\\":1751552950379,\\\"embeds\\\":[],\\\"usernameColor\\\":\\\"#00ff19\\\",\\\"username\\\":\\\"ZerGo0_Bot\\\",\\\"displayname\\\":\\\"ZerGo0_Bot\\\"}}\"}"
}
```

### Tip message (Server > Client)

```json
{
  "t": 10000,
  "d": "{\"serviceId\":46,\"event\":\"{\\\"type\\\":10,\\\"chatRoomMessage\\\":{\\\"chatRoomId\\\":\\\"408830844350771200\\\",\\\"senderId\\\":\\\"281038385793998848\\\",\\\"content\\\":\\\"tip test\\\",\\\"type\\\":0,\\\"private\\\":0,\\\"attachments\\\":[{\\\"contentType\\\":7,\\\"contentId\\\":\\\"797163797902008322\\\",\\\"metadata\\\":\\\"{\\\\\\\"amount\\\\\\\":100}\\\",\\\"chatRoomMessageId\\\":\\\"797163798283694080\\\"}],\\\"accountFlags\\\":6,\\\"messageTip\\\":null,\\\"metadata\\\":\\\"{\\\\\\\"senderIsCreator\\\\\\\":false,\\\\\\\"senderIsStaff\\\\\\\":false,\\\\\\\"senderIsFollowing\\\\\\\":true,\\\\\\\"senderSubscription\\\\\\\":{\\\\\\\"tierId\\\\\\\":\\\\\\\"795201999690801152\\\\\\\",\\\\\\\"tierColor\\\\\\\":\\\\\\\"#F73838\\\\\\\",\\\\\\\"tierName\\\\\\\":\\\\\\\"Plus\\\\\\\"}}\\\",\\\"chatRoomAccountId\\\":\\\"407996034761891840\\\",\\\"id\\\":\\\"797163798283694080\\\",\\\"createdAt\\\":1751553019637,\\\"embeds\\\":[],\\\"usernameColor\\\":\\\"#0066ff\\\",\\\"username\\\":\\\"ZerGo0\\\",\\\"displayname\\\":\\\"ZerGo0\\\"}}\"}"
}
```

### User subscribed (Server > Client)

```json
{
  "t": 10000,
  "d": "{\"serviceId\":46,\"event\":\"{\\\"type\\\":53,\\\"subAlert\\\":{\\\"chatRoomId\\\":\\\"408830844350771200\\\",\\\"senderId\\\":\\\"281038385793998848\\\",\\\"historyId\\\":\\\"797165341313605638\\\",\\\"subscriberId\\\":\\\"281038385793998848\\\",\\\"subscriptionTierId\\\":\\\"795201999690801152\\\",\\\"subscriptionTierName\\\":\\\"Plus\\\",\\\"subscriptionTierColor\\\":\\\"#F73838\\\",\\\"subscriptionStreak\\\":1,\\\"subscriptionTotalDays\\\":30,\\\"id\\\":\\\"797165385945198592\\\",\\\"usernameColor\\\":\\\"#0066ff\\\",\\\"username\\\":\\\"ZerGo0\\\",\\\"displayname\\\":\\\"ZerGo0\\\"}}\"}"
}
```

### Join chat message (Client > Server)

```json
{ "t": 46001, "d": "{\"chatRoomId\":\"408830844350771200\"}" }
```
