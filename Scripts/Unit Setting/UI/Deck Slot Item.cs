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

    public event Action<int, UnitStat> OnSlotDropped;
    public event Action<DeckSlotItem> OnBeginDragEvent;
    public event Action<PointerEventData> OnDragEvent;
    public event Action<DeckSlotItem, PointerEventData> OnEndDragEvent;
    public event Action<int, int> OnSlotSwapped;


    public void UpdateUI(UnitStat stat) // 슬롯 UI에 유닛 정보를 업데이트하는 함수  
    {
        assignedUnit = stat;
        if (stat == null)
            SetEmptySlotItem();
        else
            SetUnitSlotItem(stat);
    }

    private void SetEmptySlotItem() // 빈 슬롯 아이템의 UI를 설정하는 함수
    {
        slotNumTextObj?.SetActive(true);
        unitInfoObj?.SetActive(false);
    }

    private void SetUnitSlotItem(UnitStat stat) // 유닛 슬롯 아이템의 UI를 설정하는 함수
    {
        if (stat == null)
            return;

        slotNumTextObj?.SetActive(false);
        unitInfoObj?.SetActive(true);
        SetUnitInfoInDeckSlot(stat);
    }

    private void SetUnitInfoInDeckSlot(UnitStat stat) // 덱 슬롯 안에 유닛 정보를 설정하는 함수
    {
        if (unitIconImage != null)
            unitIconImage.sprite = stat.unitIcon;
        if (unitCostText != null)
            unitCostText.text = GameSystem.Cost.GetUnitCostText(stat.spawnCost);
    }

    public void OnBeginDrag(PointerEventData eventData) // 드래그 시작 시 실행되는 함수
    {
        if (assignedUnit == null) 
            return;

        OnBeginDragEvent?.Invoke(this);
    }

    public void OnDrag(PointerEventData eventData) // 드래그 진행 시 실행되는 함수
    {
        if (assignedUnit == null) 
            return;

        OnDragEvent?.Invoke(eventData);
    }

    public void OnEndDrag(PointerEventData eventData) // 드래그 완료 시 실행되는 함수
    {
        OnEndDragEvent?.Invoke(this, eventData);
    }

    public void OnDrop(PointerEventData eventData) // 마우스를 드롭했을 때 실행되는 함수
    {
        GameObject draggedObj = eventData.pointerDrag;
        if (draggedObj == null)
            return;

        UnitSlotItem unitSlotItem = draggedObj.GetComponent<UnitSlotItem>();
        DeckSlotItem slotItem = draggedObj.GetComponent<DeckSlotItem>();

        if (CheckDropUnitSlotItem(unitSlotItem))
            OnDropUnitSlotItem(unitSlotItem);
        
        else if (CheckDropDeckSlotItem(slotItem))
            OnDropDeckSlotItem(slotItem);
    }

    private bool CheckDropUnitSlotItem(UnitSlotItem unitSlotItem) // 드롭한 객체가 유닛 슬롯 아이템인지 확인하는 함수
    {
        return unitSlotItem != null && unitSlotItem.AssignedUnit != null;
    }

    private void OnDropUnitSlotItem(UnitSlotItem draggedItem) // 유닛 슬롯 아이템을 드롭했을 때 실행되는 함수
    {
        OnSlotDropped?.Invoke(slotIndex, draggedItem.AssignedUnit);
    }

    private bool CheckDropDeckSlotItem(DeckSlotItem slotItem) // 드롭한 객체가 덱 슬롯 아이템인지 확인하는 함수
    {
        return slotItem != null && slotItem != this;
    }

    private void OnDropDeckSlotItem(DeckSlotItem draggedItem) // 덱 슬롯 아이템을 드롭했을 때 실행되는 함수
    {
        OnSlotSwapped?.Invoke(draggedItem.slotIndex, this.slotIndex);
    }
}