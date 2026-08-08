using UnityEngine;
using System;
using System.Collections.Generic;
using Photon.Realtime;

public class RoomListContainer : MonoBehaviour
{
    [Header("방 목록 설정")]
    [SerializeField] private Transform roomListContent;
    [SerializeField] private GameObject roomListItemPrefab;

    [Header("방 목록 딕셔너리")]
    [SerializeField] private Dictionary<string, RoomListItem> roomItems = new Dictionary<string, RoomListItem>();

    public event Action<RoomInfo> OnRoomItemClicked; // 방 버튼 클릭 이벤트


    public void UpdateRoomList(List<RoomInfo> roomList) // 방 목록을 업데이트하는 함수
    {
        foreach (var room in roomList)
        {
            // 방 목록에 없고, 실제로 존재하지 않는 방일 경우, 방 항목 제거
            if (room.RemovedFromList) 
            {
                RoomListItem item;
                if (CheckRoomExist(room, out item))
                    RemoveRoom(room, item);

                continue;
            }

            RoomListItem existingItem;

            // 방 목록에 있고, 실제로 존재하는 방일 경우, 방 항목 업데이트
            if (CheckRoomExist(room, out existingItem))
            {
                ChangeRoom(room, existingItem);
            }
            // 방 목록에 있고, 실제로 존재하지 않는 방일 경우, 방 항목 생성
            else
            {
                CreateRoom(room);
            }
        }
    }

    private bool CheckRoomExist(RoomInfo room, out RoomListItem item) // 방 목록에 방이 있는지 확인하는 함수
    {
        return roomItems.TryGetValue(room.Name, out item);
    }

    private void RemoveRoom(RoomInfo room, RoomListItem item) // 방을 제거하는 함수
    {
        item.OnRoomItemClicked -= HandleRoomClick;
        Destroy(item.gameObject);
        roomItems.Remove(room.Name);
    }

    private void ChangeRoom(RoomInfo room, RoomListItem existingItem) // 방의 정보를 변경하는 함수
    {
        existingItem.Setup(room);
    }

    private void CreateRoom(RoomInfo room) // 방을 생성하는 함수
    {
        GameObject tempRoom = Instantiate(roomListItemPrefab, roomListContent);
        RoomListItem newItem = tempRoom.GetComponent<RoomListItem>();
        newItem.Setup(room);
        newItem.OnRoomItemClicked += HandleRoomClick;
        roomItems.Add(room.Name, newItem);
    }

    private void HandleRoomClick(RoomInfo info) // 방 버튼 클릭을 처리하는 함수
    {
        OnRoomItemClicked?.Invoke(info);
    }

    public List<int> GetCurrentRoomNumbers() // 현재 방의 번호 목록을 반환 함수
    {
        List<int> numbers = new List<int>();
        foreach (var item in roomItems.Values)
        {
            if (item.RoomData.CustomProperties.ContainsKey(RoomConstants.Properties.RoomNumber))
                numbers.Add((int)item.RoomData.CustomProperties[RoomConstants.Properties.RoomNumber]);
        }
        return numbers;
    }
}