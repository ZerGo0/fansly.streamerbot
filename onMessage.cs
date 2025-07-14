using System;
using System.Collections.Generic;
using Newtonsoft.Json;

public class CPHInline
{
    private const int MESSAGE_TYPE_EVENT = 10000;
    private const int BULK_TYPE_EVENT = 10001;
    private const int EVENT_TYPE_CHAT = 10;
    private const int EVENT_TYPE_SUBSCRIPTION = 53;
    private const int ATTACHMENT_TYPE_TIP = 7;

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
            ProcessMessage(message);
            return true;
        }
        catch (Exception ex)
        {
            CPH.LogError($"[Fansly] Error processing message: {ex.Message}");
            return false;
        }
    }
    
    private void ProcessMessage(string message)
    {
        var outerMessage = DeserializeJson<Dictionary<string, object>>(message);
        if (outerMessage == null || !outerMessage.ContainsKey("t"))
        {
            CPH.LogDebug("[Fansly] Invalid message format - missing 't' field");
            return;
        }
        
        int messageType = Convert.ToInt32(outerMessage["t"]);
        switch (messageType)
        {
            case MESSAGE_TYPE_EVENT:
                if (!outerMessage.ContainsKey("d"))
                {
                    CPH.LogDebug("[Fansly] Event message missing 'd' field");
                    return;
                }

                ProcessEventMessage(outerMessage["d"].ToString());
                break;
            case BULK_TYPE_EVENT:
                if (!outerMessage.ContainsKey("d"))
                {
                    CPH.LogDebug("[Fansly] Bulk event message missing 'd' field");
                    return;
                }

                var bulkData = DeserializeJson<List<Dictionary<string, object>>>(outerMessage["d"].ToString());
                foreach (var message in bulkData)
                {
                    ProcessEventMessage(JsonConvert.SerializeObject(message));
                }
                break;


            default:
                CPH.LogDebug($"[Fansly] Unknown message type: {messageType}");
                break;
        }
    }
    
    private void ProcessEventMessage(string dataStr)
    {
        var data = DeserializeJson<Dictionary<string, object>>(dataStr);
        if (data == null || !data.ContainsKey("event"))
        {
            CPH.LogDebug("[Fansly] Invalid data format - missing 'event' field");
            return;
        }
        
        var eventData = DeserializeJson<Dictionary<string, object>>(data["event"].ToString());
        if (eventData == null || !eventData.ContainsKey("type"))
        {
            CPH.LogDebug("[Fansly] Invalid event format - missing 'type' field");
            return;
        }
        
        int eventType = Convert.ToInt32(eventData["type"]);
        CPH.LogInfo($"[Fansly] Processing event type: {eventType}");
        
        switch (eventType)
        {
            case EVENT_TYPE_CHAT:
                ProcessChatEvent(eventData);
                break;
                
            case EVENT_TYPE_SUBSCRIPTION:
                ProcessSubscriptionEvent(eventData);
                break;
                
            default:
                CPH.LogDebug($"[Fansly] Unknown event type: {eventType}");
                break;
        }
    }
    
    private void ProcessChatEvent(Dictionary<string, object> eventData)
    {
        if (!eventData.ContainsKey("chatRoomMessage"))
        {
            CPH.LogDebug("[Fansly] Chat event missing 'chatRoomMessage' field");
            return;
        }
        
        string chatMessageJson = JsonConvert.SerializeObject(eventData["chatRoomMessage"]);
        ProcessChatMessage(chatMessageJson);
    }
    
    private void ProcessSubscriptionEvent(Dictionary<string, object> eventData)
    {
        if (!eventData.ContainsKey("subAlert"))
        {
            CPH.LogDebug("[Fansly] Subscription event missing 'subAlert' field");
            return;
        }
        
        string subAlertJson = JsonConvert.SerializeObject(eventData["subAlert"]);
        ProcessSubscription(subAlertJson);
    }
    
    private void ProcessChatMessage(string chatMessageJson)
    {
        if (string.IsNullOrEmpty(chatMessageJson)) return;
        
        try
        {
            var chatMessage = DeserializeJson<Dictionary<string, object>>(chatMessageJson);
            if (chatMessage == null) return;
            
            var messageData = ExtractChatMessageData(chatMessage);
            var metadata = ExtractMetadata(chatMessage);
            var tipInfo = ExtractTipInfo(chatMessage);
            
            var triggerArgs = BuildChatTriggerArgs(messageData, metadata, tipInfo);
            
            if (tipInfo.HasTip)
            {
                CPH.TriggerCodeEvent("fanslyTip", triggerArgs);
                CPH.LogInfo($"[Fansly] Tip - {messageData.Username}: ${tipInfo.Amount:F2} - {messageData.Content}");
            }
            
            CPH.TriggerCodeEvent("fanslyMessage", triggerArgs);
            CPH.LogInfo($"[Fansly] Chat - {messageData.Username}: {messageData.Content}");
        }
        catch (Exception ex)
        {
            CPH.LogError($"[Fansly] Error processing chat message: {ex.Message}");
        }
    }
    
    private void ProcessSubscription(string subAlertJson)
    {
        if (string.IsNullOrEmpty(subAlertJson)) return;
        
        try
        {
            var subAlert = DeserializeJson<Dictionary<string, object>>(subAlertJson);
            if (subAlert == null) return;
            
            var subData = ExtractSubscriptionData(subAlert);
            var triggerArgs = BuildSubscriptionTriggerArgs(subData);
            
            CPH.TriggerCodeEvent("fanslySubscription", triggerArgs);
            CPH.LogInfo($"[Fansly] Subscription - {subData.Username}: {subData.TierName} (Streak: {subData.Streak} days, Total: {subData.TotalDays} days)");
        }
        catch (Exception ex)
        {
            CPH.LogError($"[Fansly] Error processing subscription: {ex.Message}");
        }
    }
    
    private ChatMessageData ExtractChatMessageData(Dictionary<string, object> chatMessage)
    {
        return new ChatMessageData
        {
            Username = GetStringValue(chatMessage, "username", "Unknown"),
            DisplayName = GetStringValue(chatMessage, "displayname", GetStringValue(chatMessage, "username", "Unknown")),
            Content = GetStringValue(chatMessage, "content"),
            UserId = GetStringValue(chatMessage, "senderId"),
            UsernameColor = GetStringValue(chatMessage, "usernameColor", "#FFFFFF"),
            ChatRoomId = GetStringValue(chatMessage, "chatRoomId"),
            MessageId = GetStringValue(chatMessage, "id"),
            CreatedAt = GetLongValue(chatMessage, "createdAt")
        };
    }
    
    private MetadataInfo ExtractMetadata(Dictionary<string, object> chatMessage)
    {
        var info = new MetadataInfo();
        
        if (!chatMessage.ContainsKey("metadata")) return info;
        
        string metadataStr = chatMessage["metadata"].ToString();
        if (string.IsNullOrEmpty(metadataStr)) return info;
        
        try
        {
            var metadata = DeserializeJson<Dictionary<string, object>>(metadataStr);
            if (metadata == null) return info;
            
            info.IsCreator = GetBoolValue(metadata, "senderIsCreator");
            info.IsStaff = GetBoolValue(metadata, "senderIsStaff");
            info.IsFollowing = GetBoolValue(metadata, "senderIsFollowing");
            
            if (metadata.ContainsKey("senderSubscription"))
            {
                var subscription = DeserializeJson<Dictionary<string, object>>(metadata["senderSubscription"].ToString());
                if (subscription != null)
                {
                    info.TierName = GetStringValue(subscription, "tierName");
                    info.TierColor = GetStringValue(subscription, "tierColor");
                    info.TierId = GetStringValue(subscription, "tierId");
                }
            }
        }
        catch (Exception ex)
        {
            CPH.LogError($"[Fansly] Error parsing metadata: {ex.Message}");
        }
        
        return info;
    }
    
    private TipInfo ExtractTipInfo(Dictionary<string, object> chatMessage)
    {
        var info = new TipInfo();
        
        if (!chatMessage.ContainsKey("attachments")) return info;
        
        var attachmentsJson = chatMessage["attachments"].ToString();
        var attachments = DeserializeJson<List<Dictionary<string, object>>>(attachmentsJson);
        
        if (attachments == null) return info;
        
        foreach (var attachment in attachments)
        {
            if (!attachment.ContainsKey("contentType")) continue;
            
            int contentType = Convert.ToInt32(attachment["contentType"]);
            if (contentType != ATTACHMENT_TYPE_TIP) continue;
            
            info.HasTip = true;
            
            if (!attachment.ContainsKey("metadata")) continue;
            
            string attachMetadata = attachment["metadata"].ToString();
            if (string.IsNullOrEmpty(attachMetadata)) continue;
            
            try
            {
                var tipData = DeserializeJson<Dictionary<string, object>>(attachMetadata);
                if (tipData != null && tipData.ContainsKey("amount"))
                {
                    info.Amount = Convert.ToInt32(tipData["amount"]);
                    info.AmountDollars = $"${(info.Amount / 1000):F2}";
                }
            }
            catch (Exception ex)
            {
                CPH.LogError($"[Fansly] Error parsing tip metadata: {ex.Message}");
            }
            
            break; // Only process first tip attachment
        }
        
        return info;
    }
    
    private SubscriptionData ExtractSubscriptionData(Dictionary<string, object> subAlert)
    {
        return new SubscriptionData
        {
            Username = GetStringValue(subAlert, "username", "Unknown"),
            DisplayName = GetStringValue(subAlert, "displayname", GetStringValue(subAlert, "username", "Unknown")),
            UserId = GetStringValue(subAlert, "subscriberId"),
            TierName = GetStringValue(subAlert, "subscriptionTierName"),
            TierColor = GetStringValue(subAlert, "subscriptionTierColor"),
            TierId = GetStringValue(subAlert, "subscriptionTierId"),
            Streak = GetIntValue(subAlert, "subscriptionStreak", 1),
            TotalDays = GetIntValue(subAlert, "subscriptionTotalDays", 0),
            UsernameColor = GetStringValue(subAlert, "usernameColor", "#FFFFFF"),
            ChatRoomId = GetStringValue(subAlert, "chatRoomId"),
            AlertId = GetStringValue(subAlert, "id")
        };
    }
    
    private Dictionary<string, object> BuildChatTriggerArgs(ChatMessageData data, MetadataInfo metadata, TipInfo tipInfo)
    {
        var args = new Dictionary<string, object>
        {
            ["user"] = data.Username,
            ["userName"] = data.Username,
            ["displayName"] = data.DisplayName,
            ["userId"] = data.UserId,
            ["message"] = data.Content,
            ["messageId"] = data.MessageId,
            ["chatRoomId"] = data.ChatRoomId,
            ["usernameColor"] = data.UsernameColor,
            ["isCreator"] = metadata.IsCreator,
            ["isStaff"] = metadata.IsStaff,
            ["isFollowing"] = metadata.IsFollowing,
            ["isSubscriber"] = !string.IsNullOrEmpty(metadata.TierName),
            ["subscriberTier"] = metadata.TierName,
            ["subscriberTierColor"] = metadata.TierColor,
            ["subscriberTierId"] = metadata.TierId,
            ["platform"] = "Fansly",
            ["timestamp"] = DateTimeOffset.FromUnixTimeMilliseconds(data.CreatedAt).ToString("yyyy-MM-dd HH:mm:ss")
        };
        
        if (tipInfo.HasTip)
        {
            args["tipAmount"] = tipInfo.Amount;
            args["tipAmountDollars"] = tipInfo.AmountDollars;
        }
        
        return args;
    }
    
    private Dictionary<string, object> BuildSubscriptionTriggerArgs(SubscriptionData data)
    {
        return new Dictionary<string, object>
        {
            ["user"] = data.Username,
            ["userName"] = data.Username,
            ["displayName"] = data.DisplayName,
            ["userId"] = data.UserId,
            ["tier"] = data.TierName,
            ["tierName"] = data.TierName,
            ["tierColor"] = data.TierColor,
            ["tierId"] = data.TierId,
            ["streak"] = data.Streak,
            ["totalDays"] = data.TotalDays,
            ["months"] = data.TotalDays / 30,
            ["usernameColor"] = data.UsernameColor,
            ["chatRoomId"] = data.ChatRoomId,
            ["alertId"] = data.AlertId,
            ["platform"] = "Fansly",
            ["timestamp"] = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")
        };
    }
    
    // Helper methods
    private T DeserializeJson<T>(string json) where T : class
    {
        try
        {
            return JsonConvert.DeserializeObject<T>(json);
        }
        catch
        {
            return null;
        }
    }
    
    private string GetStringValue(Dictionary<string, object> dict, string key, string defaultValue = "")
    {
        return dict.ContainsKey(key) ? dict[key].ToString() : defaultValue;
    }
    
    private long GetLongValue(Dictionary<string, object> dict, string key, long defaultValue = 0)
    {
        return dict.ContainsKey(key) ? Convert.ToInt64(dict[key]) : defaultValue;
    }
    
    private int GetIntValue(Dictionary<string, object> dict, string key, int defaultValue = 0)
    {
        return dict.ContainsKey(key) ? Convert.ToInt32(dict[key]) : defaultValue;
    }
    
    private bool GetBoolValue(Dictionary<string, object> dict, string key, bool defaultValue = false)
    {
        return dict.ContainsKey(key) ? Convert.ToBoolean(dict[key]) : defaultValue;
    }
    
    // Data classes
    private class ChatMessageData
    {
        public string Username { get; set; }
        public string DisplayName { get; set; }
        public string Content { get; set; }
        public string UserId { get; set; }
        public string UsernameColor { get; set; }
        public string ChatRoomId { get; set; }
        public string MessageId { get; set; }
        public long CreatedAt { get; set; }
    }
    
    private class MetadataInfo
    {
        public bool IsCreator { get; set; }
        public bool IsStaff { get; set; }
        public bool IsFollowing { get; set; }
        public string TierName { get; set; } = "";
        public string TierColor { get; set; } = "";
        public string TierId { get; set; } = "";
    }
    
    private class TipInfo
    {
        public bool HasTip { get; set; }
        public int Amount { get; set; }
        public string AmountDollars { get; set; }
    }
    
    private class SubscriptionData
    {
        public string Username { get; set; }
        public string DisplayName { get; set; }
        public string UserId { get; set; }
        public string TierName { get; set; }
        public string TierColor { get; set; }
        public string TierId { get; set; }
        public int Streak { get; set; }
        public int TotalDays { get; set; }
        public string UsernameColor { get; set; }
        public string ChatRoomId { get; set; }
        public string AlertId { get; set; }
    }
}