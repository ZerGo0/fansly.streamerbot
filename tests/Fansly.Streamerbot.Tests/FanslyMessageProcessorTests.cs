using System;
using System.Collections.Generic;
using Xunit;

public class FanslyMessageProcessorTests
{
    private static readonly DateTimeOffset FixedNow = new DateTimeOffset(2026, 4, 3, 12, 0, 0, TimeSpan.Zero);

    [Fact]
    public void Processes_Normal_Chat_Message()
    {
        List<FanslyProcessedEvent> results = FanslyMessageProcessor.ProcessRawMessage(NormalChatRaw, FixedNow);
        Assert.Single(results);

        FanslyChatMessageResult chat = Assert.IsType<FanslyChatMessageResult>(results[0]);
        Assert.Equal("test", chat.Message.Content);
        Assert.True(chat.Metadata.SenderIsCreator);

        List<FanslyTriggerDispatch> dispatches = FanslyTriggerMapper.MapToTriggerDispatches(results);
        Assert.Single(dispatches);
        Assert.Equal(FanslyTriggerCodes.Message, dispatches[0].TriggerCode);
        Assert.Equal("ZerGo0_Bot", dispatches[0].Args["user"]);
        Assert.Equal("test", dispatches[0].Args["message"]);
        Assert.Equal(DateTimeOffset.FromUnixTimeMilliseconds(1751552950379L).ToString("yyyy-MM-dd HH:mm:ss"), dispatches[0].Args["timestamp"]);
    }

    [Fact]
    public void Processes_Private_Chat_Message_With_Subscription_Metadata()
    {
        List<FanslyProcessedEvent> results = FanslyMessageProcessor.ProcessRawMessage(PrivateChatRaw, FixedNow);
        FanslyChatMessageResult chat = Assert.IsType<FanslyChatMessageResult>(Assert.Single(results));

        Assert.Equal(1, chat.Message.Private);
        Assert.Equal("Plus", chat.Metadata.SenderSubscription.TierName);

        List<FanslyTriggerDispatch> dispatches = FanslyTriggerMapper.MapToTriggerDispatches(results);
        Assert.Single(dispatches);
        Assert.True((bool)dispatches[0].Args["isPrivate"]);
        Assert.Equal("Plus", dispatches[0].Args["subscriberTier"]);
        Assert.True((bool)dispatches[0].Args["isSubscriber"]);
    }

    [Fact]
    public void Processes_Tip_Message_As_Tip_And_Chat()
    {
        List<FanslyProcessedEvent> results = FanslyMessageProcessor.ProcessRawMessage(TipMessageRaw, FixedNow);
        Assert.Equal(2, results.Count);
        Assert.IsType<FanslyTipMessageResult>(results[0]);
        Assert.IsType<FanslyChatMessageResult>(results[1]);

        List<FanslyTriggerDispatch> dispatches = FanslyTriggerMapper.MapToTriggerDispatches(results);
        Assert.Equal(2, dispatches.Count);
        Assert.Equal(FanslyTriggerCodes.Tip, dispatches[0].TriggerCode);
        Assert.Equal(FanslyTriggerCodes.Message, dispatches[1].TriggerCode);
        Assert.Equal(100, dispatches[0].Args["tipAmount"]);
        Assert.Equal("$0.10", dispatches[0].Args["tipAmountDollars"]);
    }

    [Fact]
    public void Processes_Subscription_Message()
    {
        List<FanslyProcessedEvent> results = FanslyMessageProcessor.ProcessRawMessage(SubscriptionRaw, FixedNow);
        FanslySubscriptionResult subscription = Assert.IsType<FanslySubscriptionResult>(Assert.Single(results));
        Assert.Equal("Plus", subscription.Payload.SubscriptionTierName);

        List<FanslyTriggerDispatch> dispatches = FanslyTriggerMapper.MapToTriggerDispatches(results);
        Assert.Single(dispatches);
        Assert.Equal(FanslyTriggerCodes.Subscription, dispatches[0].TriggerCode);
        Assert.Equal(1, dispatches[0].Args["months"]);
    }

