using System;
using UnityEngine;
using UnityEngine.UI;

public class ErrorPopupPanel : BasePopupPanel
{
    [Header("UI 요소")]
    [SerializeField] private Button closeButton;

    [Header("버튼 클릭 이벤트")]
    private Action onCloseAction;


    protected override void InitializeListener()
    {
        closeButton?.onClick.AddListener(OnClickCloseButton);
    }

    protected override void UnregisterListener()
    {
        closeButton?.onClick.RemoveListener(OnClickCloseButton);
    }

    protected override void ResetUI()
    {
        messageText.text = "";
    }

    private void OnClickCloseButton() // 닫기 버튼 클릭 시 실행되는 함수
    {
        SoundManager.instance?.Play(SoundKey.ButtonClick);
        onCloseAction?.Invoke();
        Hide();
    }

    public void Setup(string message, Action onClose = null) // 에러 팝업 패널을 활성화하는 함수
    {
        onCloseAction = onClose;
        ShowPopup(message);
    }
}
