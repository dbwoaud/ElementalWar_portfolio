using System;
using UnityEngine;
using UnityEngine.UI;

public class ConfirmPopupPanel : BasePopupPanel
{
    [Header("UI 요소")]
    [SerializeField] private Button confirmButton;

    [Header("버튼 클릭 이벤트")]
    private Action onConfirmAction;


    protected override void InitializeListener()
    {
        confirmButton?.onClick.AddListener(OnClickConfirmButton);
    }

    protected override void UnregisterListener()
    {
        confirmButton?.onClick.RemoveListener(OnClickConfirmButton);
    }

    protected override void ResetUI()
    {
        messageText.text = "";
    }

    private void OnClickConfirmButton() // 확인 버튼 클릭 시 실행되는 함수
    {
        SoundManager.instance?.Play(SoundKey.ButtonClick);
        onConfirmAction?.Invoke();
        Hide();
    }

    public void Setup(string message, Action onConfirm = null) // 확인 팝업 패널을 활성화하는 함수
    {
        onConfirmAction = onConfirm;
        ShowPopup(message);
    }
}
