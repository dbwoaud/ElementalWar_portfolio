using Photon.Pun;
using Photon.Realtime;
using ExitGames.Client.Photon;
using System;

public class RoomNetworkManager : MonoBehaviourPunCallbacks
{
    public event Action OnRoomStateUpdated;
    public event Action OnLeftRoomSuccess;

    public event Action<Player> OnPlayerJoined;
    public event Action<Player> OnPlayerLeft;

    public event Action OnGameStart;
    public event Action<Player> OnBecameMasterClient;


    public void InitializeRoomState() // 방 상태를 초기화하는 함수
    {
        SetLocalReadyState(false);
        if (PhotonNetwork.IsMasterClient && PhotonNetwork.CurrentRoom != null)
        {
            PhotonNetwork.CurrentRoom.IsOpen = true;
            ResetRoomProperties();
        }
    }

    private void ResetRoomProperties() // 방 정보를 초기화하는 함수
    {
        Hashtable roomProps = new Hashtable()
        {
            {  RoomConstants.Properties.GameStart, false },
            {  RoomConstants.Properties.MapIndex, null }
        };
        PhotonNetwork.CurrentRoom.SetCustomProperties(roomProps);
    }

    public void SetLocalReadyState(bool isReady) // 플레이어의 준비 상태를 설정하는 함수
    {
        Hashtable props = new Hashtable() { { PlayerConstants.Properties.GameReady, isReady } };
        PhotonNetwork.LocalPlayer.SetCustomProperties(props);
    }

    public void StartGame() // 게임을 시작하는 함수
    {
        PhotonNetwork.CurrentRoom.IsOpen = false;
        Hashtable roomProps = new Hashtable() { { RoomConstants.Properties.GameStart, true } };
        PhotonNetwork.CurrentRoom.SetCustomProperties(roomProps);
    }

    public void LeaveRoom() // 방 퇴장을 시도하는 함수
    {
        PhotonNetwork.LeaveRoom();
    }

    public override void OnLeftRoom() // 방 퇴장 성공 시 실행되는 함수
    {
        OnLeftRoomSuccess?.Invoke();
    }

    public override void OnPlayerEnteredRoom(Player newPlayer) // 플레이어 입장 시 실행되는 함수
    {
        OnRoomStateUpdated?.Invoke();
        OnPlayerJoined?.Invoke(newPlayer);
    }

    public override void OnPlayerLeftRoom(Player otherPlayer) // 플레이어 퇴장 시 실행되는 함수
    { 
        OnRoomStateUpdated?.Invoke();
        OnPlayerLeft?.Invoke(otherPlayer);
    }

    public override void OnMasterClientSwitched(Player newMasterClient) // 방장이 바뀔 때 실행되는 함수
    {
        OnRoomStateUpdated?.Invoke();
        OnBecameMasterClient?.Invoke(newMasterClient);
    }

    public override void OnPlayerPropertiesUpdate(Player targetPlayer, Hashtable changedProps) // 플레이어 정보를 업데이트하는 함수
    {
        if (changedProps.ContainsKey(PlayerConstants.Properties.GameReady))
            OnRoomStateUpdated?.Invoke();
    }

    public override void OnRoomPropertiesUpdate(Hashtable propertiesThatChanged) // 방 정보를 업데이트하는 함수
    {
        if (propertiesThatChanged.ContainsKey(RoomConstants.Properties.GameStart) && (bool)propertiesThatChanged[RoomConstants.Properties.GameStart])
            OnGameStart?.Invoke();

    }
}