    [Fact]
    public void Processes_Session_Verified_Error_And_Ping_Envelopes()
    {
        List<FanslyProcessedEvent> sessionResults = FanslyMessageProcessor.ProcessRawMessage(SessionVerifiedRaw, FixedNow);
        List<FanslyProcessedEvent> errorResults = FanslyMessageProcessor.ProcessRawMessage(ErrorRaw, FixedNow);
        List<FanslyProcessedEvent> pingResults = FanslyMessageProcessor.ProcessRawMessage(PingRaw, FixedNow);

        Assert.IsType<FanslySessionVerifiedResult>(Assert.Single(sessionResults));
        Assert.IsType<FanslyErrorResult>(Assert.Single(errorResults));
        Assert.IsType<FanslyPingResponseResult>(Assert.Single(pingResults));

        Assert.Equal(FanslyTriggerCodes.SessionVerified, Assert.Single(FanslyTriggerMapper.MapToTriggerDispatches(sessionResults)).TriggerCode);
        Assert.Equal(FanslyTriggerCodes.Error, Assert.Single(FanslyTriggerMapper.MapToTriggerDispatches(errorResults)).TriggerCode);
        Assert.Equal(FanslyTriggerCodes.PingResponse, Assert.Single(FanslyTriggerMapper.MapToTriggerDispatches(pingResults)).TriggerCode);
    }

    [Fact]
    public void Flattens_Bulk_Envelope()
    {
        List<FanslyProcessedEvent> results = FanslyMessageProcessor.ProcessRawMessage(BulkRaw, FixedNow);
        Assert.Equal(2, results.Count);
        Assert.IsType<FanslyChatMessageResult>(results[0]);
        Assert.IsType<FanslySubscriptionResult>(results[1]);
    }

    [Fact]
    public void Processes_Moderation_And_Message_Removal_Events()
    {
        FanslyMessagesRemovedResult removed = Assert.IsType<FanslyMessagesRemovedResult>(Assert.Single(FanslyMessageProcessor.ProcessRawMessage(MessagesRemovedRaw, FixedNow)));
        FanslyModerationResult moderation = Assert.IsType<FanslyModerationResult>(Assert.Single(FanslyMessageProcessor.ProcessRawMessage(ModerationRaw, FixedNow)));

        Assert.Equal("user-1", removed.Payload.AccountId);
        Assert.Equal(2, moderation.Payload.Type);

        FanslyTriggerDispatch moderationDispatch = Assert.Single(FanslyTriggerMapper.MapToTriggerDispatches(new[] { moderation }));
        Assert.Equal(FanslyTriggerCodes.Moderation, moderationDispatch.TriggerCode);
        Assert.Equal("timeout", moderationDispatch.Args["moderationType"]);
        Assert.Equal(600.0, moderationDispatch.Args["durationSeconds"]);
    }

    [Fact]
    public void Processes_Chat_Room_Goal_Events()
    {
        FanslyGoalCreatedResult created = Assert.IsType<FanslyGoalCreatedResult>(Assert.Single(FanslyMessageProcessor.ProcessRawMessage(GoalCreatedRaw, FixedNow)));
        FanslyGoalUpdatedResult updated = Assert.IsType<FanslyGoalUpdatedResult>(Assert.Single(FanslyMessageProcessor.ProcessRawMessage(GoalUpdatedRaw, FixedNow)));

        Assert.Equal("goal-1", created.Payload.Id);
        Assert.Equal(2, updated.Payload.Status.Value);

        FanslyTriggerDispatch updatedDispatch = Assert.Single(FanslyTriggerMapper.MapToTriggerDispatches(new[] { updated }));
        Assert.Equal(FanslyTriggerCodes.GoalUpdated, updatedDispatch.TriggerCode);
        Assert.True((bool)updatedDispatch.Args["isDeleted"]);
        Assert.Equal("$5.00", updatedDispatch.Args["currentAmountDollars"]);
        Assert.Equal("$5.00", updatedDispatch.Args["goalAmountDollars"]);
    }

    [Fact]
    public void Processes_Chat_Room_And_Settings_Updates()
    {
        FanslyChatRoomUpdatedResult roomUpdated = Assert.IsType<FanslyChatRoomUpdatedResult>(Assert.Single(FanslyMessageProcessor.ProcessRawMessage(RoomUpdatedRaw, FixedNow)));
        FanslySettingsUpdatedResult settingsUpdated = Assert.IsType<FanslySettingsUpdatedResult>(Assert.Single(FanslyMessageProcessor.ProcessRawMessage(SettingsUpdatedRaw, FixedNow)));

        Assert.Equal("room-1", roomUpdated.Payload.Id);
        Assert.Equal(15000L, settingsUpdated.Payload.Slowmode.Value);

        FanslyTriggerDispatch settingsDispatch = Assert.Single(FanslyTriggerMapper.MapToTriggerDispatches(new[] { settingsUpdated }));
        Assert.Equal(FanslyTriggerCodes.SettingsUpdated, settingsDispatch.TriggerCode);
        Assert.True((bool)settingsDispatch.Args["isSlowmodeEnabled"]);
        Assert.Equal(15.0, settingsDispatch.Args["slowmodeSeconds"]);
    }

