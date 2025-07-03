using System;
using System.Threading.Tasks;

public class CPHInline
{
    public void Init()
    {
        CPH.LogInfo("[Fansly] onOpen Init");

        // We need to register the custom triggers here because the onMessage.cs file is not loaded yet
        CPH.RegisterCustomTrigger("Fansly Message", "fanslyMessage", new[] {"Fansly"});
        CPH.RegisterCustomTrigger("Fansly Tip", "fanslyTip", new[] {"Fansly"});
        CPH.RegisterCustomTrigger("Fansly Subscription", "fanslySubscription", new[] {"Fansly"});
    }

    public bool Execute()
    {
        CPH.LogInfo("[Fansly] onOpen");

        string wsIdxStr = args["wsIdx"].ToString();
        if (string.IsNullOrEmpty(wsIdxStr))
        {
            CPH.LogError("[Fansly] wsIdx not found");
            return false;
        }

        int wsIdx = int.Parse(wsIdxStr);

        string chatRoomId = CPH.GetGlobalVar<string>("fanslyChatroomId", true);
        if (string.IsNullOrEmpty(chatRoomId))
        {
            CPH.LogWarn("[Fansly] chatRoomId not found");
            return false;
        }

        CPH.LogInfo($"[Fansly] Trying to join chatroom: chatRoomId: {chatRoomId} wsIdx: {wsIdx}");
        string joinChatRoomMessage = $"{{\"t\":46001,\"d\":\"{{\\\"chatRoomId\\\":\\\"{chatRoomId}\\\"}}\"}}";
        CPH.WebsocketSend(joinChatRoomMessage, wsIdx);

        CPH.LogInfo($"[Fansly] Joined chat room: {chatRoomId}");

        Task.Run(async () =>
        {
            while (true)
            {
                CPH.LogInfo("[Fansly] Pinging Fansly websocket");
                CPH.WebsocketSend("p", wsIdx);
                await Task.Delay(10000);
            }
        });

        return true;
    }
}