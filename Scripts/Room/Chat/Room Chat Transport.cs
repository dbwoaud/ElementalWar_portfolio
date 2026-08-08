using Photon.Pun;
using Photon.Realtime;
using System;
using UnityEngine;

[RequireComponent(typeof(PhotonView))]
public class RoomChatTransport : MonoBehaviourPunCallbacks, IChatTransport
{
    [Header("캐싱 변수")]
    [SerializeField] private RoomNetworkManager roomNetworkManager;

    public event Action<string, string> OnPlayerMessageReceived; // 메시지 수신 이벤트
    public event Action<string> OnSystemMessageReceived; // 시스템 메시지 수신 이벤트


    public override void OnEnable()
    {
        base.OnEnable();
        if (roomNetworkManager != null)
        {
            roomNetworkManager.OnPlayerJoined += HandlePlayerJoined;
            roomNetworkManager.OnPlayerLeft += HandlePlayerLeft;
        }
    }

    public override void OnDisable()
    {
        base.OnDisable();
        if (roomNetworkManager != null)
        {
            roomNetworkManager.OnPlayerJoined -= HandlePlayerJoined;
            roomNetworkManager.OnPlayerLeft -= HandlePlayerLeft;
        }
    }

    public void Connect() // 플레이어 입장 메시지 이벤트를 실행하는 함수
    {
        OnSystemMessageReceived?.Invoke(PhotonNetwork.LocalPlayer.NickName + ChattingSystem.SystemMessage.PlayerEntered);
    }

    public void Disconnect() { }

    public void Send(string message) // 방 내 모든 플레이어에게 메시지를 발신하는 함수
    {
        photonView.RPC(nameof(RPC_ReceiveChat), RpcTarget.All, PhotonNetwork.LocalPlayer.NickName, message);
    }

    [PunRPC]
    private void RPC_ReceiveChat(string sender, string message) // 모든 플레이어가 메시지를 수신하는 RPC
    {
        OnPlayerMessageReceived?.Invoke(sender, message);
    }

    private void HandlePlayerJoined(Player player) // 다른 플레이어가 방 입장 시 시스템 메시지를 처리하는 함수
    {
        OnSystemMessageReceived?.Invoke(player.NickName + ChattingSystem.SystemMessage.PlayerEntered);
    }

    private void HandlePlayerLeft(Player player) // 다른 플레이어가 방 퇴장 시 시스템 메시지를 처리하는 함수
    {
        OnSystemMessageReceived?.Invoke(player.NickName + ChattingSystem.SystemMessage.PlayerExited);
    }
}