using System;
using UnityEngine;
using UnityEngine.EventSystems;

public class DeckSlotContainer : MonoBehaviour
{
    [Header("덱 슬롯 관리")]
    [SerializeField] public DeckSlotItem[] slotItems;

    public event Action<int, UnitStat> OnSlotDropped;
    public event Action<int, int> OnSlotSwapped;
    public event Action<DeckSlotItem> OnBeginDragEvent;
    public event Action<PointerEventData> OnDragEvent;
    public event Action<DeckSlotItem, PointerEventData> OnEndDragEvent;


    public void InitializeSlots() // 덱 슬롯 이벤트를 초기화하는 함수
    {
        for (int i = 0; i < slotItems.Length; i++)
        {
            slotItems[i].slotIndex = i;
            slotItems[i].OnSlotDropped += HandleSlotDropped;
            slotItems[i].OnSlotSwapped += HandleSlotSwapped;
            slotItems[i].OnBeginDragEvent += HandleBeginDrag;
            slotItems[i].OnDragEvent += HandleDrag;
            slotItems[i].OnEndDragEvent += HandleEndDrag;
            slotItems[i].UpdateUI(null);
        }
    }

    private void HandleSlotDropped(int index, UnitStat stat) // 덱 슬롯에 드롭할 때 실행되는 함수
    { 
        OnSlotDropped?.Invoke(index, stat); 
    }

    private void HandleSlotSwapped(int from, int to) // 덱 슬롯을 스왑할 때 실행되는 함수
    { 
        OnSlotSwapped?.Invoke(from, to); 
    }

    private void HandleBeginDrag(DeckSlotItem item) // 드래그 시작 시 실행되는 함수
    { 
        OnBeginDragEvent?.Invoke(item); 
    }

    private void HandleDrag(PointerEventData data) // 드래그 진행 시 실행되는 함수
    {
        OnDragEvent?.Invoke(data); 
    }

    private void HandleEndDrag(DeckSlotItem item, PointerEventData data) // 드래그 완료 시 실행되는 함수
    { 
        OnEndDragEvent?.Invoke(item, data); 
    }

    public void UpdateSlotUI(int index, UnitStat stat) // 슬롯 UI를 업데이트하는 함수
    {
        if (CheckValidIndex(index))
            slotItems[index].UpdateUI(stat);
    }

    private bool CheckValidIndex(int index) // 유효한 인덱스인지 확인하는 함수
    {
        return index >= 0 && index < slotItems.Length;
    }

    private void OnDestroy()
    {
        if (slotItems == null) 
            return;
        for (int i = 0; i < slotItems.Length; i++)
        {
            if (slotItems[i] == null) 
                continue;
            slotItems[i].OnSlotDropped -= HandleSlotDropped;
            slotItems[i].OnSlotSwapped -= HandleSlotSwapped;
            slotItems[i].OnBeginDragEvent -= HandleBeginDrag;
            slotItems[i].OnDragEvent -= HandleDrag;
            slotItems[i].OnEndDragEvent -= HandleEndDrag;
        }
    }
}