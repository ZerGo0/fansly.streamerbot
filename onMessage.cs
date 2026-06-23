using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

#if !FANSLY_TESTING
public class CPHInline
{
    public bool Execute()
    {
        CPH.LogInfo("[Fansly] onMessage");

        if (!args.ContainsKey("message"))
        {
            CPH.LogError("[Fansly] message not found in args");
            return false;
        }

        string message = args["message"].ToString();
        if (string.IsNullOrEmpty(message))
        {
            CPH.LogError("[Fansly] message is empty");
            return false;
        }

        CPH.LogDebug($"[Fansly] Raw message: {message}");

        try
        {
            List<FanslyProcessedEvent> processedEvents = FanslyMessageProcessor.ProcessRawMessage(message, DateTimeOffset.Now);
            if (processedEvents.Count == 0)
            {
                CPH.LogDebug("[Fansly] No supported events found in message");
                return true;
            }

            List<FanslyTriggerDispatch> dispatches = FanslyTriggerMapper.MapToTriggerDispatches(processedEvents);
            foreach (FanslyTriggerDispatch dispatch in dispatches)
            {
                CPH.TriggerCodeEvent(dispatch.TriggerCode, dispatch.Args);
                CPH.LogInfo($"[Fansly] Triggered {dispatch.TriggerCode} for {dispatch.EventType}");
            }

            return true;
        }
        catch (Exception ex)
        {
            CPH.LogError($"[Fansly] Error processing message: {ex.Message}");
            return false;
        }
    }
}
#endif

public static class FanslyProtocolConstants
{
    public const int ErrorEnvelopeType = 0;
    public const int SessionVerifiedEnvelopeType = 1;
    public const int PingResponseEnvelopeType = 2;
    public const int ServiceEventEnvelopeType = 10000;
    public const int BulkEnvelopeType = 10001;

    public const int ChatRoomServiceId = 46;

    public const int ChatRoomUpdatedEventType = 4;
    public const int ChatMessageEventType = 10;
    public const int MessagesRemovedEventType = 20;
    public const int ModerationEventType = 30;
    public const int GoalCreatedEventType = 50;
    public const int GoalUpdatedEventType = 51;
    public const int SubscriptionEventType = 53;
    public const int SettingsUpdatedEventType = 54;

    public const int TipAttachmentType = 7;
}

public static class FanslyTriggerCodes
{
    public const string Message = "fanslyMessage";
    public const string Tip = "fanslyTip";
    public const string Subscription = "fanslySubscription";
    public const string SessionVerified = "fanslySessionVerified";
    public const string Error = "fanslyError";
    public const string PingResponse = "fanslyPingResponse";
    public const string ChatRoomUpdated = "fanslyChatRoomUpdated";
    public const string MessagesRemoved = "fanslyMessagesRemoved";
    public const string Moderation = "fanslyModeration";
    public const string GoalCreated = "fanslyGoalCreated";
    public const string GoalUpdated = "fanslyGoalUpdated";
    public const string SettingsUpdated = "fanslySettingsUpdated";
}

public static class FanslyMessageProcessor
{
    public static List<FanslyProcessedEvent> ProcessRawMessage(string rawMessage, DateTimeOffset? now = null)
    {
        List<FanslyProcessedEvent> results = new List<FanslyProcessedEvent>();
        if (string.IsNullOrWhiteSpace(rawMessage))
        {
            return results;
        }

        DateTimeOffset resolvedNow = now ?? DateTimeOffset.Now;
        ProcessEnvelopeString(rawMessage, resolvedNow, results);
        return results;
    }

    private static void ProcessEnvelopeString(string rawMessage, DateTimeOffset now, IList<FanslyProcessedEvent> results)
    {
        JToken envelopeToken = TryParseJson(rawMessage);
        if (envelopeToken == null)
        {
            return;
        }

        ProcessEnvelopeToken(envelopeToken, rawMessage, now, results);
    }

    private static void ProcessEnvelopeToken(JToken envelopeToken, string rawJson, DateTimeOffset now, IList<FanslyProcessedEvent> results)
    {
        JObject envelope = envelopeToken as JObject;
        if (envelope == null)
        {
            return;
        }

        int? envelopeType = GetNullableInt(envelope["t"]);
        if (!envelopeType.HasValue)
        {
            return;
        }

        switch (envelopeType.Value)
        {
            case FanslyProtocolConstants.ErrorEnvelopeType:
                AddErrorResult(envelope["d"], rawJson, now, results);
                break;

            case FanslyProtocolConstants.SessionVerifiedEnvelopeType:
                AddSessionVerifiedResult(envelope["d"], rawJson, now, results);
                break;

            case FanslyProtocolConstants.PingResponseEnvelopeType:
                AddPingResponseResult(envelope["d"], rawJson, now, results);
                break;

            case FanslyProtocolConstants.ServiceEventEnvelopeType:
                AddServiceEventResults(envelope["d"], rawJson, now, results);
                break;

            case FanslyProtocolConstants.BulkEnvelopeType:
                AddBulkResults(envelope["d"], now, results);
                break;
        }
    }

