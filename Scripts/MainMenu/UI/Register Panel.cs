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

    public event Action<string, string, string> OnRegisterSubmit; // 회원 등록 버튼 클릭 이벤트

    
    protected override void RegisterListener() // UI 리스너를 등록하는 함수
    {
        registerButton?.onClick.AddListener(OnClickRegisterButton);
        cancelButton?.onClick.AddListener(OnClickCancelButton);
    }

    protected override void UnregisterListener() // UI 리스너를 해제하는 함수
    {
        registerButton?.onClick.RemoveListener(OnClickRegisterButton);
        cancelButton?.onClick.RemoveListener(OnClickCancelButton);
    }

    protected override void ResetUI() // UI를 리셋시키는 함수
    {
        nicknameInputField.text = "";
        registerEmailInputField.text = "";
        registerPasswordInputField.text = "";
    }

    public override void Hide() // 패널을 비활성화시키는 함수
    {
        ResetUI();
        base.Hide();
    }

    private void OnClickRegisterButton() // 회원 등록 버튼 클릭 시 실행되는 함수
    {
        SoundManager.Instance?.Play(SoundKey.ButtonClick);
        OnRegisterSubmit?.Invoke
        (
            nicknameInputField.text,
            registerEmailInputField.text,
            registerPasswordInputField.text
        );
    }

    private void OnClickCancelButton() // 취소 버튼 클릭 시 실행되는 함수
    {
        SoundManager.Instance?.Play(SoundKey.ButtonClick);
        Hide();
    }
}