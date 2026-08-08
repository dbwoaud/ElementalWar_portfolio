using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class UnitSlotContainer : MonoBehaviour, IDropHandler
{
    [Header("UI 요소")]
    [SerializeField] private Button backButton;

    [Header("유닛 슬롯 관리")]
    [SerializeField] private UnitSlotItem[] unitSlots;

    public event Action<UnitStat> OnUnitLeftClicked; // 마우스 왼쪽 버튼 클릭 이벤트
    public event Action<UnitSlotItem> OnBeginDragEvent; // 마우스 드래그 시작 이벤트
    public event Action<PointerEventData> OnDragEvent; // 마우스 드래그 진행 이벤트
    public event Action OnEndDragEvent; // 마우스 드래그 완료 이벤트
    public event Action<int> OnSlotUnequipped; // 유닛 장착 해제 시 실행되는 이벤트
    public event Action OnBackButtonClicked; // 뒤로가기 버튼 클릭 이벤트


    private void OnDestroy()
    {
        backButton?.onClick.RemoveListener(HandleBackButtonClicked);
        if (unitSlots == null)
            return;

        for (int i = 0; i < unitSlots.Length; i++)
        {
            if (unitSlots[i] == null)
                continue;

            unitSlots[i].OnLeftClicked -= HandleUnitLeftClicked;
            unitSlots[i].OnBeginDragEvent -= HandleBeginDrag;
            unitSlots[i].OnDragEvent -= HandleDrag;
            unitSlots[i].OnEndDragEvent -= HandleEndDrag;
        }
    }

    public void InitializeSlotContainer() // 슬롯 컨테이너를 초기화하는 함수
    {
        backButton?.onClick.AddListener(HandleBackButtonClicked);
        if (unitSlots == null)
            return;

        for (int i = 0; i< unitSlots.Length; i++) 
        {
            if (unitSlots[i] == null)
                continue;

            unitSlots[i].OnLeftClicked += HandleUnitLeftClicked;
            unitSlots[i].OnBeginDragEvent += HandleBeginDrag;
            unitSlots[i].OnDragEvent += HandleDrag;
            unitSlots[i].OnEndDragEvent += HandleEndDrag;
        }
    }

    private void HandleBackButtonClicked() // 뒤로가기 버튼 클릭 시 실행되는 함수
    {
        OnBackButtonClicked?.Invoke();
    }

    private void HandleUnitLeftClicked(UnitStat stat) // 유닛 슬롯에 마우스 왼쪽 버튼 클릭 시 실행되는 함수
    { 
        OnUnitLeftClicked?.Invoke(stat); 
    }

    private void HandleBeginDrag(UnitSlotItem item) // 유닛 슬롯에 마우스 드래그 시작 시 실행되는 함수
    { 
        OnBeginDragEvent?.Invoke(item); 
    }

    private void HandleDrag(PointerEventData data) // 유닛 슬롯에 마우스 드래그 진행 시 실행되는 함수
    { 
        OnDragEvent?.Invoke(data); 
    }

    private void HandleEndDrag() // 유닛 슬롯에 마우스 드래그 완료 시 실행되는 함수
    { 
        OnEndDragEvent?.Invoke(); 
    }

    public void UpdateUnitSlotList(List<UnitStat> elementUnits) // 속성에 맞는 유닛 슬롯 UI로 업데이트하는 함수
    {
        for (int i = 0; i < unitSlots.Length; i++)
        {
            if (i < elementUnits.Count)
                unitSlots[i].Setup(elementUnits[i]);
            else
                unitSlots[i].gameObject.SetActive(false);
        }
    }

    public void UpdateUnitSlotState(List<UnitStat> equippedUnits, UnitStat selectedUnit) // 유닛 슬롯 상태를 업데이트하는 함수
    {
        foreach (var slot in unitSlots)
        {
            if (!slot.gameObject.activeSelf || slot.AssignedUnit == null) 
                continue;

            bool isEquipped = equippedUnits.Contains(slot.AssignedUnit);
            slot.SetEquippedState(isEquipped);
            slot.SetSelectedState(slot.AssignedUnit == selectedUnit);
        }
    }

    public void OnDrop(PointerEventData eventData) // 마우스 드롭 시 실행되는 함수
    {
        DeckSlotItem draggedSlot = eventData.pointerDrag?.GetComponent<DeckSlotItem>();
        if (draggedSlot != null && draggedSlot.AssignedUnit != null)
            OnSlotUnequipped?.Invoke(draggedSlot.slotIndex);
    }
}