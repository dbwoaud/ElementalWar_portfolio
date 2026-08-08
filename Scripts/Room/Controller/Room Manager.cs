using UnityEngine;
using Photon.Pun;
using Photon.Realtime;

public class RoomManager : BaseSceneController<RoomManager>
{
    [Header("캐싱 변수")]
    [SerializeField] private RoomUIManager roomUIManager;
    [SerializeField] private RoomNetworkManager roomNetworkManager;

    [Header("게임 설정 변수")]
    [SerializeField] private bool isLocalReady = false;


    protected override void SetUIManager() // UI 매니저를 설정하는 함수
    {
        if (RoomUIManager.Instance != null)
        {
            roomUIManager = RoomUIManager.Instance;
            roomUIManager.OnClickExitRequest += HandleLeaveRoom;
            roomUIManager.OnClickActionRequest += HandleActionButton;
        }
    }

    protected override void SetNetworkManager() // 네트워크 매니저를 설정하는 함수
    {
        if (roomNetworkManager != null)
        {
            roomNetworkManager.OnRoomStateUpdated += ResetRoomState;
            roomNetworkManager.OnLeftRoomSuccess += HandleLeftRoomSuccess;
            roomNetworkManager.OnGameStart += HandleGameStart;
            roomNetworkManager.OnBecameMasterClient += UpdateRoomStateWhenMasterSwitched;
        }
    }

    protected override void PlayBGM() // 씬의 배경음악을 재생하는 함수
    {
        SoundManager.Instance?.StopAll();
        SoundManager.Instance?.Play(SoundKey.RoomBGM);
    }

    protected override void InitializeState() // 씬의 초기 상태를 설정하는 함수
    {
        PopupPanelUIManager.Instance?.HideWaiting();
        InitializeRoomInfo();
        ResetRoomState();
        roomNetworkManager.InitializeRoomState();
    }

    protected override void ResetUIManager() // UI 매니저를 리셋하는 함수
    {
        if (roomUIManager != null)
        {
            roomUIManager.OnClickExitRequest -= HandleLeaveRoom;
            roomUIManager.OnClickActionRequest -= HandleActionButton;
        }
    }

    protected override void ResetNetworkManager() // 네트워크 매니저를 리셋하는 함수
    {
        if (roomNetworkManager != null)
        {
            roomNetworkManager.OnRoomStateUpdated -= ResetRoomState;
            roomNetworkManager.OnLeftRoomSuccess -= HandleLeftRoomSuccess;
            roomNetworkManager.OnGameStart -= HandleGameStart;
            roomNetworkManager.OnBecameMasterClient -= UpdateRoomStateWhenMasterSwitched;
        }
    }

    private void InitializeRoomInfo() // 방 정보를 초기화하는 함수
    {
        Room room = PhotonNetwork.CurrentRoom;
        if (room == null)
            return;

        int roomNum = GetRoomNumber(room);
        string roomName = GetRoomName(room);
        roomUIManager?.SetRoomNameUI($" {roomNum}: {roomName}");
    }

    private int GetRoomNumber(Room room) // 방의 정보에서 방 숫자를 반환하는 함수
    {
        int roomNum = 0;
        if (room.CustomProperties.ContainsKey(RoomConstants.Properties.RoomNumber))
            roomNum = (int)room.CustomProperties[RoomConstants.Properties.RoomNumber];

        return roomNum;
    }

    private string GetRoomName(Room room) // 방의 정보에서 방 이름을 반환하는 함수
    {
        string roomName = "";
        if (room.CustomProperties.ContainsKey(RoomConstants.Properties.RoomName))
            roomName = (string)room.CustomProperties[RoomConstants.Properties.RoomName];

        return roomName;
    }

    private void HandleLeaveRoom() // 방 나가기 버튼을 처리하는 함수
    {
        PopupPanelUIManager.Instance?.ShowSelection
        (
            PopupMessage.Selection.RoomExit,
            ExitRoom,
            null
        );
    }

