using UnityEngine;
using Photon.Pun;

public class ChatController : MonoBehaviour
{
    [Header("컴포넌트")]
    [SerializeField] private MonoBehaviour transportComponent;
    [SerializeField] private ChatPanelUI viewComponent;

    private IChatTransport transport;
    private IChatView view;


    private void Awake()
    {
        transport = transportComponent as IChatTransport;
        view = viewComponent;
    }

    private void OnEnable()
    {
        if (transport != null)
        {
            transport.OnPlayerMessageReceived += HandlePlayerMessageReceived;
            transport.OnSystemMessageReceived += HandleSystemMessageReceived;
        }
        if (view != null)
            view.OnSendMessageRequest += HandleSendMessageRequest;
    }

    private void OnDisable()
    {
        if (transport != null)
        {
            transport.OnPlayerMessageReceived -= HandlePlayerMessageReceived;
            transport.OnSystemMessageReceived -= HandleSystemMessageReceived;
        }
        if (view != null)
            view.OnSendMessageRequest -= HandleSendMessageRequest;
    }

    private void Start()
    {
        transport?.Connect();
    }

    private void HandleSendMessageRequest(string message) // 메시지 전송 요청을 처리하는 함수
    {
        transport?.Send(message);
    }

    private void HandlePlayerMessageReceived(string sender, string message) // 플레이어 메시지 수신을 처리하는 함수
    {
        if (view == null)
            return;

        bool isMine = (sender == PhotonNetwork.LocalPlayer.NickName);
        string formattedMessage = ChatMessageFormatter.GetFormattedPlayerMessage(sender, message, isMine);
        view.AppendMessage(formattedMessage);
    }

    private void HandleSystemMessageReceived(string message) // 시스템 메시지 수신을 처리하는 함수
    {
        if (view == null)
            return;

        string formattedMessage = ChatMessageFormatter.GetFormattedSystemMessage(message);
        view.AppendMessage(formattedMessage);
    }
}