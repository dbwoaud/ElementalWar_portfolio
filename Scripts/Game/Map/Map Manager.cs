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

    private bool isMapReady;

    public event Action<MapData> OnMapSetupCompleted; // 맵 설정 완료 이벤트
    public event Action<float> OnLoadProgress; // 로딩 진행도 이벤트


    protected override void SetUIManager() // UI 매니저를 설정하는 함수 
    {

    }

    protected override void SetNetworkManager() // 네트워크 매니저를 설정하는 함수 
    {
        
    }

    protected override void ResetUIManager() // UI 매니저를 리셋하는 함수
    {

    }

    protected override void ResetNetworkManager() // 네트워크 매니저를 리셋하는 함수
    {

    }

    protected override void PlayBGM() // 씬의 배경음악을 재생하는 함수
    {
        SoundManager.Instance?.StopAll();
    }

    protected override void InitializeState() // 씬의 초기상태를 설정하는 함수
    {

    }

    public void SetupGameMap(int mapIndex) // 맵을 설정하는 함수
    {
        if (isMapReady)
            return;

        isMapReady = true;
        StartCoroutine(SetupGameMapCoroutine(mapIndex));
    }

    private IEnumerator SetupGameMapCoroutine(int mapIndex) // 맵을 설정하는 코루틴
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
        Transform spawnPoint = PhotonNetwork.IsMasterClient ? mapData.Player1CastlePoint : mapData.Player2CastlePoint;
        castleSpawner?.SpawnCastle(spawnPoint);
    }
}