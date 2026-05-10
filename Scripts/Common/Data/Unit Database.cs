using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "UnitDatabase", menuName = "Scriptable Objects/Unit Database")]
public class UnitDatabase : ScriptableObject
{
    [Header("등록된 모든 유닛")]
    [SerializeField] private List<UnitStat> units = new List<UnitStat>();
    private Dictionary<string, UnitStat> nameData;
    private Dictionary<ElementType, List<UnitStat>> elementData;

    public IReadOnlyList<UnitStat> All => units;

      
    private void OnEnable()
    {
        BuildData();
    }

    private void BuildData() // 빠른 조회를 위해 캐시를 구축하는 함수
    {
        nameData = new Dictionary<string, UnitStat>(units.Count);
        elementData = new Dictionary<ElementType, List<UnitStat>>();

        foreach (var stat in units)
        {
            if (stat == null || string.IsNullOrEmpty(stat.unitName))
                continue;

            nameData[stat.unitName] = stat;

            if (!elementData.TryGetValue(stat.elementType, out var statList))
            {
                statList = new List<UnitStat>();
                elementData[stat.elementType] = statList;
            }
            statList.Add(stat);
        }

        foreach (var statList in elementData.Values)
            statList.Sort((a, b) => a.spawnCost.CompareTo(b.spawnCost));
    }

    public UnitStat FindByName(string unitName) // 이름으로 유닛 정보를 조회하는 함수
    {
        if (string.IsNullOrEmpty(unitName) || nameData == null)
            return null;

        return nameData.TryGetValue(unitName, out var stat) ? stat : null;
    }

    public IReadOnlyList<UnitStat> FindByElement(ElementType type) // 속성으로 유닛 목록을 조회하는 함수
    {
        if (elementData == null)
            BuildData();

        return elementData != null && elementData.TryGetValue(type, out var list)
            ? list
            : System.Array.Empty<UnitStat>();
    }

#if UNITY_EDITOR
    private void OnValidate() // Inspector 에서 변경 시 캐시를 다시 구축하는 함수
    {
        BuildData();
    }
#endif
}
