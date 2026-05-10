using UnityEngine;
using UnityEngine.UI;
using Photon.Realtime;
using System;

public class RoomListItem : MonoBehaviour
{
    [Header("UI 요소")]
    [SerializeField] private Button joinButton;
    [SerializeField] private Text roomNameText;
    [SerializeField] private Text gameStatusText;
    [SerializeField] private GameObject lockIcon;
    [SerializeField] private Text playerCountText;

    [Header("방 정보")]
    [SerializeField] private RoomInfo roomData;
    public RoomInfo RoomData => roomData;

    public event Action<RoomInfo> OnRoomItemClicked;


    private void Start()
    {
        InitializeListener();
    }

    private void InitializeListener() // UI 리스너를 설정하는 함수
    {
        joinButton?.onClick.AddListener(OnClickEntryButton);
    }

    private void OnDestroy()
    {
        joinButton?.onClick.RemoveListener(OnClickEntryButton);
    }

    private void OnClickEntryButton() // 방 버튼 클릭 시 실행되는 함수
    {
        SoundManager.instance?.Play(SoundKey.ButtonClick);
        OnRoomItemClicked?.Invoke(roomData);
    }

    public void Setup(RoomInfo roominfo) // 방 정보를 UI에 세팅하는 함수
    {
        if (roominfo == null)
            return;

        roomData = roominfo;
        DisplayRoomInfo();
    }

    private void DisplayRoomInfo() // 방 정보를 표시하는 함수
    {
        SetRoomNameText();
        SetPlayerCountText();
        SetRoomVisibility();
        SetRoomStatusText();
    }

    private void SetRoomNameText() // 방 이름을 설정하는 함수
    {
        if (roomData.CustomProperties.ContainsKey(RoomConstants.Properties.RoomNumber) && 
            roomData.CustomProperties.ContainsKey(RoomConstants.Properties.RoomName))
        {
            int roomNum = (int)roomData.CustomProperties[RoomConstants.Properties.RoomNumber];
            string roomName = (string)roomData.CustomProperties[RoomConstants.Properties.RoomName];
            roomNameText.text = $"{roomNum}: {roomName}";
        }
    }

    private void SetPlayerCountText() // 방의 플레이어 수를 설정하는 함수
    {
        playerCountText.text = $"{roomData.PlayerCount}/{roomData.MaxPlayers}";
    }

    private void SetRoomVisibility() // 방의 공개/비공개 여부를 설정하는 함수
    {
        if (roomData.CustomProperties.ContainsKey(RoomConstants.Properties.PublicOrPrivate))
        {
            bool isPublic = (bool)roomData.CustomProperties[RoomConstants.Properties.PublicOrPrivate];
            lockIcon.SetActive(!isPublic);
        }
    }

    private void SetRoomStatusText() // 방의 게임 진행 상태를 설정하는 함수
    {
        if (roomData.CustomProperties.ContainsKey(RoomConstants.Properties.GameStart))
        {
            bool isGameStarted = (bool)roomData.CustomProperties[RoomConstants.Properties.GameStart];
            joinButton.interactable = !isGameStarted;
            gameStatusText.text = isGameStarted ? RoomConstants.Status.OnGoing : RoomConstants.Status.Waiting;
        }
    }
}