    private static void AddErrorResult(JToken dataToken, string rawJson, DateTimeOffset now, IList<FanslyProcessedEvent> results)
    {
        ErrorEvent errorEvent = DeserializeToken<ErrorEvent>(ResolveEmbeddedJson(dataToken));
        if (errorEvent == null)
        {
            return;
        }

        results.Add(new FanslyErrorResult(rawJson, now, errorEvent));
    }

    private static void AddSessionVerifiedResult(JToken dataToken, string rawJson, DateTimeOffset now, IList<FanslyProcessedEvent> results)
    {
        SessionVerifiedEvent sessionVerified = DeserializeToken<SessionVerifiedEvent>(ResolveEmbeddedJson(dataToken));
        if (sessionVerified == null)
        {
            sessionVerified = new SessionVerifiedEvent();
        }

        results.Add(new FanslySessionVerifiedResult(rawJson, now, sessionVerified));
    }

    private static void AddPingResponseResult(JToken dataToken, string rawJson, DateTimeOffset now, IList<FanslyProcessedEvent> results)
    {
        PingResponseEvent pingResponse = DeserializeToken<PingResponseEvent>(ResolveEmbeddedJson(dataToken));
        if (pingResponse == null)
        {
            pingResponse = new PingResponseEvent();
        }

        results.Add(new FanslyPingResponseResult(rawJson, now, pingResponse));
    }

    private static void AddServiceEventResults(JToken dataToken, string rawJson, DateTimeOffset now, IList<FanslyProcessedEvent> results)
    {
        FanslyServiceEvent serviceEvent = DeserializeToken<FanslyServiceEvent>(ResolveEmbeddedJson(dataToken));
        if (serviceEvent == null || serviceEvent.ServiceId != FanslyProtocolConstants.ChatRoomServiceId)
        {
            return;
        }

        ChatRoomServiceEventEnvelope eventEnvelope = DeserializeToken<ChatRoomServiceEventEnvelope>(ResolveEmbeddedJson(serviceEvent.Event));
        if (eventEnvelope == null)
        {
            return;
        }

        switch (eventEnvelope.Type)
        {
            case FanslyProtocolConstants.ChatRoomUpdatedEventType:
                if (eventEnvelope.ChatRoom != null)
                {
                    results.Add(new FanslyChatRoomUpdatedResult(rawJson, now, eventEnvelope.ChatRoom));
                }
                break;

            case FanslyProtocolConstants.ChatMessageEventType:
                if (eventEnvelope.ChatRoomMessage != null)
                {
                    ChatRoomMessageMetadata metadata = ParseMessageMetadata(eventEnvelope.ChatRoomMessage.Metadata);
                    TipAttachmentMetadata tip = ExtractTipAttachment(eventEnvelope.ChatRoomMessage.Attachments);
                    DateTimeOffset timestamp = ResolveMessageTimestamp(eventEnvelope.ChatRoomMessage.CreatedAt, now);

                    if (tip != null)
                    {
                        results.Add(new FanslyTipMessageResult(rawJson, timestamp, eventEnvelope.ChatRoomMessage, metadata, tip));
                    }

                    results.Add(new FanslyChatMessageResult(rawJson, timestamp, eventEnvelope.ChatRoomMessage, metadata, tip));
                }
                break;

            case FanslyProtocolConstants.MessagesRemovedEventType:
                if (eventEnvelope.ChatRoomBan != null)
                {
                    results.Add(new FanslyMessagesRemovedResult(rawJson, now, eventEnvelope.ChatRoomBan));
                }
                break;

            case FanslyProtocolConstants.ModerationEventType:
                if (eventEnvelope.ChatRoomBan != null)
                {
                    results.Add(new FanslyModerationResult(rawJson, now, eventEnvelope.ChatRoomBan));
                }
                break;

            case FanslyProtocolConstants.GoalCreatedEventType:
                if (eventEnvelope.ChatRoomGoal != null)
                {
                    results.Add(new FanslyGoalCreatedResult(rawJson, now, eventEnvelope.ChatRoomGoal));
                }
                break;

            case FanslyProtocolConstants.GoalUpdatedEventType:
                if (eventEnvelope.ChatRoomGoal != null)
                {
                    results.Add(new FanslyGoalUpdatedResult(rawJson, now, eventEnvelope.ChatRoomGoal));
                }
                break;

            case FanslyProtocolConstants.SubscriptionEventType:
                if (eventEnvelope.SubAlert != null)
                {
                    results.Add(new FanslySubscriptionResult(rawJson, now, eventEnvelope.SubAlert));
                }
                break;

            case FanslyProtocolConstants.SettingsUpdatedEventType:
                if (eventEnvelope.Settings != null)
                {
                    results.Add(new FanslySettingsUpdatedResult(rawJson, now, eventEnvelope.Settings));
                }
                break;
        }
    }

    private static void AddBulkResults(JToken dataToken, DateTimeOffset now, IList<FanslyProcessedEvent> results)
    {
        JToken resolved = ResolveEmbeddedJson(dataToken);
        JArray bulkMessages = resolved as JArray;
        if (bulkMessages == null)
        {
            return;
        }

        foreach (JToken item in bulkMessages)
        {
            string rawJson = ToCompactJson(item);
            if (!string.IsNullOrEmpty(rawJson))
            {
                ProcessEnvelopeString(rawJson, now, results);
            }
        }
    }

