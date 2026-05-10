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

    [Header("랜덤 맵 결정용 풀 길이")]
    [SerializeField] private int totalMapCount = 5;

    [Header("유닛 및 덱 설정 변수")]
    [SerializeField] private DeckModel deck;
    [SerializeField] private UnitStat currentSelectedUnit;

    [Header("씬 전환 변수")]
    [SerializeField] private bool transitionLocked = false;


    protected override void SetCachedVariable()
    {
        deck = new DeckModel();
        base.SetCachedVariable();
    }

    protected override void SubscribeEvents()
    {
        base.SubscribeEvents();
        if (deckHotkeyHandler != null)
            deckHotkeyHandler.OnSlotHotkeyPressed += HandleHotkeyAssignSlot;
    }

    protected override void SetUIManager()
    {
        if (UnitSettingUIManager.instance != null)
        {
            unitSettingUIManager = UnitSettingUIManager.instance;
            unitSettingUIManager.OnElementButtonClicked += HandleElementSelected;
            unitSettingUIManager.OnReadyButtonClicked += HandleReadyCheck;
            unitSettingUIManager.OnDeckSlotDropped += HandleAssignUnitToDeck;
            unitSettingUIManager.OnDeckSlotSwapped += HandleSwapUnitsInDeck;
            unitSettingUIManager.OnUnitUnequipped += RemoveUnitFromDeck;
            unitSettingUIManager.OnUnitSlotClicked += HandleUnitLeftClick;
        }
    }

    protected override void SetNetworkManager()
    {
        if (unitSettingNetworkManager != null)
        {
            unitSettingNetworkManager.OnBothPlayersReady += HandleGameStart;
            unitSettingNetworkManager.OnOpponentLeftRoom += HandleOpponentLeft;
        }
    }

    protected override void ResetUIManager()
    {
        if (unitSettingUIManager != null)
        {
            unitSettingUIManager.OnElementButtonClicked -= HandleElementSelected;
            unitSettingUIManager.OnReadyButtonClicked -= HandleReadyCheck;
            unitSettingUIManager.OnDeckSlotDropped -= HandleAssignUnitToDeck;
            unitSettingUIManager.OnDeckSlotSwapped -= HandleSwapUnitsInDeck;
            unitSettingUIManager.OnUnitUnequipped -= RemoveUnitFromDeck;
            unitSettingUIManager.OnUnitSlotClicked -= HandleUnitLeftClick;
        }
    }

    protected override void ResetNetworkManager()
    {
        if (unitSettingNetworkManager != null)
        {
            unitSettingNetworkManager.OnBothPlayersReady -= HandleGameStart;
            unitSettingNetworkManager.OnOpponentLeftRoom -= HandleOpponentLeft;
        }
    }

    protected override void PlayBGM()
    {
        SoundManager.instance?.StopAll();
        SoundManager.instance?.Play(SoundKey.UnitSettingBGM);
    }

    protected override void InitializeState() 
    {
        unitSettingNetworkManager?.ResetPlayerReadyState();
    }

    private void HandleElementSelected(ElementType type) // 속성 버튼 클릭 시 실행되는 함수
    {
        if (unitDatabase == null)
            return;
        
        List<UnitStat> filteredUnits = new List<UnitStat>(unitDatabase.FindByElement(type));
        filteredUnits.Sort((a, b) => a.spawnCost.CompareTo(b.spawnCost));
        unitSettingUIManager?.UpdateUnitSlotList(filteredUnits);
        RefreshDeckUI();
    }

    private void HandleReadyCheck() // 덱의 준비 완료 상태를 확인하는 함수
    {
        if (!deck.IsFull())
        {
            PopupPanelUIManager.instance?.ShowError(PopupMessage.Error.NeedDeckFull, null);
            return;
        }
            
        PopupPanelUIManager.instance?.ShowWaiting(PopupMessage.Waiting.WaitingOpponent, HandleCancelDeckReady);
        unitSettingNetworkManager?.SetPlayerReadyState(deck.GetUnitNames());
    }

    private void HandleCancelDeckReady() // 덱의 준비 완료 상태를 취소하는 함수
    {
        unitSettingNetworkManager?.ResetPlayerReadyState();
    }

    private void HandleAssignUnitToDeck(int slotIndex, UnitStat stat) // 유닛을 덱에 할당하는 함수
    {
        if (stat == null)
            return;

        SoundManager.instance?.Play(SoundKey.ButtonClick);

        int existingIndex = deck.FindUnitIndex(stat);
        if (existingIndex != -1)
            RemoveUnitFromDeck(existingIndex);

        deck.SetUnit(slotIndex, stat);
        unitSettingUIManager?.UpdateDeckSlotUI(slotIndex, stat);

        currentSelectedUnit = null;
        RefreshDeckUI();
    }

    private void RemoveUnitFromDeck(int slotIndex) // 인덱스에 해당하는 덱 유닛을 제거하는 함수
    {
        deck.RemoveUnit(slotIndex);
        unitSettingUIManager?.UpdateDeckSlotUI(slotIndex, null);
        RefreshDeckUI();
    }

    private void RefreshDeckUI() // 덱 슬롯 UI를 갱신하는 함수
    {
        List<UnitStat> equippedUnits = new List<UnitStat>(InputBindings.DeckSize);
        for (int i = 0; i < InputBindings.DeckSize; i++)
        {
            UnitStat stat = deck.GetUnit(i);
            if (stat != null)
                equippedUnits.Add(stat);
        }
        unitSettingUIManager?.RefreshUnitSlotState(equippedUnits, currentSelectedUnit);
    }

    private void HandleSwapUnitsInDeck(int fromIndex, int toIndex) // 덱의 슬롯을 서로 바꾸는 함수
    {
        SoundManager.instance?.Play(SoundKey.ButtonClick);
        deck.SwapUnits(fromIndex, toIndex);
        unitSettingUIManager?.UpdateDeckSlotUI(fromIndex, deck.GetUnit(fromIndex));
        unitSettingUIManager?.UpdateDeckSlotUI(toIndex, deck.GetUnit(toIndex));
    }

    private void HandleUnitLeftClick(UnitStat stat) // 유닛 슬롯의 왼쪽 클릭을 처리하는 함수
    {
        currentSelectedUnit = stat;
        RefreshDeckUI();
    }

    private void HandleGameStart() // 게임 시작을 처리하는 함수
    {
        if (transitionLocked) 
            return;
        transitionLocked = true;

        PopupPanelUIManager.instance?.HideWaiting();
        if (!PhotonNetwork.IsMasterClient)
            return;
        
        DecideMap();
        CloseRoom();
        PhotonNetwork.LoadLevel(SceneName.Game);
    }

    private void DecideMap() // 맵을 랜덤으로 결정하는 함수
    {
        int randomIndex = Random.Range(0, Mathf.Max(1, totalMapCount));
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

    private void HandleOpponentLeft(Photon.Realtime.Player leftPlayer) // 상대방의 탈주를 처리하는 함수
    {
        if (transitionLocked) 
            return;
        transitionLocked = true;

        PopupPanelUIManager.instance?.HideWaiting();
        PopupPanelUIManager.instance?.ShowError
        (
            PopupMessage.Error.OpponentLeft,
            HandleReturnToRoom
        );
    }

    private void HandleReturnToRoom() // 방으로 이동하는 함수
    {
        unitSettingNetworkManager?.ResetPlayerReadyState();
        PhotonNetwork.LoadLevel(SceneName.Room);
    }

    private void HandleHotkeyAssignSlot(int slotIndex) // 키보드 숫자키 입력으로 현재 선택된 유닛을 슬롯에 할당하는 함수
    {
        if (currentSelectedUnit == null)
            return;

        HandleAssignUnitToDeck(slotIndex, currentSelectedUnit);
    }

    protected override void UnsubscribeAll()
    {
        base.UnsubscribeAll();
        if (deckHotkeyHandler != null)
            deckHotkeyHandler.OnSlotHotkeyPressed -= HandleHotkeyAssignSlot;
    }
}