using UnityEngine;
using System;


public class GameUIManager : BaseUIManager<GameUIManager>
{
    [Header("하위 패널 관리")]
    [SerializeField] private GameStartPanel gameStartPanel;
    [SerializeField] private GameResultPanel gameResultPanel;
    [SerializeField] private GameLoadingPanel gameLoadingPanel;
    [SerializeField] private GameUnitSlotContainer slotContainer;

    public event Action OnReturnToRoomRequested;
    public event Action OnReturnToLobbyRequested;
    public event Action<int, UnitStat> OnUnitSlotClicked;


    protected override void InitUIElements()
    {
        slotContainer?.InitializeSlots();
    }

    protected override void BindButtonEvent() { }

    protected override void BindPanelEvent()
    {
        if(gameResultPanel != null)
        {
            gameResultPanel.OnReturnToRoomRequested += HandleReturnToRoomRequest;
            gameResultPanel.OnReturnToLobbyRequested += HandleReturnToLobbyRequest;
        }
        if(slotContainer != null)
        {
            slotContainer.OnUnitSlotClicked += HandleUnitSlotClick;
        }

    }

    protected override void UnbindPanelEvent()
    {
        if (gameResultPanel != null)
        {
            gameResultPanel.OnReturnToRoomRequested -= HandleReturnToRoomRequest;
            gameResultPanel.OnReturnToLobbyRequested -= HandleReturnToLobbyRequest;
        }
        if (slotContainer != null)
        {
            slotContainer.OnUnitSlotClicked -= HandleUnitSlotClick;
        }

    }

    private void HandleReturnToRoomRequest() // 방으로 돌아가기 버튼 클릭 시 실행되는 함수
    {
        OnReturnToRoomRequested?.Invoke();
    }

    private void HandleReturnToLobbyRequest() // 로비로 돌아가기 버튼 클릭 시 실행되는 함수
    {
        OnReturnToLobbyRequested?.Invoke();
    }

    private void HandleUnitSlotClick(int index, UnitStat stat) // 유닛 슬롯 클릭 시 실행되는 함수
    {
        OnUnitSlotClicked?.Invoke(index, stat);
    }

    public void ShowGameStartPanel(string player1Name, string player2Name) // 게임 시작 패널을 활성화하는 함수
    {
        gameStartPanel?.SetPlayerNames(player1Name, player2Name);
        gameStartPanel?.Show();
    }

    public void HideGameStartPanel() // 게임 시작 패널을 비활성화하는 함수
    {
        gameStartPanel?.HideImmediate();
    }

    public void ShowGameResultPanel(bool localPlayerWon, string playerName) // 게임 결과 패널을 활성화하는 함수
    {
        gameResultPanel?.DisplayResult(playerName, localPlayerWon);
        gameResultPanel?.Show();
    }

    public void ShowGameLoadingPanel(string message = null) // 게임 로딩 패널을 활성화하는 함수
    {
        gameLoadingPanel?.ShowImmediate(message);
    }

    public void UpdateLoadingProgress(float normalized) // 로딩 진행도를 갱신하는 함수
    {
        gameLoadingPanel?.UpdateProgress(normalized);
    }

    public void HideGameLoadingPanel() // 게임 로딩 패널을 비활성화하는 함수
    {
        gameLoadingPanel?.Hide();
    }

    public void UpdateDeckSlotsUI(int index, UnitStat stat) // 특정 슬롯에 유닛 정보를 표시하는 함수
    {
        slotContainer?.ShowUnitSlot(index, stat);
    }

    public void StartSlotCoolTime(int index) // 슬롯의 쿨타임을 설정하는 함수
    {
        slotContainer?.StartSlotCoolTime(index);
    }

    public void RefreshSlotsEnergyState(float currentEnergy) //  모든 슬롯을 에너지 상태에 맞춰 업데이트하는 함수
    {
        slotContainer?.RefreshSlotsEnergyState(currentEnergy);
    }


}