    private static JToken ResolveEmbeddedJson(JToken token)
    {
        if (token == null)
        {
            return null;
        }

        if (token.Type != JTokenType.String)
        {
            return token;
        }

        string raw = token.Value<string>();
        if (string.IsNullOrWhiteSpace(raw))
        {
            return null;
        }

        JToken parsed = TryParseJson(raw);
        return parsed ?? token;
    }

    private static T DeserializeToken<T>(JToken token) where T : class
    {
        if (token == null)
        {
            return null;
        }

        try
        {
            return token.ToObject<T>();
        }
        catch
        {
            return null;
        }
    }

    private static JToken TryParseJson(string json)
    {
        try
        {
            return JToken.Parse(json);
        }
        catch
        {
            return null;
        }
    }

    private static int? GetNullableInt(JToken token)
    {
        if (token == null)
        {
            return null;
        }

        int parsed;
        if (int.TryParse(token.ToString(), out parsed))
        {
            return parsed;
        }

        return null;
    }

    private static DateTimeOffset ResolveMessageTimestamp(long createdAt, DateTimeOffset fallback)
    {
        if (createdAt <= 0)
        {
            return fallback;
        }

        try
        {
            return DateTimeOffset.FromUnixTimeMilliseconds(createdAt);
        }
        catch
        {
            return fallback;
        }
    }

    private static ChatRoomMessageMetadata ParseMessageMetadata(string metadataJson)
    {
        if (string.IsNullOrWhiteSpace(metadataJson))
        {
            return new ChatRoomMessageMetadata();
        }

        ChatRoomMessageMetadata metadata = DeserializeToken<ChatRoomMessageMetadata>(TryParseJson(metadataJson));
        return metadata ?? new ChatRoomMessageMetadata();
    }

    private static TipAttachmentMetadata ExtractTipAttachment(IList<ChatRoomAttachment> attachments)
    {
        if (attachments == null)
        {
            return null;
        }

        for (int index = 0; index < attachments.Count; index++)
        {
            ChatRoomAttachment attachment = attachments[index];
            if (attachment == null || attachment.ContentType != FanslyProtocolConstants.TipAttachmentType)
            {
                continue;
            }

            TipAttachmentMetadata metadata = DeserializeToken<TipAttachmentMetadata>(TryParseJson(attachment.Metadata));
            if (metadata == null)
            {
                metadata = new TipAttachmentMetadata();
            }

            return metadata;
        }

        return null;
    }

    private static string ToCompactJson(JToken token)
    {
        return token == null ? string.Empty : token.ToString(Formatting.None);
    }
}