    [Fact]
    public void Ignores_Malformed_Metadata_And_Unknown_Envelope_Types()
    {
        FanslyChatMessageResult chat = Assert.IsType<FanslyChatMessageResult>(Assert.Single(FanslyMessageProcessor.ProcessRawMessage(MalformedMetadataRaw, FixedNow)));
        Assert.False(chat.Metadata.SenderIsCreator);
        Assert.Null(chat.Metadata.SenderSubscription);

        List<FanslyProcessedEvent> unknownResults = FanslyMessageProcessor.ProcessRawMessage(UnknownEnvelopeRaw, FixedNow);
        Assert.Empty(unknownResults);
    }

    private const string NormalChatRaw = """
{
  "t": 10000,
  "d": "{\"serviceId\":46,\"event\":\"{\\\"type\\\":10,\\\"chatRoomMessage\\\":{\\\"chatRoomId\\\":\\\"408830844350771200\\\",\\\"senderId\\\":\\\"407996034761891840\\\",\\\"content\\\":\\\"test\\\",\\\"type\\\":0,\\\"private\\\":0,\\\"attachments\\\":[],\\\"accountFlags\\\":1,\\\"metadata\\\":\\\"{\\\\\\\"senderIsCreator\\\\\\\":true,\\\\\\\"senderIsStaff\\\\\\\":false}\\\",\\\"chatRoomAccountId\\\":\\\"407996034761891840\\\",\\\"id\\\":\\\"797163507798781952\\\",\\\"createdAt\\\":1751552950379,\\\"embeds\\\":[],\\\"usernameColor\\\":\\\"#00ff19\\\",\\\"username\\\":\\\"ZerGo0_Bot\\\",\\\"displayname\\\":\\\"ZerGo0_Bot\\\"}}\"}"
}
""";

    private const string PrivateChatRaw = """
{
  "t": 10000,
  "d": "{\"serviceId\":46,\"event\":\"{\\\"type\\\":10,\\\"chatRoomMessage\\\":{\\\"chatRoomId\\\":\\\"408830844350771200\\\",\\\"senderId\\\":\\\"281038385793998848\\\",\\\"content\\\":\\\"private test 123\\\",\\\"type\\\":0,\\\"private\\\":1,\\\"attachments\\\":[],\\\"accountFlags\\\":6,\\\"metadata\\\":\\\"{\\\\\\\"senderIsCreator\\\\\\\":false,\\\\\\\"senderIsStaff\\\\\\\":false,\\\\\\\"senderIsFollowing\\\\\\\":true,\\\\\\\"senderSubscription\\\\\\\":{\\\\\\\"tierId\\\\\\\":\\\\\\\"795201999690801152\\\\\\\",\\\\\\\"tierColor\\\\\\\":\\\\\\\"#F73838\\\\\\\",\\\\\\\"tierName\\\\\\\":\\\\\\\"Plus\\\\\\\"}}\\\",\\\"chatRoomAccountId\\\":\\\"407996034761891840\\\",\\\"id\\\":\\\"896343079546531840\\\",\\\"createdAt\\\":1775199202949,\\\"embeds\\\":[],\\\"usernameColor\\\":\\\"#0066ff\\\",\\\"username\\\":\\\"ZerGo0\\\",\\\"displayname\\\":\\\"ZerGo0\\\"}}\"}"
}
""";

    private const string TipMessageRaw = """
{
  "t": 10000,
  "d": "{\"serviceId\":46,\"event\":\"{\\\"type\\\":10,\\\"chatRoomMessage\\\":{\\\"chatRoomId\\\":\\\"408830844350771200\\\",\\\"senderId\\\":\\\"281038385793998848\\\",\\\"content\\\":\\\"tip test\\\",\\\"type\\\":0,\\\"private\\\":0,\\\"attachments\\\":[{\\\"contentType\\\":7,\\\"contentId\\\":\\\"797163797902008322\\\",\\\"metadata\\\":\\\"{\\\\\\\"amount\\\\\\\":100}\\\",\\\"chatRoomMessageId\\\":\\\"797163798283694080\\\"}],\\\"accountFlags\\\":6,\\\"messageTip\\\":null,\\\"metadata\\\":\\\"{\\\\\\\"senderIsCreator\\\\\\\":false,\\\\\\\"senderIsStaff\\\\\\\":false,\\\\\\\"senderIsFollowing\\\\\\\":true,\\\\\\\"senderSubscription\\\\\\\":{\\\\\\\"tierId\\\\\\\":\\\\\\\"795201999690801152\\\\\\\",\\\\\\\"tierColor\\\\\\\":\\\\\\\"#F73838\\\\\\\",\\\\\\\"tierName\\\\\\\":\\\\\\\"Plus\\\\\\\"}}\\\",\\\"chatRoomAccountId\\\":\\\"407996034761891840\\\",\\\"id\\\":\\\"797163798283694080\\\",\\\"createdAt\\\":1751553019637,\\\"embeds\\\":[],\\\"usernameColor\\\":\\\"#0066ff\\\",\\\"username\\\":\\\"ZerGo0\\\",\\\"displayname\\\":\\\"ZerGo0\\\"}}\"}"
}
""";

