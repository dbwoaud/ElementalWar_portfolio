using System;
using UnityEngine;
using UnityEngine.UI;

public class SelectionPopupPanel : BasePopupPanel
{
    [Header("UI 요소")]
    [SerializeField] private Button yesButton;
    [SerializeField] private Button noButton;

    [Header("버튼 클릭 이벤트")]
    private Action onYesAction;
    private Action onNoAction;


    protected override void InitializeListener()
    {
        yesButton?.onClick.AddListener(OnClickYesButton);
        noButton?.onClick.AddListener(OnClickNoButton);
    }

    protected override void UnregisterListener()
    {
        yesButton?.onClick.RemoveListener(OnClickYesButton);
        noButton?.onClick.RemoveListener(OnClickNoButton);
    }

    protected override void ResetUI()
    {
        messageText.text = "";
    }

    private void OnClickYesButton() // 네 버튼을 눌렀을 때 실행되는 함수
    {
        SoundManager.instance?.Play(SoundKey.ButtonClick);
        onYesAction?.Invoke();
        Hide();
    }

    private void OnClickNoButton() // 아니오 버튼을 눌렀을 때 실행되는 함수
    {
        SoundManager.instance?.Play(SoundKey.ButtonClick);
        onNoAction?.Invoke();
        Hide();
    }

    public void Setup(string message, Action onYes, Action onNo) // 선택 팝업 패널을 활성화하는 함수
    {
        onYesAction = onYes;
        onNoAction = onNo;
        ShowPopup(message);
    }
}
