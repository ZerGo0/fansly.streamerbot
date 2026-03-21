using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using Newtonsoft.Json;

public class CPHInline
{
    private const string SendMessageEndpoint = "https://apiv3.fansly.com/api/v1/chatroom/message?ngsw-bypass=true";
    private const string AuthTokenGlobalVar = "fanslyAuthToken";
    private const string ChatRoomGlobalVar = "fanslyChatroomId";
    private const string FanslyClientId = "891628335845621760";
    private const string FanslyUserAgent = "Mozilla/5.0 (Macintosh; Intel Mac OS X 10_15_7) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/146.0.0.0 Safari/537.36";
    private const string FanslyOrigin = "https://fansly.com";
    private const string FanslyReferer = "https://fansly.com/";
    private const string FanslyAccept = "application/json, text/plain, */*";
    private const string FanslyAcceptLanguage = "en-US,en;q=0.9";

    private static readonly HttpClient HttpClient = new HttpClient
    {
        Timeout = TimeSpan.FromSeconds(30)
    };

    public void Init()
    {
        CPH.LogInfo("[Fansly] sendMessage Init");
    }

    public bool Execute()
    {
        return SendMessage();
    }

    public bool SendMessage()
    {
        string message = GetMessage();
        if (string.IsNullOrWhiteSpace(message))
        {
            CPH.LogError("[Fansly] message not found");
            return false;
        }

        string chatRoomId = GetChatRoomId();
        if (string.IsNullOrWhiteSpace(chatRoomId))
        {
            CPH.LogError("[Fansly] chatRoomId not found");
            return false;
        }

        string authToken = GetAuthToken();
        if (string.IsNullOrWhiteSpace(authToken))
        {
            CPH.LogError("[Fansly] auth token not found");
            return false;
        }

        try
        {
            var payload = new
            {
                chatRoomId,
                content = message
            };

            string payloadJson = JsonConvert.SerializeObject(payload);
            using (var request = new HttpRequestMessage(HttpMethod.Post, SendMessageEndpoint))
            {
                request.Content = new StringContent(payloadJson, Encoding.UTF8, "application/json");

                request.Headers.Clear();
                request.Headers.Add("accept", FanslyAccept);
                request.Headers.Add("accept-language", FanslyAcceptLanguage);
                request.Headers.Add("authorization", authToken);
                request.Headers.Add("origin", FanslyOrigin);
                request.Headers.Add("user-agent", FanslyUserAgent);
                request.Headers.Add("fansly-client-id", FanslyClientId);
                request.Headers.ConnectionClose = true;
                request.Headers.Referrer = new Uri(FanslyReferer);

                HttpResponseMessage response = HttpClient.SendAsync(request).GetAwaiter().GetResult();
                string responseBody = response.Content.ReadAsStringAsync().GetAwaiter().GetResult();

                if (!response.IsSuccessStatusCode)
                {
                    CPH.LogError($"[Fansly] Failed to send message: {(int)response.StatusCode} {response.ReasonPhrase} - {responseBody}");
                    return false;
                }

                var responseJson = DeserializeJson<Dictionary<string, object>>(responseBody);
                if (responseJson != null &&
                    responseJson.TryGetValue("success", out object successValue) &&
                    successValue is bool success &&
                    !success)
                {
                    CPH.LogError($"[Fansly] Fansly rejected message: {responseBody}");
                    return false;
                }
            }

            CPH.SetGlobalVar("fanslyLastSentMessage", message, false);
            CPH.LogInfo($"[Fansly] Sent message to chatroom {chatRoomId}: {message}");
            return true;
        }
        catch (Exception ex)
        {
            CPH.LogError($"[Fansly] Error sending message: {ex.Message}");
            return false;
        }
    }

    private string GetMessage()
    {
        if (CPH.TryGetArg("message", out string message) && !string.IsNullOrWhiteSpace(message))
        {
            return message.Trim();
        }

        if (CPH.TryGetArg("rawInput", out string rawInput) && !string.IsNullOrWhiteSpace(rawInput))
        {
            return rawInput.Trim();
        }

        return string.Empty;
    }

    private string GetChatRoomId()
    {
        if (CPH.TryGetArg("chatRoomId", out string chatRoomId) && !string.IsNullOrWhiteSpace(chatRoomId))
        {
            return chatRoomId.Trim();
        }

        return CPH.GetGlobalVar<string>(ChatRoomGlobalVar, true);
    }

    private string GetAuthToken()
    {
        if (CPH.TryGetArg("authToken", out string authToken) && !string.IsNullOrWhiteSpace(authToken))
        {
            return authToken.Trim();
        }

        return CPH.GetGlobalVar<string>(AuthTokenGlobalVar, true);
    }

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
}
