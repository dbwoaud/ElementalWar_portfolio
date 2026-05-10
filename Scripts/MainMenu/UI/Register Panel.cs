using System;
using UnityEngine;
using UnityEngine.UI;

public class RegisterPanel : UIPanel
{
    [Header("UI 요소")]
    [SerializeField] private InputField nicknameInputField;
    [SerializeField] private InputField registerEmailInputField;
    [SerializeField] private InputField registerPasswordInputField;
    [SerializeField] private Button registerButton;
    [SerializeField] private Button cancelButton;

    public event Action<string, string, string> OnRegisterSubmit;

    
    protected override void InitializeListener()
    {
        registerButton?.onClick.AddListener(OnClickRegisterButton);
        cancelButton?.onClick.AddListener(OnClickCancelButton);
    }

    protected override void UnregisterListener()
    {
        registerButton?.onClick.RemoveListener(OnClickRegisterButton);
        cancelButton?.onClick.RemoveListener(OnClickCancelButton);
    }

    private void OnClickRegisterButton() // 회원 등록 버튼 클릭 시 실행되는 함수
    {
        SoundManager.instance?.Play(SoundKey.ButtonClick);
        OnRegisterSubmit?.Invoke
        (
            nicknameInputField.text,
            registerEmailInputField.text,
            registerPasswordInputField.text
        );
    }

    private void OnClickCancelButton() // 취소 버튼 클릭 시 실행되는 함수
    {
        SoundManager.instance?.Play(SoundKey.ButtonClick);
        Hide();
    }

    protected override void ResetUI()
    {
        nicknameInputField.text = "";
        registerEmailInputField.text = "";
        registerPasswordInputField.text = "";
    }

    public override void Hide()
    {
        ResetUI();
        base.Hide();
    }
}
