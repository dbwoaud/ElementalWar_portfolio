using System;
using System.Collections.Generic;
using Photon.Pun;
using Photon.Realtime;
using ExitGames.Client.Photon;


public class LobbyNetworkManager : MonoBehaviourPunCallbacks
{
     public event Action<List<RoomInfo>> OnRoomListUpdated;
     public event Action OnJoinSuccess;
     public event Action<short, string> OnJoinFailed;
     public event Action OnConnectedToMasterEvent;
     public event Action OnJoinedLobbyEvent;


    public override void OnRoomListUpdate(List<RoomInfo> roomList) // 방 목록을 업데이트하는 함수
    {
        OnRoomListUpdated?.Invoke(roomList); 
    }

    public override void OnJoinedRoom() // 방 입장 성공 시 실행되는 함수
    {
        OnJoinSuccess?.Invoke();
    }

    public override void OnJoinRoomFailed(short code, string msg) // 방 입장 실패 시 실행되는 함수
    {
        OnJoinFailed?.Invoke(code, msg);
    }

    public override void OnJoinRandomFailed(short code, string msg) // 랜덤 방 입장 시 실행되는 함수
    {
        OnJoinFailed?.Invoke(code, msg);
    }

    public override void OnConnectedToMaster() // 마스터 서버에 연결 시 실행되는 함수
    {
        OnConnectedToMasterEvent?.Invoke();
        PhotonNetwork.JoinLobby();
    }

    public override void OnJoinedLobby() // 로비에 진입 성공 시 실행되는 함수
    {
        OnJoinedLobbyEvent?.Invoke();
    }

    public void CreateRoom(string roomName, string password, int roomNumber) // 방을 생성하는 함수
    {
        string roomPK = Guid.NewGuid().ToString();
        bool isPublic = string.IsNullOrEmpty(password);
        Hashtable props = SetRoomProperties(roomName, password, roomNumber, isPublic);
        RoomOptions options = SetRoomOptions(props);
        PhotonNetwork.CreateRoom(roomPK, options);
    }

    private Hashtable SetRoomProperties(string roomName, string password, int roomNumber, bool isPublic) // 방 속성을 설정하는 함수
    {
        return new Hashtable()
        {
            { RoomConstants.Properties.RoomNumber, roomNumber },
            { RoomConstants.Properties.RoomName, roomName },
            { RoomConstants.Properties.PublicOrPrivate, isPublic },
            { RoomConstants.Properties.GameStart, false },
            { RoomConstants.Properties.Password, password }
        };
    }

    private RoomOptions SetRoomOptions(Hashtable props) // 방 옵션을 설정하는 함수
    {
        return new RoomOptions
        {
            MaxPlayers = 2,
            IsVisible = true,
            CustomRoomProperties = props,
            CustomRoomPropertiesForLobby = new string[] 
            { 
                RoomConstants.Properties.RoomNumber, 
                RoomConstants.Properties.RoomName, 
                RoomConstants.Properties.PublicOrPrivate, 
                RoomConstants.Properties.GameStart, 
                RoomConstants.Properties.Password 
            },
            CleanupCacheOnLeave = false
        };
    }

    public void JoinRoom(string roomPK) // 방에 입장하는 함수
    {
        PhotonNetwork.JoinRoom(roomPK);
    }

    public void JoinRandomRoom() // 랜덤 방에 입장하는 함수
    {
        Hashtable expectedRoomProperties = GetPublicRoom();
        PhotonNetwork.JoinRandomRoom(expectedRoomProperties, 0);
    }

    private Hashtable GetPublicRoom() // 공개 방을 얻는 함수
    {
        return new Hashtable()
        {
            { RoomConstants.Properties.PublicOrPrivate, true }
        };
    }
}
