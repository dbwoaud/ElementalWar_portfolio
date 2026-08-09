using UnityEngine;
using Photon.Pun;
using System.Collections.Generic;


public class NetworkPoolManager : Singleton<NetworkPoolManager>, IPunPrefabPool
{
    [Header("프리팹 배열")]
    [SerializeField] private GameObject[] prefabArray;
    private readonly Dictionary<string, GameObject> prefabDict = new();
    private readonly Dictionary<string, Queue<GameObject>> objectPool = new();
    private readonly HashSet<GameObject> pooledObjects = new();

    protected override void Awake()
    {
        base.Awake();
        PhotonNetwork.PrefabPool = this;
        RegisterPrefabs();
    }

    protected override void OnDestroy()
    {
        foreach (Queue<GameObject> pool in objectPool.Values)
            pool.Clear();

        pooledObjects.Clear();
        base.OnDestroy();
    }

    private void RegisterPrefabs() // 프리팹 배열의 프리팹들을 네트워크 프리팹 배열에 등록하는 함수
    {
        if (prefabArray == null)
            return;

        foreach (GameObject prefab in prefabArray)
            RegisterNetworkPrefab(prefab);
    }

    public void RegisterNetworkPrefab(GameObject prefab) // 네트워크 프리팹 배열에 프리팹을 등록하는 함수
    {
        if (prefab == null || prefabDict.ContainsKey(prefab.name)) 
            return;

        prefabDict.Add(prefab.name, prefab);
        objectPool.Add(prefab.name, new Queue<GameObject>());
    }

    public GameObject Instantiate(string prefabId, Vector3 position, Quaternion rotation) // 프리팹을 생성하는 함수
    {
        if (!prefabDict.TryGetValue(prefabId, out GameObject sourcePrefab))
            return new GameObject(prefabId);

        /* 프로파일링용 before 경로: 오브젝트 풀 미사용 */
        if (!ProfilingSwitches.UsePooling)
            return CreatePrefab(sourcePrefab, position, rotation);

        /* 프로파일링용 after 경로: 오브젝트 풀 사용 */
        if (!objectPool.TryGetValue(prefabId, out Queue<GameObject> pool))
        {
            pool = new Queue<GameObject>();
            objectPool[prefabId] = pool;
        }

        return pool.Count > 0 ? GetPrefabFromPool(pool, position, rotation) : CreatePrefab(sourcePrefab, position, rotation);
    }

    public void Destroy(GameObject obj) // 프리팹을 파괴하는 함수
    {
        if (obj == null)
            return;

        /* 프로파일링용 before 경로: 오브젝트 풀 미사용 */
        if (!ProfilingSwitches.UsePooling)
        {
            Object.Destroy(obj);
            return;
        }

        /* 프로파일링용 after 경로: 오브젝트 풀 사용 */
        obj.SetActive(false);
        ReturnToPool(obj);
    }

    private GameObject GetPrefabFromPool(Queue<GameObject> pool, Vector3 position, Quaternion rotation) // 오브젝트 풀에서 프리팹을 가져오는 함수
    {
        GameObject obj = pool.Dequeue();
        pooledObjects.Remove(obj);
        obj.transform.SetPositionAndRotation(position, rotation);
        return obj;
    }

    private GameObject CreatePrefab(GameObject prefab, Vector3 position, Quaternion rotation) // 새로운 프리팹을 생성하는 함수
    {
        GameObject obj = Object.Instantiate(prefab, position, rotation);
        obj.SetActive(false);
        return obj;
    }

    private void ReturnToPool(GameObject obj) // 오브젝트 풀에 프리팹을 반환하는 함수
    {
        if (pooledObjects.Contains(obj))
            return;

        string prefabId = obj.name.Replace("(Clone)", "").Trim();

        if (!objectPool.TryGetValue(prefabId, out Queue<GameObject> pool))
            return;

        pool.Enqueue(obj);
        pooledObjects.Add(obj);
    }
}