    private const string SubscriptionRaw = """
{
  "t": 10000,
  "d": "{\"serviceId\":46,\"event\":\"{\\\"type\\\":53,\\\"subAlert\\\":{\\\"chatRoomId\\\":\\\"408830844350771200\\\",\\\"senderId\\\":\\\"281038385793998848\\\",\\\"historyId\\\":\\\"797165341313605638\\\",\\\"subscriberId\\\":\\\"281038385793998848\\\",\\\"subscriptionTierId\\\":\\\"795201999690801152\\\",\\\"subscriptionTierName\\\":\\\"Plus\\\",\\\"subscriptionTierColor\\\":\\\"#F73838\\\",\\\"subscriptionStreak\\\":1,\\\"subscriptionTotalDays\\\":30,\\\"id\\\":\\\"797165385945198592\\\",\\\"usernameColor\\\":\\\"#0066ff\\\",\\\"username\\\":\\\"ZerGo0\\\",\\\"displayname\\\":\\\"ZerGo0\\\"}}\"}"
}
""";

    private const string SessionVerifiedRaw = """
{
  "t": 1,
  "d": "{\"v\":3,\"accountId\":\"123\"}"
}
""";

    private const string ErrorRaw = """
{
  "t": 0,
  "d": "{\"code\":401,\"message\":\"unauthorized\"}"
}
""";

    private const string PingRaw = """
{
  "t": 2,
  "d": "{\"ts\":1775199202949}"
}
""";

    private const string BulkRaw = """
{
  "t": 10001,
  "d": "[{\"t\":10000,\"d\":\"{\\\"serviceId\\\":46,\\\"event\\\":\\\"{\\\\\\\"type\\\\\\\":10,\\\\\\\"chatRoomMessage\\\\\\\":{\\\\\\\"chatRoomId\\\\\\\":\\\\\\\"room-1\\\\\\\",\\\\\\\"senderId\\\\\\\":\\\\\\\"user-1\\\\\\\",\\\\\\\"content\\\\\\\":\\\\\\\"bulk chat\\\\\\\",\\\\\\\"type\\\\\\\":0,\\\\\\\"private\\\\\\\":0,\\\\\\\"attachments\\\\\\\":[],\\\\\\\"accountFlags\\\\\\\":0,\\\\\\\"metadata\\\\\\\":\\\\\\\"\\\\\\\",\\\\\\\"chatRoomAccountId\\\\\\\":\\\\\\\"owner-1\\\\\\\",\\\\\\\"id\\\\\\\":\\\\\\\"message-1\\\\\\\",\\\\\\\"createdAt\\\\\\\":1751553019637,\\\\\\\"embeds\\\\\\\":[],\\\\\\\"usernameColor\\\\\\\":\\\\\\\"#abcdef\\\\\\\",\\\\\\\"username\\\\\\\":\\\\\\\"BulkUser\\\\\\\",\\\\\\\"displayname\\\\\\\":\\\\\\\"BulkUser\\\\\\\"}}\\\"}\"},{\"t\":10000,\"d\":\"{\\\"serviceId\\\":46,\\\"event\\\":\\\"{\\\\\\\"type\\\\\\\":53,\\\\\\\"subAlert\\\\\\\":{\\\\\\\"chatRoomId\\\\\\\":\\\\\\\"room-1\\\\\\\",\\\\\\\"senderId\\\\\\\":\\\\\\\"user-2\\\\\\\",\\\\\\\"historyId\\\\\\\":\\\\\\\"history-1\\\\\\\",\\\\\\\"subscriberId\\\\\\\":\\\\\\\"user-2\\\\\\\",\\\\\\\"subscriptionTierId\\\\\\\":\\\\\\\"tier-1\\\\\\\",\\\\\\\"subscriptionTierName\\\\\\\":\\\\\\\"VIP\\\\\\\",\\\\\\\"subscriptionTierColor\\\\\\\":\\\\\\\"#ff0000\\\\\\\",\\\\\\\"subscriptionStreak\\\\\\\":2,\\\\\\\"subscriptionTotalDays\\\\\\\":60,\\\\\\\"id\\\\\\\":\\\\\\\"sub-1\\\\\\\",\\\\\\\"usernameColor\\\\\\\":\\\\\\\"#ff0000\\\\\\\",\\\\\\\"username\\\\\\\":\\\\\\\"BulkSub\\\\\\\",\\\\\\\"displayname\\\\\\\":\\\\\\\"BulkSub\\\\\\\"}}\\\"}\"}]"
}
""";

