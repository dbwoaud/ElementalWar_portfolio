using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System;
using System.Collections.Generic;


public class UnitSettingUIManager : BaseUIManager<UnitSettingUIManager>
{
    [Header("UI 설정")]
    [SerializeField] private Button voidButton;
    [SerializeField] private Button windButton;
    [SerializeField] private Button forestButton;
    [SerializeField] private Button fireButton; 
    [SerializeField] private Button mountainButton;
    [SerializeField] private Button readyButton;

    [Header("하위 컨테이너 관리")]
    [SerializeField] private DeckSlotContainer slotContainer;
    [SerializeField] private UnitSlotContainer unitSlotContainer;
    [SerializeField] private GameObject elementalButtonContainer;

    [Header("드래그 고스트 연출")]
    [SerializeField] private GameObject dragGhostObj;
    [SerializeField] private Image ghostImage;
    [SerializeField] private Text ghostCostText;
    [SerializeField] private Canvas mainCanvas;

    public event Action<ElementType> OnElementButtonClicked;
    public event Action OnReadyButtonClicked;
    public event Action<int, UnitStat> OnDeckSlotDropped;
    public event Action<int, int> OnDeckSlotSwapped;
    public event Action<int> OnUnitUnequipped;
    public event Action<UnitStat> OnUnitSlotClicked;


    protected override void InitUIElements()
    {
        mainCanvas = GetComponent<Canvas>();
        unitSlotContainer?.gameObject.SetActive(false);
        elementalButtonContainer?.SetActive(true);
        slotContainer.InitializeSlots();
        unitSlotContainer.InitializeSlots();
        InitDragGhost();
    }

    private void InitDragGhost() // 드래그 연출 설정을 초기화하는 함수
    {
        dragGhostObj?.SetActive(false);
        ghostImage.raycastTarget = false;

        CanvasGroup dragGhostCanvasGroup = dragGhostObj?.GetComponent<CanvasGroup>();
        if (dragGhostCanvasGroup == null)
            dragGhostCanvasGroup = dragGhostObj?.AddComponent<CanvasGroup>();

        dragGhostCanvasGroup.blocksRaycasts = false;
        dragGhostCanvasGroup.interactable = false;
    }

    protected override void BindButtonEvent()
    {
        voidButton?.onClick.AddListener(OnClickVoidButton);
        windButton?.onClick.AddListener(OnClickWindButton);
        forestButton?.onClick.AddListener(OnClickForestButton);
        fireButton?.onClick.AddListener(OnClickFireButton);
        mountainButton?.onClick.AddListener(OnClickMountainButton);
        readyButton?.onClick.AddListener(OnClickReadyButton);
    }

    protected override void BindPanelEvent()
    {
        if(slotContainer != null)
        {
            slotContainer.OnSlotDropped += HandleDeckSlotDropped;
            slotContainer.OnSlotSwapped += HandleDeckSlotSwapped;
            slotContainer.OnBeginDragEvent += ShowDragGhost;
            slotContainer.OnDragEvent += HandleMoveDragGhost;
            slotContainer.OnEndDragEvent += HandleSlotEndDrag;
        }

        if(unitSlotContainer != null)
        {
            unitSlotContainer.OnUnitLeftClicked += HandleUnitSlotClicked;
            unitSlotContainer.OnBeginDragEvent += ShowDragGhost;
            unitSlotContainer.OnDragEvent += HandleMoveDragGhost;
            unitSlotContainer.OnEndDragEvent += HideDragGhost;
            unitSlotContainer.OnSlotUnequipped += HandleUnitUnequipped;
            unitSlotContainer.OnBackButtonClicked += OnClickBackButton;
        }
    }

    protected override void UnbindButtonEvent()
    {
        voidButton?.onClick.RemoveListener(OnClickVoidButton);
        windButton?.onClick.RemoveListener(OnClickWindButton);
        forestButton?.onClick.RemoveListener(OnClickForestButton);
        fireButton?.onClick.RemoveListener(OnClickFireButton);
        mountainButton?.onClick.RemoveListener(OnClickMountainButton);
        readyButton?.onClick.RemoveListener(OnClickReadyButton);
    }

    protected override void UnbindPanelEvent()
    {
        if(slotContainer != null)
        {
            slotContainer.OnSlotDropped -= HandleDeckSlotDropped;
            slotContainer.OnSlotSwapped -= HandleDeckSlotSwapped;
            slotContainer.OnBeginDragEvent -= ShowDragGhost;
            slotContainer.OnDragEvent -= HandleMoveDragGhost;
            slotContainer.OnEndDragEvent -= HandleSlotEndDrag;
        }
 
        if(unitSlotContainer != null)
        {
            unitSlotContainer.OnUnitLeftClicked -= HandleUnitSlotClicked;
            unitSlotContainer.OnBeginDragEvent -= ShowDragGhost;
            unitSlotContainer.OnDragEvent -= HandleMoveDragGhost;
            unitSlotContainer.OnEndDragEvent -= HideDragGhost;
            unitSlotContainer.OnSlotUnequipped -= HandleUnitUnequipped;
            unitSlotContainer.OnBackButtonClicked -= OnClickBackButton;
        }
    }


    private void OnClickVoidButton() // 무속성 유닛 버튼을 클릭 시 실행되는 함수
    {
        PlayButtonSound(); 
        ShowUnitContainer(ElementType.Void);
    }

    private void OnClickWindButton() // 풍속성 유닛 버튼을 클릭 시 실행되는 함수
    {
        PlayButtonSound(); 
        ShowUnitContainer(ElementType.Wind);
    }

