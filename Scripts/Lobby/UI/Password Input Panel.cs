using UnityEngine;
using UnityEngine.UI;
using System;

public class PasswordInputPanel : UIPanel
{
    [Header("UI 요소")]
    [SerializeField] private InputField roomPasswordInputField;
    [SerializeField] private Button confirmButton;
    [SerializeField] private Button cancelButton;

    public event Action<string> OnSubmitPassword;
    public event Action OnCancelClicked;


    protected override void InitializeListener()
    {
        confirmButton?.onClick.AddListener(OnClickConfirmButton);
        cancelButton?.onClick.AddListener(OnClickCancelButton);
    }

    protected override void UnregisterListener()
    {
        confirmButton?.onClick.RemoveListener(OnClickConfirmButton);
        cancelButton?.onClick.RemoveListener(OnClickCancelButton);
    }

    private void OnClickConfirmButton() // 확인 버튼 클릭 시 실행되는 함수
    {
        SoundManager.instance?.Play(SoundKey.ButtonClick);
        OnSubmitPassword?.Invoke(roomPasswordInputField.text);
    }

    private void OnClickCancelButton() // 취소 버튼 클릭 시 실행되는 함수
    {
        SoundManager.instance?.Play(SoundKey.ButtonClick);
        OnCancelClicked?.Invoke();
        Hide();
    }

    public override void Hide()
    {
        ResetUI();
        base.Hide();
    }

    protected override void ResetUI()
    {
        roomPasswordInputField.text = "";
    }
}