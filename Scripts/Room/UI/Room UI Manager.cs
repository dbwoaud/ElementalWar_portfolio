using System;
using UnityEngine;
using UnityEngine.UI;

public class RoomUIManager : BaseUIManager<RoomUIManager>
{
    [Header("UI 요소")]
    [SerializeField] private Text roomNameText;
    [SerializeField] private Button exitButton;
    [SerializeField] private Button actionButton;

    [Header("하위 컨테이너")]
    [SerializeField] private PlayerSlotContainer playerSlotContainer;

    public event Action OnClickExitRequest; // 방 나가기 버튼 클릭 이벤트
    public event Action OnClickActionRequest; // 방 준비/시작 버튼 클릭 이벤트


    protected override void InitUIElements() // UI 요소 초기화 함수
    {
        playerSlotContainer?.ResetAllSlots();
    }

    protected override void BindButtonEvent() // 버튼 이벤트 할당 함수
    {
        exitButton?.onClick.AddListener(OnExitButtonClicked);
        actionButton?.onClick.AddListener(OnActionButtonClicked);
    }

    protected override void BindPanelEvent() // 패널 내부 및 데이터 이벤트 할당 함수
    {

    } 

    protected override void UnbindButtonEvent() // 버튼 이벤트 해베 함수
    {
        exitButton?.onClick.RemoveListener(OnExitButtonClicked);
        actionButton?.onClick.RemoveListener(OnActionButtonClicked);
    }

    protected override void UnbindPanelEvent() // 패널 내부 및 데이터 이벤트 해제 함수
    { 
    
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

    public void SetRoomNameUI(string roomName) // 방 이름 UI를 설정하는 함수
    {
        roomNameText.text = roomName;
    }

    public void SetActionButtonText(string text) // 게임 준비/시작 버튼 텍스트를 설정하는 함수
    {
        actionButton.GetComponent<Text>().text = text;
    }

    public void ResetSlot(int slotIndex) // 특정 플레이어 슬롯 UI를 초기화하는 함수
    {
        playerSlotContainer?.ResetSlot(slotIndex);
    }

    public void ResetAllSlots() // 모든 플레이어 슬롯 UI를 초기화하는 함수
    {
        playerSlotContainer?.ResetAllSlots();
    }

    public void UpdatePlayerSlot(int slotIndex, string name, bool isMaster, bool isReady) // 특정 플레이어 슬롯 UI를 업데이트하는 함수
    {
        playerSlotContainer?.UpdatePlayerSlot(slotIndex, name, isMaster, isReady);
    }
}