    private void OnClickForestButton() // 림속성 유닛 버튼을 클릭 시 실행되는 함수
    {
        PlayButtonSound(); 
        ShowUnitContainer(ElementType.Forest);
    }

    private void OnClickFireButton() // 화속성 유닛 버튼을 클릭 시 실행되는 함수
    {
        PlayButtonSound(); 
        ShowUnitContainer(ElementType.Fire);
    }

    private void OnClickMountainButton() // 산속성 유닛 버튼을 클릭 시 실행되는 함수
    {
        PlayButtonSound(); 
        ShowUnitContainer(ElementType.Mountain);
    }

    private void OnClickBackButton() // 뒤로 가기 버튼 클릭 시 실행되는 함수
    {
        PlayButtonSound();
        unitSlotContainer?.gameObject.SetActive(false);
        elementalButtonContainer?.SetActive(true);
    }

    private void OnClickReadyButton() // 준비 완료 버튼 클릭 시 실행되는 함수
    {
        PlayButtonSound(); 
        OnReadyButtonClicked?.Invoke(); 
    }

    private void ShowUnitContainer(ElementType type) // 속성 별 유닛을 불러오는 함수
    {
        elementalButtonContainer?.SetActive(false);
        unitSlotContainer?.gameObject.SetActive(true);
        OnElementButtonClicked?.Invoke(type);
    }

    private void HandleDeckSlotDropped(int index, UnitStat stat) // 덱 슬롯에 유닛 슬롯을 드롭했을 때 실행되는 함수
    { 
        OnDeckSlotDropped?.Invoke(index, stat); 
    }

    private void HandleDeckSlotSwapped(int from, int to) // 덱 슬롯을 스왑했을 때 실행되는 함수
    { 
        OnDeckSlotSwapped?.Invoke(from, to); 
    }

    public void ShowDragGhost(UnitSlotItem unitSlotItem) // 유닛 슬롯의 드래그 연출을 보여주는 함수
    {
        dragGhostObj?.SetActive(true);
        dragGhostObj?.transform.SetAsLastSibling();
        SetDragGhostUI(unitSlotItem);
    }

    private void SetDragGhostUI(UnitSlotItem unitSlotItem) // 유닛 슬롯의 드래그 고스트 UI를 설정하는 함수
    {
        if (ghostImage != null)
            ghostImage.sprite = unitSlotItem.AssignedUnit.unitIcon;
        if (ghostCostText != null)
            ghostCostText.text = GameSystem.Cost.GetUnitCostText(unitSlotItem.AssignedUnit.spawnCost);
    }

    private void HandleMoveDragGhost(PointerEventData data)  // 드래그 고스트가 이동할 때 실행되는 함수 
    { 
        MoveDragGhost(data, mainCanvas); 
    }

    private void HandleSlotEndDrag(DeckSlotItem slotItem, PointerEventData eventData)  // 덱 슬롯의 드래그 완료 시 실행되는 함수
    {
        HideDragGhost();
        GameObject dropTarget = eventData.pointerCurrentRaycast.gameObject;
        if (dropTarget == null || dropTarget.GetComponentInParent<DeckSlotItem>() == null)
        {
            OnUnitUnequipped?.Invoke(slotItem.slotIndex);
        }
    }

    private void HandleUnitSlotClicked(UnitStat stat) // 유닛 슬롯 클릭 시 실행되는 함수
    {
        OnUnitSlotClicked?.Invoke(stat); 
    }

    public void ShowDragGhost(DeckSlotItem slotItem) // 덱 슬롯의 드래그 연출을 보여주는 함수
    {
        dragGhostObj?.SetActive(true);
        dragGhostObj?.transform.SetAsLastSibling();
        SetDragGhostUI(slotItem);
    }

    private void SetDragGhostUI(DeckSlotItem slotItem) // 덱 슬롯의 드래그 고스트 UI를 설정하는 함수
    {
        if (ghostCostText != null)
            ghostImage.sprite = slotItem.AssignedUnit.unitIcon;
        if (ghostCostText != null)
            ghostCostText.text = GameSystem.Cost.GetUnitCostText(slotItem.AssignedUnit.spawnCost);
    }

    public void MoveDragGhost(PointerEventData eventData, Canvas mainCanvas) // 드래그 고스트 이동을 연출하는 함수
    {
        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            mainCanvas.transform as RectTransform, eventData.position, mainCanvas.worldCamera, out Vector2 localPos);
        dragGhostObj.transform.localPosition = localPos;
    }

    public void HideDragGhost() // 드래그 고스트 종료를 연출하는 함수
    {
        dragGhostObj?.SetActive(false);
    }

    private void HandleUnitUnequipped(int index) // 유닛을 장착 해제할 때 실행되는 함수
    { 
        OnUnitUnequipped?.Invoke(index); 
    }

    public void UpdateUnitSlotList(List<UnitStat> units) // 유닛 슬롯 목록을 업데이트하는 함수
    {
        unitSlotContainer?.UpdateSlots(units);
    }

    public void RefreshUnitSlotState(List<UnitStat> equippedUnits, UnitStat selectedUnit) // 유닛 슬롯 상태를 업데이트하는 함수
    {
        unitSlotContainer?.RefreshState(equippedUnits, selectedUnit);
    }

    public void UpdateDeckSlotUI(int index, UnitStat stat) // 덱 슬롯 UI를 업데이트하는 함수
    {
        slotContainer?.UpdateSlotUI(index, stat);
    }
}