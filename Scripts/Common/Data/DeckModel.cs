using System;
using System.Linq;

public class DeckModel
{
    private readonly UnitStat[] deck;

    public event Action<int, UnitStat> OnDeckSlotStateChanged; // 덱 슬롯 상태 변경 시 실행되는 이벤트


    public DeckModel()
    {
        deck = new UnitStat[InputBindings.DeckSize];
    }

    public UnitStat GetUnit(int index) // 덱의 특정 인덱스에 해당하는 유닛 정보를 반환하는 함수
    {
        return IsValidIndex(index) ? deck[index] : null;
    }

    public void SetUnit(int index, UnitStat stat) // 덱의 특정 인덱스에 유닛을 설정하는 함수
    {
        if (!IsValidIndex(index))
            return;

        deck[index] = stat;
        OnDeckSlotStateChanged?.Invoke(index, stat);
    }

    public void RemoveUnit(int index) // 덱의 특정 인덱스에 해당하는 유닛을 제거하는 함수
    {
        SetUnit(index, null);
    }

    public void SwapUnits(int from, int to) // 덱의 유닛을 서로 교체하는 함수
    {
        if (!IsValidIndex(from) || !IsValidIndex(to))
            return;

        (deck[from], deck[to]) = (deck[to], deck[from]);
        OnDeckSlotStateChanged?.Invoke(from, deck[from]);
        OnDeckSlotStateChanged?.Invoke(to, deck[to]);
    }

    private bool IsValidIndex(int index) // 유효한 인덱스인지 확인하는 함수
    {
        return index >= 0 && index < deck.Length;
    }

    public bool IsFull() // 덱에 모든 유닛이 설정되어 있는지 확인하는 함수
    {
        return deck.All(unit => unit != null);
    }

    public int FindUnitIndex(UnitStat stat) // 유닛 정보에 해당하는 덱의 인덱스를 찾는 함수
    {
        if (stat == null)
            return -1;

        for (int i = 0; i < deck.Length; i++)
        {
            if (deck[i] == stat)
                return i;
        }
        return -1;
    }

    public string[] GetUnitNames() // 덱에 있는 모든 유닛의 이름을 배열로 반환하는 함수
    {
        return deck.Select(s => s != null ? s.UnitName : string.Empty).ToArray();
    }
}