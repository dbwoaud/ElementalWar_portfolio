using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System;
using System.Collections;


public enum SlotState { Active, Inactive, CoolTime }

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
    public bool IsActive => currentState == SlotState.Active;
    public bool IsInCoolTime => currentState == SlotState.CoolTime;

    [Header("쿨타임 코루틴")]
    [SerializeField] private Coroutine coolTimeCoroutine;

    public event Action<int, UnitStat> OnUnitSlotClicked;


    public void SetupSlot(int index) // 슬롯의 인덱스와 클릭 콜백을 등록하는 함수
    {
        slotIndex = index;
        if(assignedUnit == null)
            UpdateUI(null);
    }

    public void UpdateUI(UnitStat stat) // 유닛 정보를 슬롯에 표시하는 함수
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

    private void ChangeState(SlotState newState) // 슬롯의 상태를 변화시키는 함수
    {
        currentState = newState;
        darkOverlayImage.gameObject.SetActive(!IsActive);
        if (!IsActive)
            darkOverlayImage.fillAmount = 1f;
    }

    public void OnPointerClick(PointerEventData eventData) // 마우스 클릭 시 실행되는 함수
    {
        if (assignedUnit == null || !IsActive) 
            return;

        if (eventData.button != PointerEventData.InputButton.Left)
            return;

        OnUnitSlotClicked?.Invoke(slotIndex, assignedUnit);
    }

    public void StartCoolTime() // 유닛 쿨타임 연출을 시작하는 함수
    {
        if (IsInCoolTime) 
            return;

        if (coolTimeCoroutine != null)
            StopCoroutine(coolTimeCoroutine);

        coolTimeCoroutine = StartCoroutine(CoolTimeCoroutine());
    }

    private IEnumerator CoolTimeCoroutine() // 유닛 쿨타임 연출을 시작하는 코루틴
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

        SoundManager.instance?.Play(SoundKey.UnitCoolTimeEnd);
        ChangeState(SlotState.Inactive);

        if (EnergyManager.instance != null)
            EvaluateEnergyState(EnergyManager.instance.CurrentEnergy);
    }

    public void EvaluateEnergyState(float currentEnergy) // 현재 에너지의 상태를 평가하는 함수
    {
        if (assignedUnit == null || IsInCoolTime) 
            return;

        bool hasEnoughEnergy = currentEnergy >= assignedUnit.spawnCost;
        ChangeState(hasEnoughEnergy ? SlotState.Active : SlotState.Inactive);
    }
}
