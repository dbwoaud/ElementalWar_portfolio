using Photon.Pun;
using UnityEngine;
using System;


public class CastleSpawner : MonoBehaviour
{
    [Header("성 프리팹 설정")]
    [SerializeField] private GameObject castlePrefab;

    public event Action<GameObject> OnCastleSpawned;


    private void Awake()
    {
        if (castlePrefab != null && NetworkPoolManager.instance != null)
            NetworkPoolManager.instance.RegisterNetworkPrefab(castlePrefab);
    }

    public void SpawnCastle(Transform spawnPoint) // 성을 정해진 위치에 생성하는 함수
    {
        if (spawnPoint == null || castlePrefab == null)
            return;

        bool isRightSide = !PhotonNetwork.IsMasterClient;
        object[] initData = new object[] { isRightSide };
        GameObject castleInstance = PhotonNetwork.Instantiate
        (
            castlePrefab.name,
            spawnPoint.position,
            Quaternion.identity,
            0,
            initData
        );

        OnCastleSpawned?.Invoke(castleInstance);
    }
}
