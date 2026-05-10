using System;
using UnityEngine;
using UnityEngine.UI;

public class LoginPanel : UIPanel
{
    [Header("UI 요소")]
    [SerializeField] private InputField emailInputField;
    [SerializeField] private InputField passwordInputField;
    [SerializeField] private Button closeButton;
    [SerializeField] private Button loginButton;
    [SerializeField] private Button signUpButton;

    public event Action OnSignUpClicked;
    public event Action<string, string> OnLoginSubmit;


    protected override void InitializeListener()
    {
        closeButton?.onClick.AddListener(OnClickCloseButton);
        loginButton?.onClick.AddListener(OnClickLoginButton);
        signUpButton?.onClick.AddListener(OnClickSignUpButton);
    }

    protected override void UnregisterListener()
    {
        closeButton?.onClick.RemoveListener(OnClickCloseButton);
        loginButton?.onClick.RemoveListener(OnClickLoginButton);
        signUpButton?.onClick.RemoveListener(OnClickSignUpButton);
    }

    private void OnClickCloseButton() // 닫기 버튼 클릭 시 실행되는 함수
    {
        SoundManager.instance?.Play(SoundKey.ButtonClick);
        Hide();
    }

    private void OnClickLoginButton() // 로그인 버튼 클릭 시 실행되는 함수
    {
        SoundManager.instance?.Play(SoundKey.ButtonClick);
        OnLoginSubmit?.Invoke(emailInputField.text, passwordInputField.text);
    }


    private void OnClickSignUpButton() // 회원가입 버튼 클릭 시 실행되는 함수
    {
        SoundManager.instance?.Play(SoundKey.ButtonClick);
        OnSignUpClicked?.Invoke();
    }

    protected override void ResetUI()
    {
        emailInputField.text = "";
        passwordInputField.text = "";
    }

    public override void Hide()
    {
        ResetUI();
        base.Hide();
    }
}
