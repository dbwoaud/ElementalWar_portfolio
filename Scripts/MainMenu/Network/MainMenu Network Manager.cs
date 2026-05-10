using Photon.Pun;
using UnityEngine;
using System;

public class MainMenuNetworkManager  : MonoBehaviourPunCallbacks
{
    [Header("포톤 네트워크 설정")]
    [SerializeField] private string gameVersion = "1.0";

    public event Action OnConnectedToMasterEvent;
    public event Action OnJoinedLobbyEvent;


    public void ConnectToPhoton(string nickname) // 포톤 네트워크 서버에 연결하는 함수
    {
        PhotonNetwork.AutomaticallySyncScene = false;
        PhotonNetwork.GameVersion = gameVersion;
        PhotonNetwork.LocalPlayer.NickName = nickname;
        PhotonNetwork.ConnectUsingSettings();
    }

    public override void OnConnectedToMaster() // 포톤 네트워크 마스터 서버에 연결하는 함수
    {
        OnConnectedToMasterEvent?.Invoke();
        PhotonNetwork.JoinLobby();
    }

    public override void OnJoinedLobby() // 로그인 후 로비로 진입하는 함수
    {
        OnJoinedLobbyEvent?.Invoke();
    }
}