public static class FanslyTriggerMapper
{
    public static List<FanslyTriggerDispatch> MapToTriggerDispatches(IEnumerable<FanslyProcessedEvent> processedEvents)
    {
        List<FanslyTriggerDispatch> dispatches = new List<FanslyTriggerDispatch>();
        if (processedEvents == null)
        {
            return dispatches;
        }

        foreach (FanslyProcessedEvent processedEvent in processedEvents)
        {
            if (processedEvent is FanslyTipMessageResult)
            {
                FanslyTipMessageResult tip = (FanslyTipMessageResult)processedEvent;
                dispatches.Add(new FanslyTriggerDispatch(FanslyTriggerCodes.Tip, tip.EventType, BuildChatArgs(tip.Message, tip.Metadata, tip.Tip, processedEvent)));
                continue;
            }

            if (processedEvent is FanslyChatMessageResult)
            {
                FanslyChatMessageResult chat = (FanslyChatMessageResult)processedEvent;
                dispatches.Add(new FanslyTriggerDispatch(FanslyTriggerCodes.Message, chat.EventType, BuildChatArgs(chat.Message, chat.Metadata, chat.Tip, processedEvent)));
                continue;
            }

            if (processedEvent is FanslySubscriptionResult)
            {
                FanslySubscriptionResult subscription = (FanslySubscriptionResult)processedEvent;
                dispatches.Add(new FanslyTriggerDispatch(FanslyTriggerCodes.Subscription, subscription.EventType, BuildSubscriptionArgs(subscription)));
                continue;
            }

            if (processedEvent is FanslySessionVerifiedResult)
            {
                FanslySessionVerifiedResult sessionVerified = (FanslySessionVerifiedResult)processedEvent;
                Dictionary<string, object> args = CreateBaseArgs(sessionVerified, null);
                args["version"] = sessionVerified.Payload.Version.HasValue ? sessionVerified.Payload.Version.Value : 0;
                args["payloadJson"] = SerializeObject(sessionVerified.Payload);
                dispatches.Add(new FanslyTriggerDispatch(FanslyTriggerCodes.SessionVerified, sessionVerified.EventType, args));
                continue;
            }

            if (processedEvent is FanslyErrorResult)
            {
                FanslyErrorResult error = (FanslyErrorResult)processedEvent;
                Dictionary<string, object> args = CreateBaseArgs(error, null);
                args["code"] = error.Payload.Code;
                args["message"] = error.Payload.Message ?? string.Empty;
                args["payloadJson"] = SerializeObject(error.Payload);
                dispatches.Add(new FanslyTriggerDispatch(FanslyTriggerCodes.Error, error.EventType, args));
                continue;
            }

            if (processedEvent is FanslyPingResponseResult)
            {
                FanslyPingResponseResult pingResponse = (FanslyPingResponseResult)processedEvent;
                Dictionary<string, object> args = CreateBaseArgs(pingResponse, null);
                args["payloadJson"] = SerializeObject(pingResponse.Payload);
                dispatches.Add(new FanslyTriggerDispatch(FanslyTriggerCodes.PingResponse, pingResponse.EventType, args));
                continue;
            }

            if (processedEvent is FanslyChatRoomUpdatedResult)
            {
                FanslyChatRoomUpdatedResult roomUpdated = (FanslyChatRoomUpdatedResult)processedEvent;
                Dictionary<string, object> args = CreateBaseArgs(roomUpdated, roomUpdated.Payload.Id);
                args["accountId"] = roomUpdated.Payload.AccountId ?? string.Empty;
                args["flags"] = roomUpdated.Payload.Flags.HasValue ? roomUpdated.Payload.Flags.Value : 0;
                args["payloadJson"] = SerializeObject(roomUpdated.Payload);
                dispatches.Add(new FanslyTriggerDispatch(FanslyTriggerCodes.ChatRoomUpdated, roomUpdated.EventType, args));
                continue;
            }

            if (processedEvent is FanslyMessagesRemovedResult)
            {
                FanslyMessagesRemovedResult removed = (FanslyMessagesRemovedResult)processedEvent;
                Dictionary<string, object> args = CreateBaseArgs(removed, removed.Payload.ChatRoomId);
                args["moderationId"] = removed.Payload.Id ?? string.Empty;
                args["accountId"] = removed.Payload.AccountId ?? string.Empty;
                args["moderationType"] = GetModerationTypeName(removed.Payload.Type);
                args["durationMs"] = removed.Payload.Duration.HasValue ? removed.Payload.Duration.Value : 0;
                args["durationSeconds"] = removed.Payload.Duration.HasValue ? removed.Payload.Duration.Value / 1000.0 : 0.0;
                args["payloadJson"] = SerializeObject(removed.Payload);
                dispatches.Add(new FanslyTriggerDispatch(FanslyTriggerCodes.MessagesRemoved, removed.EventType, args));
                continue;
            }

            if (processedEvent is FanslyModerationResult)
            {
                FanslyModerationResult moderation = (FanslyModerationResult)processedEvent;
                Dictionary<string, object> args = CreateBaseArgs(moderation, moderation.Payload.ChatRoomId);
                args["moderationId"] = moderation.Payload.Id ?? string.Empty;
                args["accountId"] = moderation.Payload.AccountId ?? string.Empty;
                args["moderationType"] = GetModerationTypeName(moderation.Payload.Type);
                args["moderationTypeId"] = moderation.Payload.Type;
                args["durationMs"] = moderation.Payload.Duration.HasValue ? moderation.Payload.Duration.Value : 0;
                args["durationSeconds"] = moderation.Payload.Duration.HasValue ? moderation.Payload.Duration.Value / 1000.0 : 0.0;
                args["payloadJson"] = SerializeObject(moderation.Payload);
                dispatches.Add(new FanslyTriggerDispatch(FanslyTriggerCodes.Moderation, moderation.EventType, args));
                continue;
            }

            if (processedEvent is FanslyGoalCreatedResult)
            {
                FanslyGoalCreatedResult goalCreated = (FanslyGoalCreatedResult)processedEvent;
                dispatches.Add(new FanslyTriggerDispatch(FanslyTriggerCodes.GoalCreated, goalCreated.EventType, BuildGoalArgs(goalCreated, goalCreated.Payload)));
                continue;
            }

            if (processedEvent is FanslyGoalUpdatedResult)
            {
                FanslyGoalUpdatedResult goalUpdated = (FanslyGoalUpdatedResult)processedEvent;
                dispatches.Add(new FanslyTriggerDispatch(FanslyTriggerCodes.GoalUpdated, goalUpdated.EventType, BuildGoalArgs(goalUpdated, goalUpdated.Payload)));
                continue;
            }

            if (processedEvent is FanslySettingsUpdatedResult)
            {
                FanslySettingsUpdatedResult settingsUpdated = (FanslySettingsUpdatedResult)processedEvent;
                Dictionary<string, object> args = CreateBaseArgs(settingsUpdated, settingsUpdated.Payload.ChatRoomId);
                args["slowmodeMs"] = settingsUpdated.Payload.Slowmode.HasValue ? settingsUpdated.Payload.Slowmode.Value : 0;
                args["slowmodeSeconds"] = settingsUpdated.Payload.Slowmode.HasValue ? settingsUpdated.Payload.Slowmode.Value / 1000.0 : 0.0;
                args["isSlowmodeEnabled"] = settingsUpdated.Payload.Slowmode.HasValue && settingsUpdated.Payload.Slowmode.Value > 0;
                args["payloadJson"] = SerializeObject(settingsUpdated.Payload);
                dispatches.Add(new FanslyTriggerDispatch(FanslyTriggerCodes.SettingsUpdated, settingsUpdated.EventType, args));
            }
        }

        return dispatches;
    }

