using Photon.Pun;
using ExitGames.Client.Photon;
using System.Collections.Generic;
using UnityEngine;

public class UnitSettingManager : BaseSceneController<UnitSettingManager>
{
    [Header("캐싱 변수")]
    [SerializeField] private UnitSettingUIManager unitSettingUIManager;
    [SerializeField] private UnitSettingNetworkManager unitSettingNetworkManager;
    [SerializeField] private DeckHotkeyHandler deckHotkeyHandler;
    [SerializeField] private UnitDatabase unitDatabase;

    [Header("전체 맵 개수")]
    [SerializeField] private int totalMapCount = 5;

    [Header("유닛 및 덱 설정 변수")]
    [SerializeField] private DeckModel deck;
    [SerializeField] private UnitStat currentSelectedUnit;

    [Header("씬 이동 설정 변수")]
    [SerializeField] private bool transitionLocked = false;


    protected override void SetCachedVariable() // 캐싱 변수를 설정하는 함수
    {
        deck = new DeckModel();
        base.SetCachedVariable();
    }

    protected override void SubscribeEvents() // 이벤트를 구독하는 함수
    {
        base.SubscribeEvents();
        if (deckHotkeyHandler != null)
            deckHotkeyHandler.OnSlotHotkeyPressed += HandleHotkeyAssignSlot;
    }

    protected override void SetUIManager() // UI 매니저를 설정하는 함수
    {
        if (UnitSettingUIManager.Instance != null)
        {
            unitSettingUIManager = UnitSettingUIManager.Instance;
            unitSettingUIManager.OnElementButtonClicked += HandleElementSelected;
            unitSettingUIManager.OnReadyButtonClicked += HandleDeckReadyState;
            unitSettingUIManager.OnDeckSlotDropped += HandleAssignUnitToDeck;
            unitSettingUIManager.OnDeckSlotSwapped += HandleSwapUnitsInDeck;
            unitSettingUIManager.OnUnitUnequipped += RemoveUnitFromDeck;
            unitSettingUIManager.OnUnitSlotClicked += HandleUnitLeftClick;
        }
    }

    protected override void SetNetworkManager() // 네트워크 매니저를 설정하는 함수
    {
        if (unitSettingNetworkManager != null)
        {
            unitSettingNetworkManager.OnBothPlayersReady += HandleGameStart;
            unitSettingNetworkManager.OnOpponentLeftRoom += HandleOpponentLeft;
        }
    }

    protected override void PlayBGM() // 씬의 배경음악을 재생하는 함수
    {
        SoundManager.Instance?.StopAll();
        SoundManager.Instance?.Play(SoundKey.UnitSettingBGM);
    }

    protected override void InitializeState() // 씬의 초기상태를 설정하는 함수
    {
        unitSettingNetworkManager?.ResetPlayerReadyState();
    }

    protected override void ResetUIManager() // UI 매니저를 리셋하는 함수
    {
        if (unitSettingUIManager != null)
        {
            unitSettingUIManager.OnElementButtonClicked -= HandleElementSelected;
            unitSettingUIManager.OnReadyButtonClicked -= HandleDeckReadyState;
            unitSettingUIManager.OnDeckSlotDropped -= HandleAssignUnitToDeck;
            unitSettingUIManager.OnDeckSlotSwapped -= HandleSwapUnitsInDeck;
            unitSettingUIManager.OnUnitUnequipped -= RemoveUnitFromDeck;
            unitSettingUIManager.OnUnitSlotClicked -= HandleUnitLeftClick;
        }
    }

    protected override void ResetNetworkManager() // 네트워크 매니저를 리셋하는 함수
    {
        if (unitSettingNetworkManager != null)
        {
            unitSettingNetworkManager.OnBothPlayersReady -= HandleGameStart;
            unitSettingNetworkManager.OnOpponentLeftRoom -= HandleOpponentLeft;
        }
    }

    protected override void UnsubscribeAll()
    {
        base.UnsubscribeAll();
        if (deckHotkeyHandler != null)
            deckHotkeyHandler.OnSlotHotkeyPressed -= HandleHotkeyAssignSlot;
    }

    private void HandleHotkeyAssignSlot(int slotIndex) // 키보드 숫자 키 입력으로 현재 선택된 유닛을 덱 슬롯에 할당하는 함수
    {
        if (currentSelectedUnit == null)
            return;

        HandleAssignUnitToDeck(slotIndex, currentSelectedUnit);
    }

    private void HandleElementSelected(ElementType type) // 속성 선택을 처리하는 함수
    {
        if (unitDatabase == null)
            return;
        
        List<UnitStat> filteredUnits = new(unitDatabase.FindByElement(type));
        unitSettingUIManager?.UpdateUnitSlotList(filteredUnits);
        UpdateDeckSlot();
    }

