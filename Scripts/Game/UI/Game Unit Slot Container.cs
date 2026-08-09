using System;
using UnityEngine;

public class GameUnitSlotContainer : MonoBehaviour
{
    [Header("게임 유닛 슬롯 배열")]
    [SerializeField] private GameUnitSlotItem[] gameSlots;

    public event Action<int, UnitStat> OnUnitSlotClicked; // 게임 유닛 슬롯 클릭 이벤트


    private void OnDestroy()
    {
        if (gameSlots != null)
        {
            for (int i = 0; i < gameSlots.Length; i++)
            {
                if (gameSlots[i] != null)
                    gameSlots[i].OnUnitSlotClicked -= HandleSlotClick;
            }
        }
    }

    public void InitializeSlots() // 게임 유닛 슬롯을 초기화하는 함수
    {
        for (int i = 0; i < gameSlots.Length; i++)
        {
            gameSlots[i].SetupSlot(i);
            gameSlots[i].OnUnitSlotClicked += HandleSlotClick;
        }
    }

    private void HandleSlotClick(int index, UnitStat stat) // 게임 유닛 슬롯 클릭을 처리하는 함수
    {
        OnUnitSlotClicked?.Invoke(index, stat);
    }

    public void SetSlotsUI(int index, UnitStat stat) // 게임 유닛 슬롯 UI를 설정하는 함수
    {
        if (IsValidIndex(index))
            gameSlots[index].SetSlotUI(stat);
    }

    public void StartSlotCoolTime(int index) // 게임 유닛 슬롯의 쿨타임 연출을 시작하는 함수
    {
        if (IsValidIndex(index))
            gameSlots[index].StartCoolTime();
    }

    private bool IsValidIndex(int index) // 게임 유닛 슬롯의 유효한 인덱스를 확인하는 함수
    {
        return index >= 0 && index < gameSlots.Length;
    }

    public void UpdateSlotStateByEnergy(float currentEnergy) // 현재 에너지에 따른 슬롯 상태를 업데이트하는 함수
    {
        foreach (var slot in gameSlots)
            slot.UpdateSlotStateByEnergy(currentEnergy);
    }

    public bool CheckUnitSpawnable(int index) // 유닛을 소환할 수 있는지 확인하는 함수
    {
        return IsValidIndex(index) && gameSlots[index].IsSpawnable;
    }
}