    private static Dictionary<string, object> BuildChatArgs(ChatRoomMessage message, ChatRoomMessageMetadata metadata, TipAttachmentMetadata tip, FanslyProcessedEvent processedEvent)
    {
        Dictionary<string, object> args = CreateBaseArgs(processedEvent, message.ChatRoomId);
        args["user"] = message.Username ?? "Unknown";
        args["userName"] = message.Username ?? "Unknown";
        args["displayName"] = string.IsNullOrEmpty(message.DisplayName) ? (message.Username ?? "Unknown") : message.DisplayName;
        args["userId"] = message.SenderId ?? string.Empty;
        args["message"] = message.Content ?? string.Empty;
        args["messageId"] = message.Id ?? string.Empty;
        args["chatRoomId"] = message.ChatRoomId ?? string.Empty;
        args["usernameColor"] = string.IsNullOrEmpty(message.UsernameColor) ? "#FFFFFF" : message.UsernameColor;
        args["isCreator"] = metadata != null && metadata.SenderIsCreator;
        args["isStaff"] = metadata != null && metadata.SenderIsStaff;
        args["isFollowing"] = metadata != null && metadata.SenderIsFollowing;
        args["isSubscriber"] = metadata != null && metadata.SenderSubscription != null && !string.IsNullOrEmpty(metadata.SenderSubscription.TierName);
        args["subscriberTier"] = metadata != null && metadata.SenderSubscription != null ? metadata.SenderSubscription.TierName ?? string.Empty : string.Empty;
        args["subscriberTierColor"] = metadata != null && metadata.SenderSubscription != null ? metadata.SenderSubscription.TierColor ?? string.Empty : string.Empty;
        args["subscriberTierId"] = metadata != null && metadata.SenderSubscription != null ? metadata.SenderSubscription.TierId ?? string.Empty : string.Empty;
        args["isPrivate"] = message.Private == 1;
        args["accountFlags"] = message.AccountFlags;
        args["messageType"] = message.Type;

        if (tip != null)
        {
            args["tipAmount"] = tip.Amount;
            args["tipAmountDollars"] = FormatTipAmount(tip.Amount);
        }

        return args;
    }

    private static Dictionary<string, object> BuildSubscriptionArgs(FanslySubscriptionResult subscription)
    {
        Dictionary<string, object> args = CreateBaseArgs(subscription, subscription.Payload.ChatRoomId);
        args["user"] = subscription.Payload.Username ?? "Unknown";
        args["userName"] = subscription.Payload.Username ?? "Unknown";
        args["displayName"] = string.IsNullOrEmpty(subscription.Payload.DisplayName) ? (subscription.Payload.Username ?? "Unknown") : subscription.Payload.DisplayName;
        args["userId"] = subscription.Payload.SubscriberId ?? string.Empty;
        args["tier"] = subscription.Payload.SubscriptionTierName ?? string.Empty;
        args["tierName"] = subscription.Payload.SubscriptionTierName ?? string.Empty;
        args["tierColor"] = subscription.Payload.SubscriptionTierColor ?? string.Empty;
        args["tierId"] = subscription.Payload.SubscriptionTierId ?? string.Empty;
        args["streak"] = subscription.Payload.SubscriptionStreak.HasValue ? subscription.Payload.SubscriptionStreak.Value : 0;
        args["totalDays"] = subscription.Payload.SubscriptionTotalDays.HasValue ? subscription.Payload.SubscriptionTotalDays.Value : 0;
        args["months"] = subscription.Payload.SubscriptionTotalDays.HasValue ? subscription.Payload.SubscriptionTotalDays.Value / 30 : 0;
        args["usernameColor"] = string.IsNullOrEmpty(subscription.Payload.UsernameColor) ? "#FFFFFF" : subscription.Payload.UsernameColor;
        args["alertId"] = subscription.Payload.Id ?? string.Empty;
        return args;
    }

    private static Dictionary<string, object> BuildGoalArgs(FanslyProcessedEvent processedEvent, ChatRoomGoal payload)
    {
        Dictionary<string, object> args = CreateBaseArgs(processedEvent, payload.ChatRoomId);
        int currentAmount = payload.CurrentAmount.HasValue ? payload.CurrentAmount.Value : 0;
        int goalAmount = payload.GoalAmount.HasValue ? payload.GoalAmount.Value : 0;
        args["goalId"] = payload.Id ?? string.Empty;
        args["accountId"] = payload.AccountId ?? string.Empty;
        args["label"] = payload.Label ?? string.Empty;
        args["description"] = payload.Description ?? string.Empty;
        args["currentAmount"] = currentAmount;
        args["currentAmountDollars"] = FormatBalanceAmount(currentAmount);
        args["goalAmount"] = goalAmount;
        args["goalAmountDollars"] = FormatBalanceAmount(goalAmount);
        args["status"] = payload.Status.HasValue ? payload.Status.Value : 0;
        args["version"] = payload.Version.HasValue ? payload.Version.Value : 0;
        args["deletedAt"] = payload.DeletedAt.HasValue ? payload.DeletedAt.Value : 0;
        args["isDeleted"] = payload.DeletedAt.HasValue && payload.DeletedAt.Value > 0;
        args["payloadJson"] = SerializeObject(payload);
        return args;
    }

