using Photon.Pun;
using Photon.Realtime;
using ExitGames.Client.Photon;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using UnityEngine;

public class LobbyManager : BaseSceneController<LobbyManager>
{
    [Header("캐싱 변수")]
    [SerializeField] private LobbyNetworkManager lobbyNetworkManager;
    [SerializeField] private LobbyUIManager lobbyUIManager;
    [SerializeField] private RoomInfo selectedPrivateRoom;


    protected override void SetUIManager() // UI 매니저를 설정하는 함수
    {
        if (LobbyUIManager.Instance != null)
        {
            lobbyUIManager = LobbyUIManager.Instance;
            lobbyUIManager.OnClickRandomButton += HandleRandomJoin;
            lobbyUIManager.OnClickRoomItem += HandleRoomClick;
            lobbyUIManager.OnRoomCreateSubmit += HandleCreateSubmit;
            lobbyUIManager.OnPasswordSubmit += HandlePasswordSubmit;
            lobbyUIManager.OnPasswordCancel += HandlePasswordCancel;
        }
    }

    protected override void SetNetworkManager() // 네트워크 매니저를 설정하는 함수
    {
        if (lobbyNetworkManager != null)
        {
            lobbyNetworkManager.OnRoomListUpdated += HandleRoomListUpdated;
            lobbyNetworkManager.OnJoinSuccess += HandleJoinRoomSuccess;
            lobbyNetworkManager.OnJoinFailed += HandleJoinRoomError;
            lobbyNetworkManager.OnConnectedToMasterEvent += HandleConnectedToMaster;
            lobbyNetworkManager.OnJoinedLobbyEvent += HandleJoinedLobby;
        }
    }

    protected override void ResetUIManager() // UI 매니저를 리셋하는 함수
    {
        if(lobbyUIManager != null)
        {
            lobbyUIManager.OnClickRandomButton -= HandleRandomJoin;
            lobbyUIManager.OnClickRoomItem -= HandleRoomClick;
            lobbyUIManager.OnRoomCreateSubmit -= HandleCreateSubmit;
            lobbyUIManager.OnPasswordSubmit -= HandlePasswordSubmit;
            lobbyUIManager.OnPasswordCancel -= HandlePasswordCancel;
        }
    }

    protected override void ResetNetworkManager() // 네트워크 매니저를 리셋하는 함수
    {
        if (lobbyNetworkManager != null)
        {
            lobbyNetworkManager.OnRoomListUpdated -= HandleRoomListUpdated;
            lobbyNetworkManager.OnJoinSuccess -= HandleJoinRoomSuccess;
            lobbyNetworkManager.OnJoinFailed -= HandleJoinRoomError;
            lobbyNetworkManager.OnConnectedToMasterEvent -= HandleConnectedToMaster;
            lobbyNetworkManager.OnJoinedLobbyEvent -= HandleJoinedLobby;
        }
    }

    protected override void PlayBGM() // 씬의 배경음악을 재생하는 함수
    {
        SoundManager.Instance?.StopAll();
        SoundManager.Instance?.Play(SoundKey.LobbyBGM);
    }

    protected override void InitializeState() // 씬의 초기상태를 설정하는 함수
    {
        PhotonNetwork.AutomaticallySyncScene = false;
        if (PhotonNetwork.LocalPlayer != null)
        {
            lobbyUIManager?.SetNicknameUI(PhotonNetwork.LocalPlayer.NickName);
            ResetLocalPlayerProperties();
        }

        if (CanEnterLobby())
        {
            PopupPanelUIManager.Instance?.ShowWaiting(PopupMessage.Waiting.LobbyConnection, null);
            PhotonNetwork.JoinLobby();
        }
    }

    private void ResetLocalPlayerProperties() // 로비 입장 시 로컬 플레이어의 정보를 초기화하는 함수
    {
        Hashtable resetProps = new Hashtable
        {
            { PlayerConstants.Properties.GameReady, false },
            { PlayerConstants.Properties.DeckReady, false },
            { PlayerConstants.Properties.DeckList, new string[0] }
        };
        PhotonNetwork.LocalPlayer.SetCustomProperties(resetProps);
    }

    private bool CanEnterLobby() // 로비 입장이 가능한 상태인지 확인하는 함수
    {
        return PhotonNetwork.IsConnectedAndReady && !PhotonNetwork.InLobby;
    }

    private void HandleRandomJoin() // 랜덤 방 버튼 클릭을 처리하는 함수
    {
        PopupPanelUIManager.Instance?.ShowWaiting(PopupMessage.Waiting.RandomMatching, null);
        lobbyNetworkManager?.JoinRandomRoom();
    }

