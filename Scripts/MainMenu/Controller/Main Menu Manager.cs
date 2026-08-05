using Photon.Pun;
using System.Text.RegularExpressions;
using UnityEngine;

public class MainMenuManager : BaseSceneController<MainMenuManager>
{
    [Header("캐싱 변수")]
    [SerializeField] private MainMenuUIManager mainMenuUIManager;
    [SerializeField] private MainMenuNetworkManager mainMenuNetworkManager;
    [SerializeField] private PlayFabAuthManager playFabManager;


    protected override void SetUIManager() // UI Manager를 설정하는 함수
    {
        if (MainMenuUIManager.instance != null)
        {
            mainMenuUIManager = MainMenuUIManager.instance;
            mainMenuUIManager.OnLoginRequest += HandleLogin;
            mainMenuUIManager.OnRegisterRequest += HandleRegister;
        }
    }

    protected override void SetNetworkManager() // NetworkManager를 설정하는 함수
    {
        if (playFabManager != null)
        {
            playFabManager.OnLoginSuccessEvent += OnLoginSuccess;
            playFabManager.OnLoginErrorEvent += OnLoginFailure;
            playFabManager.OnRegisterSuccessEvent += OnRegisterSuccess;
            playFabManager.OnRegisterErrorEvent += OnRegisterFailure;
        }
        if (mainMenuNetworkManager != null)
        {
            mainMenuNetworkManager.OnConnectedToMasterEvent += HandleConnectedToMaster;
            mainMenuNetworkManager.OnJoinedLobbyEvent += HandleJoinedLobby;
        }
    }

    protected override void PlayBGM() // 씬의 배경음악을 재생하는 함수
    {
        SoundManager.instance?.StopAll();
        SoundManager.instance?.Play(SoundKey.MainMenuBGM);
    }

    protected override void InitializeState()
    {
        
    }

    protected override void ResetUIManager()
    {
        if (mainMenuUIManager != null)
        {
            mainMenuUIManager.OnLoginRequest -= HandleLogin;
            mainMenuUIManager.OnRegisterRequest -= HandleRegister;
        }
    }

    protected override void ResetNetworkManager()
    {
        if (playFabManager != null)
        {
            playFabManager.OnLoginSuccessEvent -= OnLoginSuccess;
            playFabManager.OnLoginErrorEvent -= OnLoginFailure;
            playFabManager.OnRegisterSuccessEvent -= OnRegisterSuccess;
            playFabManager.OnRegisterErrorEvent -= OnRegisterFailure;
        }
        if (mainMenuNetworkManager != null)
        {
            mainMenuNetworkManager.OnConnectedToMasterEvent -= HandleConnectedToMaster;
            mainMenuNetworkManager.OnJoinedLobbyEvent -= HandleJoinedLobby;
        }
    }

    private void HandleLogin(string email, string password) // 로그인을 처리하는 함수
    {
        PopupPanelUIManager.instance?.ShowWaiting(PopupMessage.Waiting.Login, null);
        playFabManager?.PlayFabLogin(email, password);
    }

    private void OnLoginSuccess(string nickname) // 로그인 성공 시 실행되는 함수
    {
        mainMenuUIManager?.HideLoginPanel();
        PopupPanelUIManager.instance?.ShowWaiting(PopupMessage.Waiting.ServerConnection, null);
        mainMenuNetworkManager?.ConnectToPhoton(nickname);
    }

    private void OnLoginFailure(string errorMsg) // 로그인 실패 시 실행되는 함수
    {
        PopupPanelUIManager.instance?.HideWaiting();
        PopupPanelUIManager.instance?.ShowError(errorMsg);
    }

    private void HandleRegister(string nickname, string email, string password) // 회원가입을 처리하는 함수
    {
        if (!CheckValidNickname(nickname))
        {
            PopupPanelUIManager.instance?.ShowError(PopupMessage.Error.InvalidNickname);
            return;
        }
        playFabManager?.PlayFabRegister(email, password, nickname);
    }

    private bool CheckValidNickname(string nickname) // 유효 닉네임을 검사하는 함수
    {
        if (string.IsNullOrWhiteSpace(nickname))
            return false;

        Regex regex = new Regex(RegexPattern.User.ValidNickname);
        return regex.IsMatch(nickname);
    }

    private void OnRegisterSuccess() // 회원가입 성공 시 실행되는 함수
    {
        PopupPanelUIManager.instance?.ShowConfirm
        (
            PopupMessage.Confirm.SuccessRegister,
            HandleRegisterSuccess
        );   
    }

    private void HandleRegisterSuccess() // 회원가입 성공을 처리하는 함수
    {
        mainMenuUIManager?.SetUIRegisterSuccess();
    }

    private void OnRegisterFailure(string errorMsg) // 회원가입 실패 시 실행되는 함수
    {
        PopupPanelUIManager.instance?.ShowError(errorMsg);
    }

    private void HandleConnectedToMaster() // 마스터 서버 연결을 처리하는 함수
    {
        PopupPanelUIManager.instance?.ShowWaiting(PopupMessage.Waiting.LobbyConnection, null);
    }

    private void HandleJoinedLobby() // 로비 입장 시 실행되는 함수
    {
        PopupPanelUIManager.instance?.HideWaiting();
        PhotonNetwork.LoadLevel(SceneName.Lobby);
    }
}
