using Photon.Pun;
using System.Text.RegularExpressions;
using UnityEngine;

public class MainMenuManager : BaseSceneController<MainMenuManager>
{
    [Header("캐싱 변수")]
    [SerializeField] private MainMenuUIManager mainMenuUIManager;
    [SerializeField] private MainMenuNetworkManager mainMenuNetworkManager;
    [SerializeField] private PlayFabAuthManager playFabManager;


    protected override void SetUIManager() // UI 매니저를 설정하는 함수
    {
        if (MainMenuUIManager.Instance != null)
        {
            mainMenuUIManager = MainMenuUIManager.Instance;
            mainMenuUIManager.OnLoginRequest += HandleLogin;
            mainMenuUIManager.OnRegisterRequest += HandleRegister;
        }
    }

    protected override void SetNetworkManager() // 네트워크 매니저를 설정하는 함수
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
        SoundManager.Instance?.StopAll();
        SoundManager.Instance?.Play(SoundKey.MainMenuBGM);
    }

    protected override void InitializeState() // 씬의 초기상태를 설정하는 함수
    {
        
    }

    protected override void ResetUIManager() // UI 매니저를 리셋하는 함수
    {
        if (mainMenuUIManager != null)
        {
            mainMenuUIManager.OnLoginRequest -= HandleLogin;
            mainMenuUIManager.OnRegisterRequest -= HandleRegister;
        }
    }

    protected override void ResetNetworkManager() // 네트워크 매니저를 리셋하는 함수
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
        PopupPanelUIManager.Instance?.ShowWaiting(PopupMessage.Waiting.Login, null);
        playFabManager?.PlayFabLogin(email, password);
    }

    private void OnLoginSuccess(string nickname) // 로그인 성공 시 실행되는 함수
    {
        mainMenuUIManager?.HideLoginPanel();
        PopupPanelUIManager.Instance?.ShowWaiting(PopupMessage.Waiting.ServerConnection, null);
        mainMenuNetworkManager?.ConnectToPhoton(nickname);
    }

    private void OnLoginFailure(string errorMsg) // 로그인 실패 시 실행되는 함수
    {
        PopupPanelUIManager.Instance?.HideWaiting();
        PopupPanelUIManager.Instance?.ShowError(errorMsg);
    }

    private void HandleRegister(string nickname, string email, string password) // 회원가입을 처리하는 함수
    {
        if (!CheckValidNickname(nickname))
        {
            PopupPanelUIManager.Instance?.ShowError(PopupMessage.Error.InvalidNickname);
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
        PopupPanelUIManager.Instance?.ShowConfirm
        (
            PopupMessage.Confirm.SuccessRegister,
            HandleRegisterSuccess
        );   
    }

    private void HandleRegisterSuccess() // 회원가입 성공을 처리하는 함수
    {
        mainMenuUIManager?.HideRegisterPanel();
    }

    private void OnRegisterFailure(string errorMsg) // 회원가입 실패 시 실행되는 함수
    {
        PopupPanelUIManager.Instance?.ShowError(errorMsg);
    }

    private void HandleConnectedToMaster() // 마스터 서버 연결을 처리하는 함수
    {
        PopupPanelUIManager.Instance?.ShowWaiting(PopupMessage.Waiting.LobbyConnection, null);
    }

    private void HandleJoinedLobby() // 로비 입장 시 실행되는 함수
    {
        PopupPanelUIManager.Instance?.HideWaiting();
        PhotonNetwork.LoadLevel(SceneName.Lobby);
    }
}
