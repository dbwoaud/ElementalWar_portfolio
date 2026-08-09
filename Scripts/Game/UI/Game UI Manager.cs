using UnityEngine;
using System;


public class GameUIManager : BaseUIManager<GameUIManager>
{
    [Header("하위 패널 관리")]
    [SerializeField] private GameStartPanel gameStartPanel;
    [SerializeField] private GameResultPanel gameResultPanel;
    [SerializeField] private GameLoadingPanel gameLoadingPanel;
    [SerializeField] private GameUnitSlotContainer slotContainer;

    public event Action OnReturnToRoomRequested; // 방으로 돌아가기 버튼 클릭 이벤트
    public event Action OnReturnToLobbyRequested; // 로비로 돌아가기 버튼 클릭 이벤트
    public event Action<int, UnitStat> OnUnitSlotClicked; // 게임 유닛 슬롯 버튼 클릭 이벤트


    protected override void InitUIElements() // UI 요소 초기화 함수
    {
        slotContainer?.InitializeSlots();
    }

    protected override void BindButtonEvent() // 버튼 이벤트 할당 함수
    { 

    }

    protected override void BindPanelEvent() // 패널 내부 및 데이터 이벤트 할당 함수
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

    protected override void UnbindButtonEvent() // 버튼 이벤트 해제 함수
    {

    }

    protected override void UnbindPanelEvent() // 패널 내부 및 데이터 이벤트 해제 함수
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

    private void HandleReturnToRoomRequest() // 방으로 돌아가기 요청을 처리하는 함수
    {
        OnReturnToRoomRequested?.Invoke();
    }

    private void HandleReturnToLobbyRequest() // 로비로 돌아가기 요청을 처리하는 함수
    {
        OnReturnToLobbyRequested?.Invoke();
    }

    private void HandleUnitSlotClick(int index, UnitStat stat) // 유닛 슬롯 클릭을 처리하는 함수
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

    public void UpdateLoadingProgress(float normalized) // 로딩 진행도를 업데이트하는 함수
    {
        gameLoadingPanel?.UpdateProgress(normalized);
    }

    public void HideGameLoadingPanel() // 게임 로딩 패널을 비활성화하는 함수
    {
        gameLoadingPanel?.Hide();
    }

    public void SetGameUnitSlotsUI(int index, UnitStat stat) // 게임 유닛 슬롯 UI를 설정하는 함수
    {
        slotContainer?.SetSlotsUI(index, stat);
    }

    public void StartSlotCoolTime(int index) // 게임 유닛 슬롯의 쿨타임 연출을 시작하는 함수
    {
        slotContainer?.StartSlotCoolTime(index);
    }

    public void UpdateSlotStateByEnergy(float currentEnergy) // 현재 에너지에 따른 슬롯 상태를 업데이트하는 함수
    {
        slotContainer?.UpdateSlotStateByEnergy(currentEnergy);
    }

    public bool CheckUnitSpawnable(int index) // 유닛을 소환할 수 있는지 확인하는 함수
    {
        return slotContainer != null && slotContainer.CheckUnitSpawnable(index);
    }
}