    private void UpdateDeckSlot() // 덱 슬롯을 업데이트하는 함수
    {
        List<UnitStat> equippedUnits = new List<UnitStat>(InputBindings.DeckSize);
        for (int i = 0; i < InputBindings.DeckSize; i++)
        {
            UnitStat stat = deck.GetUnit(i);
            if (stat != null)
                equippedUnits.Add(stat);
        }
        unitSettingUIManager?.UpdateUnitSlotState(equippedUnits, currentSelectedUnit);
    }

    private void HandleDeckReadyState() // 덱 준비 상태를 처리하는 함수
    {
        if (!deck.IsFull())
        {
            PopupPanelUIManager.Instance?.ShowError(PopupMessage.Error.NeedDeckFull, null);
            return;
        }
            
        PopupPanelUIManager.Instance?.ShowWaiting(PopupMessage.Waiting.WaitingOpponent, HandleCancelDeckReadyState);
        unitSettingNetworkManager?.SetPlayerReadyState(deck.GetUnitNames());
    }

    private void HandleCancelDeckReadyState() // 덱 준비 완료 상태 취소를 처리하는 함수
    {
        unitSettingNetworkManager?.ResetPlayerReadyState();
    }

    private void HandleAssignUnitToDeck(int slotIndex, UnitStat stat) // 덱의 유닛 할당을 처리하는 함수
    {
        if (stat == null)
            return;

        SoundManager.Instance?.Play(SoundKey.ButtonClick);

        /* 유닛이 존재하는 슬롯에 새로운 유닛을 할당하면 기존 유닛 제거 */
        int existingIndex = deck.FindUnitIndex(stat);
        if (existingIndex != -1)
            RemoveUnitFromDeck(existingIndex);

        deck.SetUnit(slotIndex, stat);
        unitSettingUIManager?.UpdateDeckSlotUI(slotIndex, stat);

        currentSelectedUnit = null;
        UpdateDeckSlot();
    }

    private void RemoveUnitFromDeck(int slotIndex) // 덱 특정 인덱스의 유닛을 제거하는 함수
    {
        deck.RemoveUnit(slotIndex);
        unitSettingUIManager?.UpdateDeckSlotUI(slotIndex, null);
        UpdateDeckSlot();
    }

    private void HandleSwapUnitsInDeck(int fromIndex, int toIndex) // 덱 슬롯 교체를 처리하는 함수
    {
        SoundManager.Instance?.Play(SoundKey.ButtonClick);
        deck.SwapUnits(fromIndex, toIndex);
        unitSettingUIManager?.UpdateDeckSlotUI(fromIndex, deck.GetUnit(fromIndex));
        unitSettingUIManager?.UpdateDeckSlotUI(toIndex, deck.GetUnit(toIndex));
    }

    private void HandleUnitLeftClick(UnitStat stat) // 유닛 슬롯의 마우스 왼쪽 클릭을 처리하는 함수
    {
        currentSelectedUnit = stat;
        UpdateDeckSlot();
    }

    private void HandleGameStart() // 게임 시작을 처리하는 함수
    {
        if (transitionLocked) 
            return;

        transitionLocked = true;

        PopupPanelUIManager.Instance?.HideWaiting();
        if (!PhotonNetwork.IsMasterClient)
            return;
        
        DecideMap();
        CloseRoom();
        PhotonNetwork.LoadLevel(SceneName.Game);
    }

    private void DecideMap() // 맵을 랜덤으로 결정하는 함수
    {
        int randomIndex = Random.Range(0, totalMapCount);
        var props = new Hashtable
        {
            { RoomConstants.Properties.MapIndex, randomIndex },
            { RoomConstants.Properties.GameStart, true },
        };
        PhotonNetwork.CurrentRoom.SetCustomProperties(props);
    }

    private void CloseRoom() // 현재 방을 닫는 함수
    {
        PhotonNetwork.CurrentRoom.IsOpen = false;
    }

    private void HandleOpponentLeft(Photon.Realtime.Player leftPlayer) // 상대 플레이어의 탈주를 처리하는 함수
    {
        if (transitionLocked) 
            return;

        transitionLocked = true;

        PopupPanelUIManager.Instance?.HideWaiting();
        PopupPanelUIManager.Instance?.ShowError
        (
            PopupMessage.Error.OpponentLeft,
            HandleReturnToRoom
        );
    }

    private void HandleReturnToRoom() // 방 이동을 처리하는 함수
    {
        unitSettingNetworkManager?.ResetPlayerReadyState();
        PhotonNetwork.LoadLevel(SceneName.Room);
    }
}