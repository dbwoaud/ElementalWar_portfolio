using ExitGames.Client.Photon;
using Photon.Chat;
using Photon.Pun;
using System;
using UnityEngine;

public class LobbyChatTransport : MonoBehaviour, IChatTransport, IChatClientListener
{
    [Header("채팅 설정")]
    [SerializeField] private string chatRegion = "kr";
    private ChatClient chatClient;

    public event Action<string, string> OnMessageReceived;
    public event Action<string> OnSystemMessage;


    private void Update()
    {
        chatClient?.Service();
    }

    private void OnDestroy()
    {
        Disconnect();
    }

    public void Connect() // 포톤 채팅 서버에 연결하는 함수
    {
        chatClient = new ChatClient(this) { ChatRegion = chatRegion };
        chatClient.Connect
        (
            PhotonNetwork.PhotonServerSettings.AppSettings.AppIdChat,
            "1.0",
            new AuthenticationValues(PhotonNetwork.LocalPlayer.NickName)
        );
    }

    public void Disconnect() // 포톤 채팅 서버 연결을 해제하는 함수
    {
        chatClient?.Disconnect();
    }

    public void Send(string message) // 메시지를 글로벌 로비 채널에 송신하는 함수
    {
        if (chatClient != null && chatClient.CanChat)
            chatClient.PublishMessage(ChattingSystem.Lobby.ChannelName, message);
    }

    #region IChatClientListener Callbacks

    public void OnConnected() // 채팅 서버 접속 시 실행되는 함수
    {
        chatClient.Subscribe(new[] { ChattingSystem.Lobby.ChannelName });
    }

    public void OnGetMessages(string channelName, string[] senders, object[] messages) // 메시지 수신 시 실행되는 함수
    {
        for (int i = 0; i < senders.Length; i++)
        {
            string sender = senders[i];
            string message = messages[i].ToString();
            OnMessageReceived?.Invoke(sender, message);
        }
    }

    public void OnSubscribed(string[] channels, bool[] results) { }
    public void DebugReturn(DebugLevel level, string message) { }
    public void OnDisconnected() { }
    public void OnChatStateChange(ChatState state) { }
    public void OnPrivateMessage(string sender, object message, string channelName) { }
    public void OnUnsubscribed(string[] channels) { }
    public void OnStatusUpdate(string user, int status, bool gotMessage, object message) { }
    public void OnUserSubscribed(string channel, string user) { }
    public void OnUserUnsubscribed(string channel, string user) { }
    #endregion
}
