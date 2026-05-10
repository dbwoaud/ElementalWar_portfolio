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

    public event Action<RoomInfo> OnRoomItemClicked;


    public void UpdateRoomList(List<RoomInfo> roomList) // 방 리스트를 업데이트하는 함수
    {
        foreach (var room in roomList)
        {
            if (room.RemovedFromList)
            {
                RoomListItem item;
                if (CheckRoomExist(room, out item))
                    RemoveRoom(room, item);
                continue;
            }

            RoomListItem existingItem;
            if (CheckRoomExist(room, out existingItem))
            {
                ChangeRoom(room, existingItem);
            }
            else
            {
                CreateRoom(room);
            }
        }
    }

    private bool CheckRoomExist(RoomInfo room, out RoomListItem item) // 방 리스트에 방이 있는지 확인하는 함수
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

    private void HandleRoomClick(RoomInfo info) // 방 버튼을 클릭할 때 실행되는 함수
    {
        OnRoomItemClicked?.Invoke(info);
    }

    public List<int> GetCurrentRoomNumbers() // 현재 방의 번호를 얻는 함수
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