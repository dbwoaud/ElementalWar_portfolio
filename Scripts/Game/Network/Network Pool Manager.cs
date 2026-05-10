using UnityEngine;
using Photon.Pun;
using System.Collections.Generic;


public class NetworkPoolManager : Singleton<NetworkPoolManager>, IPunPrefabPool
{
    [Header("프리팹 딕셔너리")]
    private readonly Dictionary<string, GameObject> prefabDict = new Dictionary<string, GameObject>();

    [Header("오브젝트 풀")]
    private readonly Dictionary<string, Queue<GameObject>> objectPool = new Dictionary<string, Queue<GameObject>>();


    protected override void Awake()
    {
        base.Awake();
        PhotonNetwork.PrefabPool = this;
    }

    public void RegisterNetworkPrefab(GameObject prefab) // 네트워크 프리팹에 오브젝트를 등록하는 함수
    {
        if (prefab == null || prefabDict.ContainsKey(prefab.name)) 
            return;

        prefabDict.Add(prefab.name, prefab);
        objectPool.Add(prefab.name, new Queue<GameObject>());
    }

    public GameObject Instantiate(string prefabId, Vector3 position, Quaternion rotation) // 프리팹을 생성하는 함수
    {
        if (prefabDict.TryGetValue(prefabId, out GameObject sourcePrefab))
        {
            Queue<GameObject> pool = objectPool[prefabId];

            GameObject obj = pool.Count > 0
                ? ReuseFromPool(pool, position, rotation)
                : CreateNewInstance(sourcePrefab, position, rotation);

            return obj;
        }

        return null;
    }

    public void Destroy(GameObject obj) // 프리팹을 파괴하는 함수
    {
        obj.SetActive(false);
        ReturnToPool(obj);
    }

    private GameObject ReuseFromPool(Queue<GameObject> pool, Vector3 position, Quaternion rotation) // 오브젝트 풀에서 프리팹을 불러오는 함수
    {
        GameObject obj = pool.Dequeue();
        obj.transform.SetPositionAndRotation(position, rotation);
        return obj;
    }

    private GameObject CreateNewInstance(GameObject prefab, Vector3 position, Quaternion rotation) // 새로운 프리팹 인스턴스를 생성하는 함수
    {
        GameObject obj = UnityEngine.Object.Instantiate(prefab, position, rotation);
        obj.SetActive(false);
        return obj;
    }

    private void ReturnToPool(GameObject obj) // 프리팹을 오브젝트 풀에 반환하는 함수
    {
        string prefabId = obj.name.Replace("(Clone)", "").Trim();

        if (objectPool.TryGetValue(prefabId, out Queue<GameObject> pool))
        {
            pool.Enqueue(obj);
        }
    }
}
