using Photon.Pun;
using UnityEngine;
using System;
using System.Collections;

public class MapManager : BaseSceneController<MapManager>
{
    [Header("캐싱 변수")]
    [SerializeField] private MapSpawner mapSpawner;
    [SerializeField] private CastleSpawner castleSpawner;
    [SerializeField] private Collider2D cachedGroundCollider;
    public Collider2D GroundCollider => cachedGroundCollider;

    public event Action<MapData> OnMapSetupCompleted;
    public event Action<float> OnLoadProgress;


    protected override void SetUIManager() { }
    protected override void SetNetworkManager() { }
    protected override void ResetUIManager() { }
    protected override void ResetNetworkManager() { }
    protected override void InitializeState() { }

    protected override void PlayBGM()
    {
        SoundManager.instance?.StopAll();
    }

    public void SetupGameEnvironment(int mapIndex) // 맵을 최종적으로 설정하는 함수
    {
        StartCoroutine(SetupGameEnvironmentCoroutine(mapIndex));
    }

    private IEnumerator SetupGameEnvironmentCoroutine(int mapIndex) // 맵을 최종적으로 설정하는 코루틴
    {
        OnLoadProgress?.Invoke(0.1f);
        yield return null;

        MapData spawnedMap = mapSpawner?.SpawnMap(mapIndex);
        if (spawnedMap == null)
            yield break;

        cachedGroundCollider = spawnedMap.GroundCollider;
        OnLoadProgress?.Invoke(0.5f);
        yield return null;

        SpawnPlayerCastle(spawnedMap);
        OnLoadProgress?.Invoke(1.0f);
        yield return null;

        OnMapSetupCompleted?.Invoke(spawnedMap);
    }


    private void SpawnPlayerCastle(MapData mapData) // 플레이어 성을 생성하는 함수
    {
        Transform spawnPoint = PhotonNetwork.IsMasterClient
            ? mapData.Player1CastlePoint
            : mapData.Player2CastlePoint;

        castleSpawner?.SpawnCastle(spawnPoint);
    }
}