    private static Dictionary<string, object> CreateBaseArgs(FanslyProcessedEvent processedEvent, string chatRoomId)
    {
        Dictionary<string, object> args = new Dictionary<string, object>();
        args["platform"] = "Fansly";
        args["eventType"] = processedEvent.EventType;
        args["timestamp"] = processedEvent.Timestamp.ToString("yyyy-MM-dd HH:mm:ss");
        args["rawJson"] = processedEvent.RawJson ?? string.Empty;
        args["chatRoomId"] = chatRoomId ?? string.Empty;
        return args;
    }

    private static string SerializeObject(object payload)
    {
        return payload == null ? string.Empty : JsonConvert.SerializeObject(payload);
    }

    private static string FormatTipAmount(int amount)
    {
        return FormatBalanceAmount(amount);
    }

    private static string FormatBalanceAmount(int amount)
    {
        return $"${((float)amount / 1000):F2}";
    }

    private static string GetModerationTypeName(int type)
    {
        switch (type)
        {
            case 1:
                return "ban";
            case 2:
                return "timeout";
            case 3:
                return "unban";
            default:
                return "unknown";
        }
    }
}

public class FanslyTriggerDispatch
{
    public FanslyTriggerDispatch(string triggerCode, string eventType, Dictionary<string, object> args)
    {
        TriggerCode = triggerCode;
        EventType = eventType;
        Args = args ?? new Dictionary<string, object>();
    }

    public string TriggerCode { get; private set; }
    public string EventType { get; private set; }
    public Dictionary<string, object> Args { get; private set; }
}

public abstract class FanslyProcessedEvent
{
    protected FanslyProcessedEvent(string eventType, string rawJson, DateTimeOffset timestamp)
    {
        EventType = eventType;
        RawJson = rawJson ?? string.Empty;
        Timestamp = timestamp;
    }

    public string EventType { get; private set; }
    public string RawJson { get; private set; }
    public DateTimeOffset Timestamp { get; private set; }
}

public class FanslyChatMessageResult : FanslyProcessedEvent
{
    public FanslyChatMessageResult(string rawJson, DateTimeOffset timestamp, ChatRoomMessage message, ChatRoomMessageMetadata metadata, TipAttachmentMetadata tip)
        : base("message", rawJson, timestamp)
    {
        Message = message;
        Metadata = metadata ?? new ChatRoomMessageMetadata();
        Tip = tip;
    }

    public ChatRoomMessage Message { get; private set; }
    public ChatRoomMessageMetadata Metadata { get; private set; }
    public TipAttachmentMetadata Tip { get; private set; }
}

public class FanslyTipMessageResult : FanslyProcessedEvent
{
    public FanslyTipMessageResult(string rawJson, DateTimeOffset timestamp, ChatRoomMessage message, ChatRoomMessageMetadata metadata, TipAttachmentMetadata tip)
        : base("tip", rawJson, timestamp)
    {
        Message = message;
        Metadata = metadata ?? new ChatRoomMessageMetadata();
        Tip = tip ?? new TipAttachmentMetadata();
    }

    public ChatRoomMessage Message { get; private set; }
    public ChatRoomMessageMetadata Metadata { get; private set; }
    public TipAttachmentMetadata Tip { get; private set; }
}

public class FanslySubscriptionResult : FanslyProcessedEvent
{
    public FanslySubscriptionResult(string rawJson, DateTimeOffset timestamp, SubAlert payload)
        : base("subscription", rawJson, timestamp)
    {
        Payload = payload;
    }

    public SubAlert Payload { get; private set; }
}

public class FanslySessionVerifiedResult : FanslyProcessedEvent
{
    public FanslySessionVerifiedResult(string rawJson, DateTimeOffset timestamp, SessionVerifiedEvent payload)
        : base("sessionVerified", rawJson, timestamp)
    {
        Payload = payload;
    }

    public SessionVerifiedEvent Payload { get; private set; }
}

public class FanslyErrorResult : FanslyProcessedEvent
{
    public FanslyErrorResult(string rawJson, DateTimeOffset timestamp, ErrorEvent payload)
        : base("error", rawJson, timestamp)
    {
        Payload = payload;
    }

    public ErrorEvent Payload { get; private set; }
}

public class FanslyPingResponseResult : FanslyProcessedEvent
{
    public FanslyPingResponseResult(string rawJson, DateTimeOffset timestamp, PingResponseEvent payload)
        : base("pingResponse", rawJson, timestamp)
    {
        Payload = payload;
    }

    public PingResponseEvent Payload { get; private set; }
}

public class FanslyChatRoomUpdatedResult : FanslyProcessedEvent
{
    public FanslyChatRoomUpdatedResult(string rawJson, DateTimeOffset timestamp, ChatRoom payload)
        : base("chatRoomUpdated", rawJson, timestamp)
    {
        Payload = payload;
    }

