using System;
using UnityEngine;
using UnityEngine.UI;

public class ConfirmPopupPanel : BasePopupPanel
{
    [Header("UI 요소")]
    [SerializeField] private Button confirmButton;

    private Action onConfirmAction; // 확인 버튼 클릭 이벤트


    protected override void RegisterListener() // UI 리스너를 등록하는 함수
    {
        confirmButton?.onClick.AddListener(OnClickConfirmButton);
    }

    protected override void UnregisterListener() // UI 리스너를 해제하는 함수
    {
        confirmButton?.onClick.RemoveListener(OnClickConfirmButton);
    }

    protected override void ResetUI() // UI를 리셋하는 함수
    {
        messageText.text = "";
    }

    private void OnClickConfirmButton() // 확인 버튼 클릭 시 실행되는 함수
    {
        SoundManager.Instance?.Play(SoundKey.ButtonClick);
        onConfirmAction?.Invoke();
        Hide();
    }

    public void Setup(string message, Action onConfirm = null) // 확인 팝업 패널을 설정하는 함수
    {
        onConfirmAction = onConfirm;
        ShowPopup(message);
    }
}