    private void HandleRoomClick(RoomInfo info) // 방 버튼 클릭을 처리하는 함수
    {
        bool isPublic = (bool)info.CustomProperties[RoomConstants.Properties.PublicOrPrivate];
        if (isPublic)
        {
            PopupPanelUIManager.Instance?.ShowWaiting(PopupMessage.Waiting.RoomEntry, null);
            lobbyNetworkManager?.JoinRoom(info.Name);
        }
        else
        {
            selectedPrivateRoom = info;
            lobbyUIManager?.ShowPasswordPanel();
        }
    }

    private void HandleCreateSubmit(string name, string pw) // 방 생성 버튼 클릭을 처리하는 함수
    {
        if (CheckInvalidRoomName(name))
        {
            PopupPanelUIManager.Instance?.ShowError(PopupMessage.Error.InvalidRoomName);
            return;
        }
        
        if(CheckInvalidPassword(pw))
        {
            PopupPanelUIManager.Instance?.ShowError(PopupMessage.Error.InvalidPassword);
            return;
        }

        PopupPanelUIManager.Instance?.ShowWaiting(PopupMessage.Waiting.RoomCreate);
        int nextNum = CalculateNextNumber(lobbyUIManager?.GetCurrentRoomNumbers());
        lobbyNetworkManager?.CreateRoom(name, pw, nextNum);
    }

    private bool CheckInvalidRoomName(string name) // 유효 방 이름을 검사하는 함수
    {
        return string.IsNullOrWhiteSpace(name) || name.Length < 2 || name.Length > 16;
    }

    private bool CheckInvalidPassword(string pw) // 유효 비밀번호를 검사하는 함수
    {
        Regex regex = new Regex(RegexPattern.Room.ValidPassword);
        return !regex.IsMatch(pw) && !string.IsNullOrEmpty(pw);
    }

    private int CalculateNextNumber(List<int> existing) // 다음 방 번호를 계산하는 함수
    {
        int num = 1;
        while (existing.Contains(num)) 
            num++;

        return num;
    }

    private void HandlePasswordSubmit(string input)  // 비밀번호 패널에서 확인 버튼 클릭 시 실행되는 함수 
    {
        if (selectedPrivateRoom == null)
            return;

        string roomPassword = (string)selectedPrivateRoom.CustomProperties[RoomConstants.Properties.Password];
        if (input == roomPassword)
            EnterPrivateRoom();
        else
            PopupPanelUIManager.Instance?.ShowError(PopupMessage.Error.NotMatchPassword, null);      
    }

    private void EnterPrivateRoom() // 비공개 방에 들어가는 함수
    {
        lobbyUIManager?.HidePasswordPanel();
        PopupPanelUIManager.Instance?.ShowWaiting(PopupMessage.Waiting.RoomEntry, null);
        lobbyNetworkManager?.JoinRoom(selectedPrivateRoom.Name);
        selectedPrivateRoom = null;
    }

    private void HandlePasswordCancel() // 비밀번호 패널에서 취소 버튼 클릭 시 실행되는 함수
    {
        selectedPrivateRoom = null;
    }

    private void HandleRoomListUpdated(List<RoomInfo> list) // 방 목록 업데이트를 처리하는 함수
    {
        lobbyUIManager?.UpdateRoomListUI(list);
    }

    private void HandleJoinRoomSuccess() // 방 입장 성공 시 실행되는 함수
    {
        PopupPanelUIManager.Instance?.HideWaiting();
        lobbyUIManager?.HideRoomCreatePanel();
        PhotonNetwork.AutomaticallySyncScene = true;
        PhotonNetwork.LoadLevel(SceneName.Room);
    }

    private void HandleJoinRoomError(short code, string msg) // 방 입장 실패 시 실행되는 함수
    {
        string userMsg = ErrorTranslator.GetPhotonErrorMessage(code);
        PopupPanelUIManager.Instance?.HideWaiting();
        PopupPanelUIManager.Instance?.ShowError(userMsg, null);
    }

    private void HandleConnectedToMaster() // 마스터 서버 연결을 처리하는 함수
    {
        PopupPanelUIManager.Instance?.ShowWaiting(PopupMessage.Waiting.ServerConnection, null);
    } 

    private void HandleJoinedLobby() // 로비 입장 시 실행되는 함수
    {
        PopupPanelUIManager.Instance?.HideWaiting();
    }
}