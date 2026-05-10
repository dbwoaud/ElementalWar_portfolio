using System.Collections.Generic;

public static class UnitRegistry
{
    private static readonly HashSet<Unit> activeUnits = new HashSet<Unit>();
    public static IReadOnlyCollection<Unit> ActiveUnits => activeUnits;

    public static void Register(Unit unit) // 유닛을 활성 목록에 등록하는 함수
    {
        if (unit == null)
            return;

        activeUnits.Add(unit);
    }

    public static void Unregister(Unit unit) // 유닛을 활성 목록에서 제거하는 함수
    {
        if (unit == null)
            return;

        activeUnits.Remove(unit);
    }

    public static void Clear() // 활성 유닛을 초기화하는 함수
    {
        activeUnits.Clear();
    }

    public static void CopyTo(List<Unit> buffer) // 활성 유닛 목록을 외부로 복사하는 함수
    {
        if (buffer == null)
            return;

        buffer.Clear();
        foreach (var u in activeUnits)
        {
            if (u != null)
                buffer.Add(u);
        }
    }
}
