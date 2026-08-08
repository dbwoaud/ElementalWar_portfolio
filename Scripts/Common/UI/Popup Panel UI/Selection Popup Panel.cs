using System;
using UnityEngine;
using UnityEngine.UI;

public class SelectionPopupPanel : BasePopupPanel
{
    [Header("UI 요소")]
    [SerializeField] private Button yesButton;
    [SerializeField] private Button noButton;

    private Action onYesAction; // 예 버튼 클릭 이벤트
    private Action onNoAction; // 아니오 버튼 클릭 이벤트 


    protected override void RegisterListener() // UI 리스너를 등록하는 함수
    {
        yesButton?.onClick.AddListener(OnClickYesButton);
        noButton?.onClick.AddListener(OnClickNoButton);
    }

    protected override void UnregisterListener() // UI 리스너를 해제하는 함수
    {
        yesButton?.onClick.RemoveListener(OnClickYesButton);
        noButton?.onClick.RemoveListener(OnClickNoButton);
    }

    protected override void ResetUI() // UI를 리셋하는 함수
    {
        messageText.text = "";
    }

    private void OnClickYesButton() // 네 버튼을 눌렀을 때 실행되는 함수
    {
        SoundManager.Instance?.Play(SoundKey.ButtonClick);
        onYesAction?.Invoke();
        Hide();
    }

    private void OnClickNoButton() // 아니오 버튼을 눌렀을 때 실행되는 함수
    {
        SoundManager.Instance?.Play(SoundKey.ButtonClick);
        onNoAction?.Invoke();
        Hide();
    }

    public void Setup(string message, Action onYes, Action onNo) // 선택 팝업 패널을 설정하는 함수
    {
        onYesAction = onYes;
        onNoAction = onNo;
        ShowPopup(message);
    }
}
