using Photon.Pun;
using UnityEngine;

public class CastleSpawner : MonoBehaviour
{
    [Header("성 프리팹 설정")]
    [SerializeField] private GameObject castlePrefab;



    public void SpawnCastle(Transform spawnPoint) // 성을 정해진 위치에 생성하는 함수
    {
        if (spawnPoint == null || castlePrefab == null)
            return;

        NetworkPoolManager.Instance?.RegisterNetworkPrefab(castlePrefab);
        PhotonNetwork.Instantiate(castlePrefab.name, spawnPoint.position, Quaternion.identity, 0);
    }
}
