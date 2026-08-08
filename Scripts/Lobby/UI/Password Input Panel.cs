using UnityEngine;
using UnityEngine.UI;
using System;

public class PasswordInputPanel : UIPanel
{
    [Header("UI 요소")]
    [SerializeField] private InputField roomPasswordInputField;
    [SerializeField] private Button confirmButton;
    [SerializeField] private Button cancelButton;

    public event Action<string> OnSubmitPassword; // 확인 버튼 클릭 이벤트
    public event Action OnCancelClicked; // 취소 버튼 클릭 이벤트


    protected override void RegisterListener() // UI 리스너를 등록하는 함수
    {
        confirmButton?.onClick.AddListener(OnClickConfirmButton);
        cancelButton?.onClick.AddListener(OnClickCancelButton);
    }

    protected override void UnregisterListener() // UI 리스너를 해제하는 함수
    {
        confirmButton?.onClick.RemoveListener(OnClickConfirmButton);
        cancelButton?.onClick.RemoveListener(OnClickCancelButton);
    }

    protected override void ResetUI() // UI를 리셋시키는 함수
    {
        roomPasswordInputField.text = "";
    }

    public override void Hide() // 패널을 비활성화하는 함수
    {
        ResetUI();
        base.Hide();
    }

    private void OnClickConfirmButton() // 확인 버튼 클릭 시 실행되는 함수
    {
        SoundManager.Instance?.Play(SoundKey.ButtonClick);
        OnSubmitPassword?.Invoke(roomPasswordInputField.text);
    }

    private void OnClickCancelButton() // 취소 버튼 클릭 시 실행되는 함수
    {
        SoundManager.Instance?.Play(SoundKey.ButtonClick);
        OnCancelClicked?.Invoke();
        Hide();
    }
}