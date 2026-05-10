using System;
using UnityEngine;
using UnityEngine.UI;

public class ChatPanelUI : MonoBehaviour, IChatView
{
    [Header("대화창 UI")]
    [SerializeField] private InputField chatInputField;
    [SerializeField] private ScrollRect chatView;
    [SerializeField] private Transform contentParent;
    [SerializeField] private GameObject chatTextPrefab;

    public event Action<string> OnSendMessageRequest;


    private void Start()
    {
        if (chatInputField != null)
            chatInputField.onSubmit.AddListener(HandleInputSubmit);
    }

    private void OnDestroy()
    {
        if (chatInputField != null)
            chatInputField.onSubmit.RemoveListener(HandleInputSubmit);
    }

    private void HandleInputSubmit(string inputContent) // 채팅이 입력될 때 실행되는 함수
    {
        if (string.IsNullOrWhiteSpace(inputContent))
            return;

        OnSendMessageRequest?.Invoke(inputContent);
        chatInputField.text = string.Empty;
        chatInputField.ActivateInputField();
    }

    public void AppendMessage(string formattedMessage) // 메시지를 화면에 한 줄 추가하는 함수
    {
        if (chatTextPrefab == null || contentParent == null)
            return;

        GameObject newChatObj = Instantiate(chatTextPrefab, contentParent);
        Text chatText = newChatObj.GetComponent<Text>();
        if (chatText != null)
            chatText.text = formattedMessage;

        UpdateScrollPosition();
    }

    private void UpdateScrollPosition() // 스크롤을 가장 아래로 이동시키는 함수
    {
        Canvas.ForceUpdateCanvases();
        if (chatView != null)
            chatView.verticalNormalizedPosition = 0f;
    }
}