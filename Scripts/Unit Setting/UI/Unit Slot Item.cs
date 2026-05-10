using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using System;

public class UnitSlotItem : MonoBehaviour, IPointerClickHandler, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    [Header("UI 요소")]
    [SerializeField] private Image unitIconImage;
    [SerializeField] private Text unitCostText;
    [SerializeField] private Text unitNameText;
    [SerializeField] private Text enterText;
    [SerializeField] private GameObject unitDescriptionObj;
    [SerializeField] private Text descriptionText;
    [SerializeField] private GameObject unitSelectionPanel;

    [Header("저장 중인 유닛 정보")]
    [SerializeField] private UnitStat assignedUnit;
    public UnitStat AssignedUnit => assignedUnit;

    public event Action<UnitStat> OnLeftClicked;
    public event Action<UnitSlotItem> OnBeginDragEvent;
    public event Action<PointerEventData> OnDragEvent;
    public event Action OnEndDragEvent;


    public void Setup(UnitStat stat) // 유닛 정보를 유닛 슬롯 아이템에 설정하는 함수
    {
        SetupUI(stat);
        SetEquippedState(false);
        SetSelectedState(false);
        gameObject?.SetActive(true);
    }

    private void SetupUI(UnitStat stat) // 유닛 정보를 UI에 출력하는 함수
    {
        if (stat == null)
            return;

        assignedUnit = stat;

        if(unitIconImage != null)
            unitIconImage.sprite = stat.unitIcon;
        if(unitCostText != null)
            unitCostText.text = GameSystem.Cost.GetUnitCostText(stat.spawnCost);
        if(unitNameText != null)
            unitNameText.text = stat.unitName;
        if(descriptionText != null)
            descriptionText.text = stat.unitDescription;

        unitDescriptionObj?.SetActive(false);
    }

    public void SetEquippedState(bool isEquipped) // 유닛의 장착 상태를 설정하는 함수
    {
        enterText?.gameObject.SetActive(isEquipped);
    }

    public void SetSelectedState(bool isSelected) // 선택된 유닛 슬롯 상태를 설정하는 함수
    {
        unitSelectionPanel?.SetActive(isSelected);
    }

    public void OnPointerClick(PointerEventData eventData) // 마우스 클릭 시 실행되는 함수
    {
        if (assignedUnit == null)
            return;

        SoundManager.instance?.Play(SoundKey.ButtonClick);
        if (eventData.button == PointerEventData.InputButton.Left)
            OnLeftClicked?.Invoke(assignedUnit);
        else if (eventData.button == PointerEventData.InputButton.Right)
            ToggleLocalDescription();
    }

    private void ToggleLocalDescription() // 유닛 설명 패널을 토글하는 함수
    {
        bool isActive = unitDescriptionObj.activeSelf;
        unitDescriptionObj?.SetActive(!isActive);
    }

    public void OnBeginDrag(PointerEventData eventData) // 유닛 슬롯 아이템을 드래그 시작 시 실행되는 함수
    {
        if (assignedUnit == null) 
            return;

        OnBeginDragEvent?.Invoke(this);
    }

    public void OnDrag(PointerEventData eventData) // 유닛 슬롯 아이템을 드래그 진행 시 실행되는 함수
    {
        if (assignedUnit == null) 
            return;

        OnDragEvent?.Invoke(eventData);
    }

    public void OnEndDrag(PointerEventData eventData) // 유닛 슬롯 아이템을 드래그 완료 시 실행되는 함수
    {
        if (assignedUnit == null)
            return;

        OnEndDragEvent?.Invoke();
    }
}
