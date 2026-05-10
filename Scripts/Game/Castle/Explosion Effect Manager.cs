using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ExplosionEffectManager: Singleton<ExplosionEffectManager>
{
    [Header("캐싱 변수")]
    [SerializeField] private float cachedGroundMinX;
    [SerializeField] private float cachedGroundMaxX;
    [SerializeField] private bool hasCachedBounds;

    [Header("폭발 연출 설정")]
    [SerializeField] private GameObject explosionPrefab;
    [SerializeField] private float spacing = 0.5f; 
    [SerializeField] private float delay = 0.2f;
    [SerializeField] private float effectLifeTime;

    [Header("오브젝트 풀")]
    [SerializeField] private Queue<GameObject> explosionPool = new Queue<GameObject>();
    [SerializeField] private int initialiPoolsize = 40;
    [SerializeField] private int maxPoolSize = 80;


    protected override void Awake()
    {
        base.Awake();
        ParticleSystem ps = explosionPrefab.GetComponent<ParticleSystem>();
        if (ps != null)
        {
            effectLifeTime = ps.main.duration + ps.main.startLifetime.constantMax;
        }
        PreloadExplosions(initialiPoolsize);
    }

    private void PreloadExplosions(int count) // 폭발 프리팹을 풀에 미리 넣는 함수
    {
        for (int i = 0; i < count; i++)
        {
            GameObject obj = Instantiate(explosionPrefab, transform);
            obj.SetActive(false);
            explosionPool.Enqueue(obj);
        }
    }

    public void PlayChainExplosion(Vector3 startPos) // 연쇄 폭발 연출을 재생하는 함수
    {
        StartCoroutine(ChainExplosionCoroutine(startPos));
    }

    private IEnumerator ChainExplosionCoroutine(Vector3 startPos) // 연쇄 폭발 연출을 재생하는 코루틴
    {
        SpawnExplosion(startPos);
        yield return new WaitForSeconds(delay);

        if (TryGetGroundBounds(out float minX, out float maxX))
        {
            yield return StartCoroutine(ExpandExplosionsRoutine(startPos.x, startPos.y, startPos.z, minX, maxX));
        }
    }

    private void SpawnExplosion(Vector3 pos) // 폭발 프리팹을 생성하는 함수
    {
        GameObject effect;
        if (explosionPool.Count > 0)
            effect = GetPrefabFromPool(pos);
        else
            effect = Instantiate(explosionPrefab, pos, Quaternion.identity, transform);
    
        SoundManager.instance?.Play(SoundKey.Explosion);
        StartCoroutine(ReturnPrefabToPoolAfterDelay(effect, effectLifeTime));
    }

    private GameObject GetPrefabFromPool(Vector3 pos) // 폭발 프리팹을 오브젝트 풀에서 가져오는 함수
    {
        GameObject effect = explosionPool.Dequeue();
        effect.transform.position = pos;
        effect.SetActive(true);
        return effect;
    }

    private bool TryGetGroundBounds(out float minX, out float maxX) // 지형의 x축 경계 값을 계산하는 함수
    {
        if (hasCachedBounds)
        {
            minX = cachedGroundMinX;
            maxX = cachedGroundMaxX;
            return true;
        }

        Collider2D groundCollider = MapManager.instance?.GroundCollider;
        if (groundCollider != null)
        {
            cachedGroundMinX = groundCollider.bounds.min.x;
            cachedGroundMaxX = groundCollider.bounds.max.x;
            hasCachedBounds = true;
            minX = cachedGroundMinX;
            maxX = cachedGroundMaxX;
            return true;
        }

        minX = 0;
        maxX = 0;
        return false;
    }

    private IEnumerator ExpandExplosionsRoutine(float startX, float startY, float startZ, float minX, float maxX) // 지형의 경계 끝까지 연쇄 폭발 연출을 재생하는 코루틴                                                                                                         
    {
        int step = 1;
        bool canExpandLeft = true;
        bool canExpandRight = true;

        while (canExpandLeft || canExpandRight)
        {
            float leftX = startX - (spacing * step);
            float rightX = startX + (spacing * step);

            canExpandLeft = leftX >= minX;
            canExpandRight = rightX <= maxX;

            if (canExpandLeft)
                SpawnExplosion(new Vector3(leftX, startY, startZ));

            if (canExpandRight)
                SpawnExplosion(new Vector3(rightX, startY, startZ));

            step++;

            if (canExpandLeft || canExpandRight)
                yield return new WaitForSeconds(delay);
        }
    }

    private IEnumerator ReturnPrefabToPoolAfterDelay(GameObject effect, float time) // 폭발 프리팹을 일정시간 이후에 오브젝트 풀로 반환하는 코루틴
    {
        yield return new WaitForSeconds(time);

        if (effect == null)
            yield break;

        if (explosionPool.Count >= maxPoolSize)
        {
            Destroy(effect);
            yield break;
        }

        effect.SetActive(false);
        explosionPool.Enqueue(effect);
    }
}