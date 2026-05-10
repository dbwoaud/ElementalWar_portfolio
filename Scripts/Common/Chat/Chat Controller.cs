using UnityEngine;

public class ChatController : MonoBehaviour
{
    [Header("전송 컴포넌트")]
    [SerializeField] private MonoBehaviour transportComponent;

    [Header("채팅 UI")]
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
            transport.OnMessageReceived += HandleMessageReceived;
            transport.OnSystemMessage += HandleSystemMessage;
        }
        if (view != null)
            view.OnSendMessageRequest += HandleSendMessageRequest;
    }

    private void OnDisable()
    {
        if (transport != null)
        {
            transport.OnMessageReceived -= HandleMessageReceived;
            transport.OnSystemMessage -= HandleSystemMessage;
        }
        if (view != null)
            view.OnSendMessageRequest -= HandleSendMessageRequest;
    }

    private void Start()
    {
        transport?.Connect();
    }

    private void HandleSendMessageRequest(string message) // 입력창에서 송신 요청이 들어왔을 때 실행되는 함수
    {
        transport?.Send(message);
    }

    private void HandleMessageReceived(string sender, string message) // 메시지 수신 시 실행되는 함수
    {
        if (view == null)
            return;

        bool isMine = (sender == Photon.Pun.PhotonNetwork.LocalPlayer.NickName);
        string formatted = ChatMessageFormatter.FormatPlayerMessage(sender, message, isMine);
        view.AppendMessage(formatted);
    }

    private void HandleSystemMessage(string message) // 시스템 메시지 수신 시 실행되는 함수
    {
        if (view == null)
            return;

        string formatted = ChatMessageFormatter.FormatSystemMessage(message);
        view.AppendMessage(formatted);
    }
}