    public ChatRoom Payload { get; private set; }
}

public class FanslyMessagesRemovedResult : FanslyProcessedEvent
{
    public FanslyMessagesRemovedResult(string rawJson, DateTimeOffset timestamp, ChatRoomBan payload)
        : base("messagesRemoved", rawJson, timestamp)
    {
        Payload = payload;
    }

    public ChatRoomBan Payload { get; private set; }
}

public class FanslyModerationResult : FanslyProcessedEvent
{
    public FanslyModerationResult(string rawJson, DateTimeOffset timestamp, ChatRoomBan payload)
        : base("moderation", rawJson, timestamp)
    {
        Payload = payload;
    }

    public ChatRoomBan Payload { get; private set; }
}

public class FanslyGoalCreatedResult : FanslyProcessedEvent
{
    public FanslyGoalCreatedResult(string rawJson, DateTimeOffset timestamp, ChatRoomGoal payload)
        : base("goalCreated", rawJson, timestamp)
    {
        Payload = payload;
    }

    public ChatRoomGoal Payload { get; private set; }
}

public class FanslyGoalUpdatedResult : FanslyProcessedEvent
{
    public FanslyGoalUpdatedResult(string rawJson, DateTimeOffset timestamp, ChatRoomGoal payload)
        : base("goalUpdated", rawJson, timestamp)
    {
        Payload = payload;
    }

    public ChatRoomGoal Payload { get; private set; }
}

public class FanslySettingsUpdatedResult : FanslyProcessedEvent
{
    public FanslySettingsUpdatedResult(string rawJson, DateTimeOffset timestamp, ChatRoomSettings payload)
        : base("settingsUpdated", rawJson, timestamp)
    {
        Payload = payload;
    }

    public ChatRoomSettings Payload { get; private set; }
}

public class ErrorEvent
{
    [JsonProperty("code")]
    public int Code { get; set; }

    [JsonProperty("message")]
    public string Message { get; set; }

    [JsonExtensionData]
    public IDictionary<string, JToken> AdditionalData { get; set; }
}

public class SessionVerifiedEvent
{
    [JsonProperty("v")]
    public int? Version { get; set; }

    [JsonExtensionData]
    public IDictionary<string, JToken> AdditionalData { get; set; }
}

public class PingResponseEvent
{
    [JsonExtensionData]
    public IDictionary<string, JToken> AdditionalData { get; set; }
}

public class FanslyServiceEvent
{
    [JsonProperty("serviceId")]
    public int ServiceId { get; set; }

    [JsonProperty("event")]
    public JToken Event { get; set; }

    [JsonExtensionData]
    public IDictionary<string, JToken> AdditionalData { get; set; }
}

public class ChatRoomServiceEventEnvelope
{
    [JsonProperty("type")]
    public int Type { get; set; }

    [JsonProperty("chatRoom")]
    public ChatRoom ChatRoom { get; set; }

    [JsonProperty("chatRoomMessage")]
    public ChatRoomMessage ChatRoomMessage { get; set; }

    [JsonProperty("chatRoomBan")]
    public ChatRoomBan ChatRoomBan { get; set; }

    [JsonProperty("chatRoomGoal")]
    public ChatRoomGoal ChatRoomGoal { get; set; }

    [JsonProperty("subAlert")]
    public SubAlert SubAlert { get; set; }

    [JsonProperty("settings")]
    public ChatRoomSettings Settings { get; set; }

    [JsonExtensionData]
    public IDictionary<string, JToken> AdditionalData { get; set; }
}

public class ChatRoom
{
    [JsonProperty("id")]
    public string Id { get; set; }

    [JsonProperty("accountId")]
    public string AccountId { get; set; }

    [JsonProperty("flags")]
    public int? Flags { get; set; }

    [JsonExtensionData]
    public IDictionary<string, JToken> AdditionalData { get; set; }
}

public class ChatRoomMessage
{
    [JsonProperty("chatRoomId")]
    public string ChatRoomId { get; set; }

    [JsonProperty("senderId")]
    public string SenderId { get; set; }

    [JsonProperty("content")]
    public string Content { get; set; }

    [JsonProperty("type")]
    public int Type { get; set; }

    [JsonProperty("private")]
    public int Private { get; set; }

    [JsonProperty("attachments")]
    public List<ChatRoomAttachment> Attachments { get; set; }

    [JsonProperty("accountFlags")]
    public int AccountFlags { get; set; }

    [JsonProperty("metadata")]
    public string Metadata { get; set; }

    [JsonProperty("chatRoomAccountId")]
    public string ChatRoomAccountId { get; set; }

    [JsonProperty("id")]
    public string Id { get; set; }

    [JsonProperty("createdAt")]
    public long CreatedAt { get; set; }

    [JsonProperty("embeds")]
    public List<JToken> Embeds { get; set; }

    [JsonProperty("usernameColor")]
    public string UsernameColor { get; set; }

    [JsonProperty("username")]
    public string Username { get; set; }

    [JsonProperty("displayname")]
    public string DisplayName { get; set; }

    [JsonProperty("messageTip")]
    public MessageTip MessageTip { get; set; }

