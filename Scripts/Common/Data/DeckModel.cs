using System;
using System.Linq;

public class DeckModel
{
    private UnitStat[] myDeck;
    public int Capacity => myDeck.Length;

    public event Action<int, UnitStat> OnSlotChanged;


    public DeckModel()
    {
        myDeck = new UnitStat[InputBindings.DeckSize];
    }

    public UnitStat GetUnit(int index) // 특정 인덱스의 유닛 정보를 조회하는 함수
    {
        return IsValidIndex(index) ? myDeck[index] : null;
    }

    public void SetUnit(int index, UnitStat stat) // 특정 인덱스에 유닛을 배치하는 함수
    {
        if (!IsValidIndex(index))
            return;

        myDeck[index] = stat;
        OnSlotChanged?.Invoke(index, stat);
    }

    public void RemoveUnit(int index) // 특정 인덱스의 유닛을 제거하는 함수
    {
        SetUnit(index, null);
    }

    public void SwapUnits(int from, int to) // 덱에 유닛을 스왑하는 함수
    {
        if (!IsValidIndex(from) || !IsValidIndex(to))
            return;

        (myDeck[from], myDeck[to]) = (myDeck[to], myDeck[from]);
        OnSlotChanged?.Invoke(from, myDeck[from]);
        OnSlotChanged?.Invoke(to, myDeck[to]);
    }

    private bool IsValidIndex(int index) // 유효한 인덱스인지 확인하는 함수
    {
        return index >= 0 && index < myDeck.Length;
    }

    public bool IsFull() // 덱의 모든 유닛이 설정되어있는지 검사하는 함수
    {
        return myDeck.All(unit => unit != null);
    }

    public int FindUnitIndex(UnitStat stat) // 유닛이 위치한 인덱스를 검색하는 함수
    {
        if (stat == null)
            return -1;

        for (int i = 0; i < myDeck.Length; i++)
        {
            if (myDeck[i] == stat)
                return i;
        }
        return -1;
    }

    public string[] GetUnitNames() //덱의 모든 유닛 이름을 배열로 반환하는 함수
    {
        return myDeck.Select(s => s != null ? s.unitName : string.Empty).ToArray();
    }

    public UnitStat[] GetSnapshot() // 덱의 복사본을 반환하는 함수
    {
        return (UnitStat[])myDeck.Clone();
    }
}