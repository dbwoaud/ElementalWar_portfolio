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

    public event Action OnSignUpClicked; // 회원가입 버튼 클릭 이벤트 
    public event Action<string, string> OnLoginSubmit; // 로그인 버튼 클릭 이벤트


    protected override void RegisterListener() // UI 리스너를 등록하는 함수
    {
        closeButton?.onClick.AddListener(OnClickCloseButton);
        loginButton?.onClick.AddListener(OnClickLoginButton);
        signUpButton?.onClick.AddListener(OnClickSignUpButton);
    }

    protected override void UnregisterListener() // UI 리스너를 해제하는 함수
    {
        closeButton?.onClick.RemoveListener(OnClickCloseButton);
        loginButton?.onClick.RemoveListener(OnClickLoginButton);
        signUpButton?.onClick.RemoveListener(OnClickSignUpButton);
    }

    protected override void ResetUI() // UI를 리셋시키는 함수
    {
        emailInputField.text = "";
        passwordInputField.text = "";
    }

    public override void Hide() // 패널을 비활성화시키는 함수
    {
        ResetUI();
        base.Hide();
    }

    private void OnClickCloseButton() // 닫기 버튼 클릭 시 실행되는 함수
    {
        SoundManager.Instance?.Play(SoundKey.ButtonClick);
        Hide();
    }

    private void OnClickLoginButton() // 로그인 버튼 클릭 시 실행되는 함수
    {
        SoundManager.Instance?.Play(SoundKey.ButtonClick);
        OnLoginSubmit?.Invoke(emailInputField.text, passwordInputField.text);
    }


    private void OnClickSignUpButton() // 회원가입 버튼 클릭 시 실행되는 함수
    {
        SoundManager.Instance?.Play(SoundKey.ButtonClick);
        OnSignUpClicked?.Invoke();
    }
}