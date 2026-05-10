using System;
using UnityEngine;


public class GameUnitSlotContainer : MonoBehaviour
{
    [Header("게임 슬롯 배열")]
    [SerializeField] private GameUnitSlotItem[] gameSlots;

    public event Action<int, UnitStat> OnUnitSlotClicked;


    public void InitializeSlots() // 게임의 유닛 슬롯을 초기화하는 함수
    {
        for (int i = 0; i < gameSlots.Length; i++)
        {
            gameSlots[i].SetupSlot(i);
            gameSlots[i].OnUnitSlotClicked += HandleSlotClick;
        }
    }

    private void HandleSlotClick(int index, UnitStat stat) // 슬롯 클릭 시 실행되는 함수
    {
        OnUnitSlotClicked?.Invoke(index, stat);
    }

    public void ShowUnitSlot(int index, UnitStat stat) // 슬롯에 유닛 정보를 표시하는 함수
    {
        if (IsValidIndex(index))
            gameSlots[index].UpdateUI(stat);
    }

    public void StartSlotCoolTime(int index) // 슬롯의 쿨타임을 시작하는 함수
    {
        if (IsValidIndex(index))
            gameSlots[index].StartCoolTime();
    }

    private bool IsValidIndex(int index) // 게임 슬롯의 유효한 인덱스를 확인하는 함수
    {
        return index >= 0 && index < gameSlots.Length;
    }

    public void RefreshSlotsEnergyState(float currentEnergy) // 모든 슬롯을 에너지 상태에 맞춰 업데이트하는 함수
    {
        foreach (var slot in gameSlots)
            slot.EvaluateEnergyState(currentEnergy);
    }

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
}
