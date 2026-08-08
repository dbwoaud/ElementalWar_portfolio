using Photon.Pun;
using Photon.Realtime;
using ExitGames.Client.Photon;
using System;

public class RoomNetworkManager : MonoBehaviourPunCallbacks
{
    public event Action OnRoomStateUpdated; // 방 상태 업데이트 이벤트
    public event Action OnLeftRoomSuccess; // 방을 나갔을 때 실행되는 이벤트
    public event Action<Player> OnPlayerJoined; // 다른 플레이어 입장 이벤트
    public event Action<Player> OnPlayerLeft; // 다른 플레이어 퇴장 이벤트
    public event Action OnGameStart; // 게임 시작 시 실행되는 이벤트
    public event Action<Player> OnBecameMasterClient; // 방의 방장이 되었을 때 실행되는 이벤트 


    public override void OnPlayerPropertiesUpdate(Player targetPlayer, Hashtable changedProps) // 플레이어 정보를 업데이트하는 함수
    {
        if (changedProps.ContainsKey(PlayerConstants.Properties.GameReady))
            OnRoomStateUpdated?.Invoke();
    }

    public override void OnRoomPropertiesUpdate(Hashtable changedProps) // 방 정보를 업데이트하는 함수
    {
        if (changedProps.ContainsKey(RoomConstants.Properties.GameStart) && (bool)changedProps[RoomConstants.Properties.GameStart])
            OnGameStart?.Invoke();
    }

    public override void OnLeftRoom() // 방 퇴장 성공 시 실행되는 함수
    {
        OnLeftRoomSuccess?.Invoke();
    }

    public override void OnPlayerEnteredRoom(Player otherPlayer) // 플레이어 입장 시 실행되는 함수
    {
        OnRoomStateUpdated?.Invoke();
        OnPlayerJoined?.Invoke(otherPlayer);
    }

    public override void OnPlayerLeftRoom(Player otherPlayer) // 플레이어 퇴장 시 실행되는 함수
    {
        OnRoomStateUpdated?.Invoke();
        OnPlayerLeft?.Invoke(otherPlayer);
    }

    public override void OnMasterClientSwitched(Player newMasterClient) // 방의 방장이 바뀔 때 실행되는 함수
    {
        OnRoomStateUpdated?.Invoke();
        OnBecameMasterClient?.Invoke(newMasterClient);
    }

    public void InitializeRoomState() // 방 상태를 초기화하는 함수
    {
        SetLocalReadyState(false);
        if (PhotonNetwork.IsMasterClient && PhotonNetwork.CurrentRoom != null)
        {
            PhotonNetwork.CurrentRoom.IsOpen = true;
            ResetRoomProperties();
        }
    }

    public void SetLocalReadyState(bool isReady) // 플레이어의 준비 상태를 설정하는 함수
    {
        Hashtable props = new Hashtable() { { PlayerConstants.Properties.GameReady, isReady } };
        PhotonNetwork.LocalPlayer.SetCustomProperties(props);
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

    public void StartGame() // 게임을 시작하는 함수
    {
        PhotonNetwork.CurrentRoom.IsOpen = false;
        Hashtable roomProps = new Hashtable() { { RoomConstants.Properties.GameStart, true } };
        PhotonNetwork.CurrentRoom.SetCustomProperties(roomProps);
    }

    public void LeaveRoom() // 방을 퇴장하는 함수 함수
    {
        PhotonNetwork.LeaveRoom();
    }
}