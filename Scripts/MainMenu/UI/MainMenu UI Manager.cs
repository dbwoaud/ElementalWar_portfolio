using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MainMenuUIManager : BaseUIManager<MainMenuUIManager>
{
    [Header("UI 설정")]
    [SerializeField] private Button startButton;
    [SerializeField] private Button descriptionButton;
    [SerializeField] private Button exitButton;

    [Header("하위 패널 관리")]
    [SerializeField] private DescriptionPanel descriptionPanel;
    [SerializeField] private LoginPanel loginPanel;
    [SerializeField] private RegisterPanel registerPanel;
    [SerializeField] private List<UIPanel> uiPanels = new List<UIPanel>();

    public event Action<string, string> OnLoginRequest; // 로그인 요청 이벤트
    public event Action<string, string, string> OnRegisterRequest; // 회원 가입 요청 이벤트


    protected override void InitUIElements() // UI 요소 초기화 함수
    {
        uiPanels.Add(loginPanel);
        uiPanels.Add(registerPanel);
        uiPanels.Add(descriptionPanel);
        HideAllPanelsImmediate();
    }

    protected override void BindButtonEvent() // 버튼 이벤트 할당 함수
    {
        startButton?.onClick.AddListener(OnClickStartButton);
        descriptionButton?.onClick.AddListener(OnClickDescriptionButton);
        exitButton?.onClick.AddListener(OnClickExitButton);
    }

    protected override void BindPanelEvent() // 패널 내부 및 데이터 이벤트 할당 함수
    {
        if(loginPanel != null)
        {
            loginPanel.OnSignUpClicked += HandleSignUpClicked;
            loginPanel.OnLoginSubmit += HandleLoginSubmit;
        }

        if(registerPanel != null)
        {
            registerPanel.OnRegisterSubmit += HandleRegisterSubmit;
        }
    }

    protected override void UnbindButtonEvent() // 버튼 이벤트 해제 함수
    {
        startButton?.onClick.RemoveListener(OnClickStartButton);
        descriptionButton?.onClick.RemoveListener(OnClickDescriptionButton);
        exitButton?.onClick.RemoveListener(OnClickExitButton);
    }

    protected override void UnbindPanelEvent() // 패널 내부 및 데이터 이벤트 해제 함수
    {
        if(loginPanel != null)
        {
            loginPanel.OnSignUpClicked -= HandleSignUpClicked;
            loginPanel.OnLoginSubmit -= HandleLoginSubmit;
        }

        if(registerPanel != null)
        {
            registerPanel.OnRegisterSubmit -= HandleRegisterSubmit;
        }
    }

    private void HideAllPanelsImmediate() // 모든 하위 패널을 즉시 비활성화시키는 함수
    {
        foreach (var panel in uiPanels)
            panel?.HideImmediate();
    }

    private void HandleSignUpClicked() // 회원가입 버튼 클릭 시 실행되는 함수
    {
        registerPanel.Show();
    }

    private void HandleLoginSubmit(string email, string pw) // 로그인 버튼 클릭 시 실행되는 함수
    {
        OnLoginRequest?.Invoke(email, pw);
    }

    private void HandleRegisterSubmit(string nick, string email, string pw) // 회원 등록 버튼 클릭 시 실행되는 함수
    {
        OnRegisterRequest?.Invoke(nick, email, pw);
    }

    private void OnClickStartButton() // 게임 시작 버튼 클릭 시 실행되는 함수
    {
        PlayButtonSound();
        loginPanel.Show();
    }

    private void OnClickDescriptionButton() // 게임 설명 버튼 클릭 시 실행되는 함수
    {
        PlayButtonSound();
        descriptionPanel.Show();
    }

    private void OnClickExitButton() // 게임 나가기 버튼 클릭 시 실행되는 함수
    {
        PlayButtonSound();
        PopupPanelUIManager.Instance?.ShowSelection
        (
                PopupMessage.Selection.GameExit,
                ExitGame,
                null
        );
    }

    private void ExitGame() // 게임을 종료시키는 함수
    {
        Application.Quit();
    }

    public void HideRegisterPanel() // 회원가입 패널을 비활성화하는 함수
    {
        registerPanel.Hide();
    }

    public void HideLoginPanel() // 로그인 패널을 비활성화하는 함수
    {
        loginPanel.Hide();
    }
}