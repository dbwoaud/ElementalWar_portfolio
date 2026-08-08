using System;
using UnityEngine;
using UnityEngine.UI;

public class ErrorPopupPanel : BasePopupPanel
{
    [Header("UI 요소")]
    [SerializeField] private Button closeButton;

    private Action onCloseAction; // 닫기 버튼 클릭 이벤트


    protected override void RegisterListener() // UI 리스너를 등록하는 함수
    {
        closeButton?.onClick.AddListener(OnClickCloseButton);
    }

    protected override void UnregisterListener() // UI 리스너를 해제하는 함수
    {
        closeButton?.onClick.RemoveListener(OnClickCloseButton);
    }

    protected override void ResetUI() // UI를 리셋하는 함수
    {
        messageText.text = "";
    }

    private void OnClickCloseButton() // 닫기 버튼 클릭 시 실행되는 함수
    {
        SoundManager.Instance?.Play(SoundKey.ButtonClick);
        onCloseAction?.Invoke();
        Hide();
    }

    public void Setup(string message, Action onClose = null) // 에러 팝업 패널을 설정하는 함수
    {
        onCloseAction = onClose;
        ShowPopup(message);
    }
}