    private const string MessagesRemovedRaw = """
{
  "t": 10000,
  "d": "{\"serviceId\":46,\"event\":\"{\\\"type\\\":20,\\\"chatRoomBan\\\":{\\\"id\\\":\\\"ban-1\\\",\\\"chatRoomId\\\":\\\"room-1\\\",\\\"accountId\\\":\\\"user-1\\\",\\\"type\\\":1,\\\"duration\\\":0}}\"}"
}
""";

    private const string ModerationRaw = """
{
  "t": 10000,
  "d": "{\"serviceId\":46,\"event\":\"{\\\"type\\\":30,\\\"chatRoomBan\\\":{\\\"id\\\":\\\"ban-2\\\",\\\"chatRoomId\\\":\\\"room-1\\\",\\\"accountId\\\":\\\"user-2\\\",\\\"type\\\":2,\\\"duration\\\":600000}}\"}"
}
""";

    private const string GoalCreatedRaw = """
{
  "t": 10000,
  "d": "{\"serviceId\":46,\"event\":\"{\\\"type\\\":50,\\\"chatRoomGoal\\\":{\\\"id\\\":\\\"goal-1\\\",\\\"chatRoomId\\\":\\\"room-1\\\",\\\"accountId\\\":\\\"owner-1\\\",\\\"status\\\":1,\\\"currentAmount\\\":1500,\\\"goalAmount\\\":5000,\\\"label\\\":\\\"New mic\\\",\\\"description\\\":\\\"Upgrade stream audio\\\",\\\"version\\\":1}}\"}"
}
""";

    private const string GoalUpdatedRaw = """
{
  "t": 10000,
  "d": "{\"serviceId\":46,\"event\":\"{\\\"type\\\":51,\\\"chatRoomGoal\\\":{\\\"id\\\":\\\"goal-1\\\",\\\"chatRoomId\\\":\\\"room-1\\\",\\\"accountId\\\":\\\"owner-1\\\",\\\"status\\\":2,\\\"currentAmount\\\":5000,\\\"goalAmount\\\":5000,\\\"label\\\":\\\"New mic\\\",\\\"description\\\":\\\"Upgrade stream audio\\\",\\\"version\\\":2,\\\"deletedAt\\\":1775199202949}}\"}"
}
""";

    private const string RoomUpdatedRaw = """
{
  "t": 10000,
  "d": "{\"serviceId\":46,\"event\":\"{\\\"type\\\":4,\\\"chatRoom\\\":{\\\"id\\\":\\\"room-1\\\",\\\"accountId\\\":\\\"owner-1\\\",\\\"flags\\\":8}}\"}"
}
""";

    private const string SettingsUpdatedRaw = """
{
  "t": 10000,
  "d": "{\"serviceId\":46,\"event\":\"{\\\"type\\\":54,\\\"settings\\\":{\\\"chatRoomId\\\":\\\"room-1\\\",\\\"slowmode\\\":15000}}\"}"
}
""";

    private const string MalformedMetadataRaw = """
{
  "t": 10000,
  "d": "{\"serviceId\":46,\"event\":\"{\\\"type\\\":10,\\\"chatRoomMessage\\\":{\\\"chatRoomId\\\":\\\"room-1\\\",\\\"senderId\\\":\\\"user-1\\\",\\\"content\\\":\\\"broken metadata\\\",\\\"type\\\":0,\\\"private\\\":0,\\\"attachments\\\":[],\\\"accountFlags\\\":0,\\\"metadata\\\":\\\"{not-json\\\",\\\"chatRoomAccountId\\\":\\\"owner-1\\\",\\\"id\\\":\\\"message-1\\\",\\\"createdAt\\\":1751553019637,\\\"embeds\\\":[],\\\"usernameColor\\\":\\\"#abcdef\\\",\\\"username\\\":\\\"Broken\\\",\\\"displayname\\\":\\\"Broken\\\"}}\"}"
}
""";

    private const string UnknownEnvelopeRaw = """
{
  "t": 99999,
  "d": "{}"
}
""";
}
