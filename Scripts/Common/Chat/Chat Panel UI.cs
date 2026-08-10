using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class ChatPanelUI : MonoBehaviour, IChatView
{
    [Header("대화창 UI")]
    [SerializeField] private InputField chatInputField;
    [SerializeField] private ScrollRect chatView;
    [SerializeField] private Transform contentParent;
    [SerializeField] private GameObject chatTextPrefab;
    [SerializeField] private int maxMessageCount = 100;

    public event Action<string> OnSendMessageRequest; // 메시지 발신 요청 이벤트


    private void Start()
    {
        if (chatInputField != null)
            chatInputField.onSubmit.AddListener(HandleSendMessage);
    }

    private void OnDestroy()
    {
        if (chatInputField != null)
            chatInputField.onSubmit.RemoveListener(HandleSendMessage);
    }

    private void HandleSendMessage(string inputContent) // 메시지 전송을 처리하는 함수
    {
        if (string.IsNullOrWhiteSpace(inputContent))
            return;

        OnSendMessageRequest?.Invoke(inputContent);

        chatInputField.text = string.Empty;
        StartCoroutine(ResetInputField());
    }

    private IEnumerator ResetInputField() // 채팅 입력을 초기화하는 코루틴
    {
        yield return null;
        chatInputField.ActivateInputField();
        yield return null;
        chatInputField.text = string.Empty;
        chatInputField.MoveTextEnd(false);
    }

    public void AppendMessage(string formattedMessage) // 메시지를 추가하는 함수
    {
        if (chatTextPrefab == null || contentParent == null)
            return;

        RemoveOldMessages();

        GameObject newChatObj = Instantiate(chatTextPrefab, contentParent);
        if (newChatObj.TryGetComponent(out Text chatText))
            chatText.text = formattedMessage;

        UpdateScrollPosition();
    }

    private void RemoveOldMessages() // 오래된 메시지를 제거하는 함수
    {
        while (contentParent.childCount >= maxMessageCount)
            Destroy(contentParent.GetChild(0).gameObject);
    }

    private void UpdateScrollPosition() // 스크롤 위치를 업데이트하는 함수
    {
        Canvas.ForceUpdateCanvases();
        if (chatView != null)
            chatView.verticalNormalizedPosition = 0f;
    }
}