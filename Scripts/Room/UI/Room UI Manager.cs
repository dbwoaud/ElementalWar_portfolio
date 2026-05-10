using System;
using UnityEngine;
using UnityEngine.UI;

public class RoomUIManager : BaseUIManager<RoomUIManager>
{
    [Header("UI 요소")]
    [SerializeField] private Text roomNameText;
    [SerializeField] private Button exitButton;
    [SerializeField] private Button actionButton;
    [SerializeField] private Text actionButtonText;

    [Header("하위 컨테이너")]
    [SerializeField] private PlayerSlotContainer playerSlotContainer;

    public event Action OnClickExitRequest;
    public event Action OnClickActionRequest;


    protected override void InitUIElements()
    {
        playerSlotContainer?.ClearAllSlots();
    }

    protected override void BindButtonEvent()
    {
        exitButton?.onClick.AddListener(OnExitButtonClicked);
        actionButton?.onClick.AddListener(OnActionButtonClicked);
    }

    protected override void BindPanelEvent() { }

    protected override void UnbindButtonEvent()
    {
        exitButton?.onClick.RemoveListener(OnExitButtonClicked);
        actionButton?.onClick.RemoveListener(OnActionButtonClicked);
    }

    private void OnExitButtonClicked() // 나가기 버튼 클릭 시 실행되는 함수
    {
        PlayButtonSound();
        OnClickExitRequest?.Invoke();
    }

    private void OnActionButtonClicked() // 게임 시작 버튼 클릭 시 실행되는 함수
    {
        PlayButtonSound();
        OnClickActionRequest?.Invoke();
    }

    public void SetRoomNameUI(string roomName) // 방 이름을 출력하는 함수
    {
        roomNameText.text = roomName;
    }

    public void SetActionButtonText(string text) // 게임 시작 버튼 텍스트를 설정하는 함수
    {
        actionButtonText.text = text;
    }

    public void ClearSlot(int slotIndex) // 특정 플레이어 슬롯 UI를 초기화하는 함수
    {
        playerSlotContainer?.ClearSlot(slotIndex);
    }

    public void ClearAllSlots() // 모든 플레이어 슬롯 UI를 초기화하는 함수
    {
        playerSlotContainer?.ClearAllSlots();
    }

    public void UpdatePlayerSlot(int slotIndex, string name, bool isMaster, bool isReady) // 특정 플레이어 슬롯 UI를 업데이트하는 함수
    {
        playerSlotContainer?.UpdatePlayerSlot(slotIndex, name, isMaster, isReady);
    }
}