    [JsonExtensionData]
    public IDictionary<string, JToken> AdditionalData { get; set; }
}

public class ChatRoomAttachment
{
    [JsonProperty("contentType")]
    public int ContentType { get; set; }

    [JsonProperty("contentId")]
    public string ContentId { get; set; }

    [JsonProperty("metadata")]
    public string Metadata { get; set; }

    [JsonProperty("chatRoomMessageId")]
    public string ChatRoomMessageId { get; set; }

    [JsonProperty("messageId")]
    public string MessageId { get; set; }

    [JsonProperty("pos")]
    public int? Position { get; set; }

    [JsonExtensionData]
    public IDictionary<string, JToken> AdditionalData { get; set; }
}

public class MessageTip
{
    [JsonProperty("receiverId")]
    public string ReceiverId { get; set; }

    [JsonProperty("amount")]
    public int Amount { get; set; }

    [JsonProperty("message")]
    public string Message { get; set; }

    [JsonProperty("customTargetId")]
    public string CustomTargetId { get; set; }

    [JsonExtensionData]
    public IDictionary<string, JToken> AdditionalData { get; set; }
}

public class ChatRoomMessageMetadata
{
    [JsonProperty("senderIsCreator")]
    public bool SenderIsCreator { get; set; }

    [JsonProperty("senderIsStaff")]
    public bool SenderIsStaff { get; set; }

    [JsonProperty("senderIsFollowing")]
    public bool SenderIsFollowing { get; set; }

    [JsonProperty("senderSubscription")]
    public SenderSubscription SenderSubscription { get; set; }

    [JsonExtensionData]
    public IDictionary<string, JToken> AdditionalData { get; set; }
}

public class SenderSubscription
{
    [JsonProperty("tierId")]
    public string TierId { get; set; }

    [JsonProperty("tierColor")]
    public string TierColor { get; set; }

    [JsonProperty("tierName")]
    public string TierName { get; set; }

    [JsonExtensionData]
    public IDictionary<string, JToken> AdditionalData { get; set; }
}

public class TipAttachmentMetadata
{
    [JsonProperty("amount")]
    public int Amount { get; set; }

    [JsonExtensionData]
    public IDictionary<string, JToken> AdditionalData { get; set; }
}

public class SubAlert
{
    [JsonProperty("chatRoomId")]
    public string ChatRoomId { get; set; }

    [JsonProperty("senderId")]
    public string SenderId { get; set; }

    [JsonProperty("historyId")]
    public string HistoryId { get; set; }

    [JsonProperty("subscriberId")]
    public string SubscriberId { get; set; }

    [JsonProperty("subscriptionTierId")]
    public string SubscriptionTierId { get; set; }

    [JsonProperty("subscriptionTierName")]
    public string SubscriptionTierName { get; set; }

    [JsonProperty("subscriptionTierColor")]
    public string SubscriptionTierColor { get; set; }

    [JsonProperty("subscriptionStreak")]
    public int? SubscriptionStreak { get; set; }

    [JsonProperty("subscriptionTotalDays")]
    public int? SubscriptionTotalDays { get; set; }

    [JsonProperty("id")]
    public string Id { get; set; }

    [JsonProperty("usernameColor")]
    public string UsernameColor { get; set; }

    [JsonProperty("username")]
    public string Username { get; set; }

    [JsonProperty("displayname")]
    public string DisplayName { get; set; }

    [JsonExtensionData]
    public IDictionary<string, JToken> AdditionalData { get; set; }
}

public class ChatRoomBan
{
    [JsonProperty("id")]
    public string Id { get; set; }

    [JsonProperty("chatRoomId")]
    public string ChatRoomId { get; set; }

    [JsonProperty("accountId")]
    public string AccountId { get; set; }

    [JsonProperty("type")]
    public int Type { get; set; }

    [JsonProperty("duration")]
    public long? Duration { get; set; }

    [JsonExtensionData]
    public IDictionary<string, JToken> AdditionalData { get; set; }
}

public class ChatRoomGoal
{
    [JsonProperty("id")]
    public string Id { get; set; }

    [JsonProperty("chatRoomId")]
    public string ChatRoomId { get; set; }

    [JsonProperty("accountId")]
    public string AccountId { get; set; }

    [JsonProperty("status")]
    public int? Status { get; set; }

    [JsonProperty("currentAmount")]
    public int? CurrentAmount { get; set; }

    [JsonProperty("goalAmount")]
    public int? GoalAmount { get; set; }

    [JsonProperty("label")]
    public string Label { get; set; }

    [JsonProperty("description")]
    public string Description { get; set; }

    [JsonProperty("version")]
    public int? Version { get; set; }

    [JsonProperty("deletedAt")]
    public long? DeletedAt { get; set; }

    [JsonExtensionData]
    public IDictionary<string, JToken> AdditionalData { get; set; }
}

public class ChatRoomSettings
{
    [JsonProperty("chatRoomId")]
    public string ChatRoomId { get; set; }

    [JsonProperty("slowmode")]
    public long? Slowmode { get; set; }

    [JsonExtensionData]
    public IDictionary<string, JToken> AdditionalData { get; set; }
}