    private void ExitRoom() // 방에서 퇴장하는 함수
    {
        PopupPanelUIManager.Instance?.ShowWaiting(PopupMessage.Waiting.ServerConnection, null);
        roomNetworkManager?.LeaveRoom();
    }

    private void HandleActionButton() // 게임 준비/시작 버튼 클릭 시 실행되는 함수
    {
        if(PhotonNetwork.IsMasterClient)
        {
            if(CanStartGame())
                roomNetworkManager?.StartGame(); 
        }
        else
        {
            isLocalReady = !isLocalReady;
            roomNetworkManager?.SetLocalReadyState(isLocalReady);
            ResetRoomState();
        }
    }

    private bool CanStartGame() // 게임 시작 가능 여부를 확인하는 함수
    {
        if (PhotonNetwork.CurrentRoom.PlayerCount < 2)
        {
            PopupPanelUIManager.Instance?.ShowError(PopupMessage.Error.NeedMorePlayer, null);
            return false;
        }

        foreach (Player p in PhotonNetwork.PlayerList)
        {
            if (!p.IsMasterClient)
            {
                if (!IsPlayerReady(p))
                {
                    PopupPanelUIManager.Instance?.ShowError(PopupMessage.Error.NeedAllReady, null);
                    return false;
                }
            }
        }
        return true;
    }

    private void ResetRoomState() // 방 상태를 초기화하는 함수
    {
        if (PhotonNetwork.CurrentRoom == null)
            return;

        roomUIManager?.ResetAllSlots();

        Player[] players = PhotonNetwork.PlayerList;
        bool isMaster = PhotonNetwork.IsMasterClient;
        bool allGuestsReady = UpdatePlayerSlotAndCheckReady(players);
        UpdateActionButtontText(players.Length, isMaster, allGuestsReady);
    }

    private bool UpdatePlayerSlotAndCheckReady(Player[] players) // 슬롯을 업데이트하고 플레이어의 준비 상태를 확인하는 함수
    {
        bool allGuestsReady = true;
        foreach (Player p in players)
        {
            bool pIsMaster = p.IsMasterClient;
            bool pIsReady = IsPlayerReady(p);

            int slotIndex = p.IsLocal ? 0 : 1;
            roomUIManager?.UpdatePlayerSlot(slotIndex, p.NickName, pIsMaster, pIsReady);

            if (!pIsMaster && !pIsReady)
                allGuestsReady = false;
        }
        return allGuestsReady;
    }

    private bool IsPlayerReady(Player p) // 플레이어 준비 상태를 확인하는 함수
    {
        return p.CustomProperties.ContainsKey(PlayerConstants.Properties.GameReady) &&
            (bool)p.CustomProperties[PlayerConstants.Properties.GameReady];
    }

    private void UpdateActionButtontText(int playerCount, bool isMaster, bool allGuestsReady) // 준비/시작 버튼 텍스트를 업데이트하는 함수
    {
        if (isMaster)
            roomUIManager?.SetActionButtonText(RoomConstants.ButtonText.Start);
        else
            roomUIManager?.SetActionButtonText(isLocalReady ? RoomConstants.ButtonText.CancelReady : RoomConstants.ButtonText.Ready);
    }

    private void HandleLeftRoomSuccess() // 방 퇴장 성공을 처리하는 함수
    {
        PopupPanelUIManager.Instance?.HideWaiting();
        PhotonNetwork.LoadLevel(SceneName.Lobby);
    }

    private void HandleGameStart() // 게임 시작을 처리하는 함수
    {
        if (PhotonNetwork.IsMasterClient)
            PhotonNetwork.LoadLevel(SceneName.UnitSetting);
    }

    private void UpdateRoomStateWhenMasterSwitched(Player newMaster) // 방장 교체 시 새 방장이 방 상태를 업데이트하는 함수
    {
        if (!newMaster.IsLocal)
            return;

        isLocalReady = false;
        roomNetworkManager?.InitializeRoomState();
        ResetRoomState();
    }
}