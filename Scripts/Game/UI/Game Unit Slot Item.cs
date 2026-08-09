using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System;
using System.Collections;

public enum SlotState 
{ 
    Active, // 활성화
    Inactive, // 비활성화
    CoolTime // 쿨타임
}

public class GameUnitSlotItem : MonoBehaviour, IPointerClickHandler
{
    [Header("UI 요소")]
    [SerializeField] private GameObject unitInfoObj;
    [SerializeField] private Image darkOverlayImage;
    [SerializeField] private Image unitIconImage;
    [SerializeField] private Text unitCostText;

    [Header("슬롯 정보 및 상태")]
    [SerializeField] private int slotIndex;
    [SerializeField] private UnitStat assignedUnit;
    [SerializeField] private SlotState currentState = SlotState.Inactive;

    [Header("쿨타임 코루틴")]
    [SerializeField] private Coroutine coolTimeCoroutine;

    public bool IsActive => currentState == SlotState.Active;
    public bool IsInCoolTime => currentState == SlotState.CoolTime;
    public bool IsSpawnable => assignedUnit != null && IsActive;

    public event Action<int, UnitStat> OnUnitSlotClicked; // 게임 유닛 슬롯 클릭 이벤트


    public void SetupSlot(int index) // 게임 유닛 슬롯을 설정하는 함수
    {
        slotIndex = index;
        if(assignedUnit == null)
            SetSlotUI(null);
    }

    public void SetSlotUI(UnitStat stat) // 게임 유닛 슬롯 UI를 설정하는 함수
    {
        assignedUnit = stat;

        bool hasUnit = stat != null;
        unitInfoObj.SetActive(hasUnit);
        darkOverlayImage.gameObject.SetActive(hasUnit);
        if(hasUnit)
        {
            unitIconImage.sprite = stat.unitIcon;
            unitCostText.text = GameSystem.Cost.GetUnitCostText(stat.spawnCost);
        }

        ChangeState(SlotState.Inactive);  
    }

    private void ChangeState(SlotState newState) // 게임 유닛 슬롯의 상태를 변화시키는 함수
    {
        currentState = newState;
        darkOverlayImage.gameObject.SetActive(!IsActive);
        if (!IsActive)
            darkOverlayImage.fillAmount = 1f;
    }

    public void OnPointerClick(PointerEventData eventData) // 게임 유닛 슬롯에 마우스 클릭 시 실행되는 함수
    {
        if (!IsSpawnable) 
            return;

        if (eventData.button != PointerEventData.InputButton.Left)
            return;

        OnUnitSlotClicked?.Invoke(slotIndex, assignedUnit);
    }

    public void StartCoolTime() // 게임 유닛 슬롯의 쿨타임 연출을 시작하는 함수
    {
        if (assignedUnit == null || IsInCoolTime)
            return;

        coolTimeCoroutine = StartCoroutine(StartCoolTimeCoroutine());
    }

    private IEnumerator StartCoolTimeCoroutine() // 게임 유닛 슬롯의 쿨타임 연출을 시작하는 코루틴
    {
        ChangeState(SlotState.CoolTime);

        float timer = 0f;
        float coolTime = assignedUnit.spawnCoolTime;
        while (timer < coolTime)
        {
            timer += Time.deltaTime;
            darkOverlayImage.fillAmount = 1f - (timer / coolTime);
            yield return null;
        }

        SoundManager.Instance?.Play(SoundKey.UnitCoolTimeEnd);
        ChangeState(SlotState.Inactive);

        if (EnergyManager.Instance != null)
            UpdateSlotStateByEnergy(EnergyManager.Instance.CurrentEnergy);
    }

    public void UpdateSlotStateByEnergy(float currentEnergy) // 현재 에너지에 따른 슬롯 상태를 업데이트하는 함수
    {
        if (assignedUnit == null || IsInCoolTime) 
            return;

        bool hasEnergy = currentEnergy >= assignedUnit.spawnCost;
        ChangeState(hasEnergy ? SlotState.Active : SlotState.Inactive);
    }
}
