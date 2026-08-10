using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using System;

public class DeckSlotItem : MonoBehaviour, IDropHandler, IBeginDragHandler, IDragHandler, IEndDragHandler
{   
    [Header("UI 요소")]
    [SerializeField] private GameObject slotNumTextObj;
    [SerializeField] private GameObject unitInfoObj;
    [SerializeField] private Image unitIconImage;
    [SerializeField] private Text unitCostText;

    [Header("슬롯 관리")]
    [SerializeField] public int slotIndex;
    [SerializeField] private UnitStat assignedUnit;
    public UnitStat AssignedUnit => assignedUnit;

    public event Action<int, UnitStat> OnSlotDropped; // 마우스 드롭 이벤트 
    public event Action<DeckSlotItem> OnBeginDragEvent; // 마우스 드래그 시작 이벤트
    public event Action<PointerEventData> OnDragEvent; // 마우스 드래그 진행 이벤트
    public event Action<DeckSlotItem, PointerEventData> OnEndDragEvent; // 마우스 드래그 완료 이벤트
    public event Action<int, int> OnSlotSwapped; // 슬롯 교체 시 실행되는 이벤트


    public void UpdateUI(UnitStat stat) // 덱 슬롯에 유닛 정보를 업데이트하는 함수  
    {
        assignedUnit = stat;
        if (stat == null)
            SetEmptySlotItem();
        else
            SetUnitSlotItem(stat);
    }

    private void SetEmptySlotItem() // 덱 슬롯을 빈 슬롯으로 설정하는 함수
    {
        slotNumTextObj?.SetActive(true);
        unitInfoObj?.SetActive(false);
    }

    private void SetUnitSlotItem(UnitStat stat) // 덱 슬롯을 유닛 정보로 설정하는 함수
    {
        if (stat == null)
            return;

        slotNumTextObj?.SetActive(false);
        unitInfoObj?.SetActive(true);
        PutUnitInfoInDeckSlot(stat);
    }

    private void PutUnitInfoInDeckSlot(UnitStat stat) // 덱 슬롯에 유닛 정보를 넣는 함수
    {
        if (unitIconImage != null)
            unitIconImage.sprite = stat.UnitIcon;

        if (unitCostText != null)
            unitCostText.text = GameSystem.Cost.GetUnitCostText(stat.SpawnCost);
    }

    public void OnBeginDrag(PointerEventData eventData) // 마우스 드래그 시작 시 실행되는 함수
    {
        if (assignedUnit == null) 
            return;

        OnBeginDragEvent?.Invoke(this);
    }

    public void OnDrag(PointerEventData eventData) // 마우스 드래그 진행 시 실행되는 함수
    {
        if (assignedUnit == null) 
            return;

        OnDragEvent?.Invoke(eventData);
    }

    public void OnEndDrag(PointerEventData eventData) // 마우스 드래그 완료 시 실행되는 함수
    {
        OnEndDragEvent?.Invoke(this, eventData);
    }

    public void OnDrop(PointerEventData eventData) // 마우스 드롭 시 실행되는 함수
    {
        GameObject draggedObj = eventData.pointerDrag;
        if (draggedObj == null)
            return;

        UnitSlotItem unitSlotItem = draggedObj.GetComponent<UnitSlotItem>();
        DeckSlotItem deckSlotItem = draggedObj.GetComponent<DeckSlotItem>();

        if (CheckDropUnitSlotItem(unitSlotItem))
            OnDropUnitSlotItem(unitSlotItem);
        
        else if (CheckDropDeckSlotItem(deckSlotItem))
            OnDropDeckSlotItem(deckSlotItem);
    }

    private bool CheckDropUnitSlotItem(UnitSlotItem unitSlotItem) // 드롭 오브젝트가 유닛 슬롯인지 확인하는 함수
    {
        return unitSlotItem != null && unitSlotItem.AssignedUnit != null;
    }

    private void OnDropUnitSlotItem(UnitSlotItem draggedItem) // 유닛 슬롯 드롭 시 실행되는 함수
    {
        OnSlotDropped?.Invoke(slotIndex, draggedItem.AssignedUnit);
    }

    private bool CheckDropDeckSlotItem(DeckSlotItem deckSlotItem) // 드롭한 오브젝트가 덱 슬롯인지 확인하는 함수
    {
        return deckSlotItem != null && deckSlotItem != this;
    }

    private void OnDropDeckSlotItem(DeckSlotItem draggedItem) // 덱 슬롯 드롭 시 실행되는 함수
    {
        OnSlotSwapped?.Invoke(draggedItem.slotIndex, this.slotIndex);
    }
}