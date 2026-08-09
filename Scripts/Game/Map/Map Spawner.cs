using System;
using System.Collections.Generic;
using UnityEngine;

public class MapSpawner : MonoBehaviour
{
    [Header("맵 프리팹 목록")]
    [SerializeField] private List<MapData> mapPrefabList;
 
    public event Action<MapData> OnMapSpawned; // 맵 생성 완료 이벤트 


    public MapData SpawnMap(int mapIndex) // 맵을 생성하는 함수
    {
        if (CheckInValidIndex(mapIndex))
            return null;

        MapData mapInstance = Instantiate(mapPrefabList[mapIndex]);
        OnMapSpawned?.Invoke(mapInstance);
        return mapInstance;
    }

    private bool CheckInValidIndex(int mapIndex) // 유효하지 않은 인덱스인지 확인하는 함수
    {
        return mapIndex < 0 || mapIndex >= mapPrefabList.Count;
    }
}