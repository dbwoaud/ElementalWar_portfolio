using System;
using System.Collections.Generic;
using UnityEngine;

public class MapSpawner : MonoBehaviour
{
    [Header("프리팹 데이터베이스")]
    [SerializeField] private List<MapData> mapPrefabList;
 
    public event Action<MapData> OnMapSpawned; 


    public MapData SpawnMap(int mapIndex) // 맵을 생성하는 함수
    {
        if (CheckInValidIndex(mapIndex))
            return null;

        MapData mapInstance = Instantiate(mapPrefabList[mapIndex]);
        OnMapSpawned?.Invoke(mapInstance);
        return mapInstance;
    }

    private bool CheckInValidIndex(int mapIndex) // 유효한 인덱스인지 확인하는 함수
    {
        return mapIndex < 0 || mapIndex >= mapPrefabList.Count;
    }
}