using System.Collections.Generic;
using Photon.Pun;
using UnityEngine;

[DisallowMultipleComponent]
[RequireComponent(typeof(UnitStats))]
[RequireComponent(typeof(UnitMovement))]
[RequireComponent(typeof(UnitNetworkSync))]
public class UnitCombat : MonoBehaviour
{
    [Header("유닛 컴포넌트")]
    [SerializeField] private UnitStats stats;
    [SerializeField] private UnitMovement movement;
    [SerializeField] private UnitNetworkSync networkSync;

    private const int ScanBufferSize = 16;
    private const int AoeBufferSize = 32;
    private const float MinScanWidth = 0.05f;
    private readonly Collider2D[] scanBuffer = new Collider2D[ScanBufferSize];
    private readonly Collider2D[] aoeBuffer = new Collider2D[AoeBufferSize];
    private readonly List<Collider2D> currentTargetBuffer = new(8);
    private ContactFilter2D targetFilter;

    private int targetLayerMask;
    public int TargetLayerMask
    {
        get => targetLayerMask;
        set
        {
            targetLayerMask = value;
            targetFilter.useLayerMask = true;
            targetFilter.layerMask = value;
            targetFilter.useTriggers = false;
        }
    }


    private void Awake()
    {
        if (stats == null) 
            stats = GetComponent<UnitStats>();

        if (movement == null) 
            movement = GetComponent<UnitMovement>();

        if (networkSync == null) 
            networkSync = GetComponent<UnitNetworkSync>();
    }

    public bool CheckValidEnemy() // 공격 사거리 내 적 유닛이 있는지 확인하는 함수
    {
        return FindValidEnemy() != null;
    }

    public Collider2D FindValidEnemy() // 공격 사거리 내 적을 찾는 함수
    {
        var col = movement.UnitCollider;
        if (col == null)
            return null;

        Bounds b = col.bounds;
        return FindOverlappingEnemy(b) ?? FindFrontEnemy(b);
    }

    private Collider2D FindOverlappingEnemy(Bounds b) // 겹쳐 있는 적을 찾는 함수
    {
        return FindEnemyInBox(b.center, b.size);
    }

    private Collider2D FindFrontEnemy(Bounds b) // 전방 공격 사거리 내 적을 찾는 함수
    {
        Bounds attackBox = CalculateAttackBox(b);
        return FindEnemyInBox(attackBox.center, attackBox.size);
    }

    private Collider2D FindEnemyInBox(Vector2 center, Vector2 size) // 박스 내 적을 찾는 함수
    {
        ProfilingCounters.CountPhysicsQuery();

#if ENABLE_PROFILING
        /* 프로파일링용 before 경로: 호출마다 새 배열을 반환 */
        if (!ProfilingSwitches.UseNonAllocQueries)
        {
            Collider2D[] allocated = Physics2D.OverlapBoxAll(center, size, 0f, targetLayerMask);
            for (int i = 0; i < allocated.Length; i++)
            {
                if (allocated[i] == null || allocated[i] == movement.UnitCollider)
                    continue;

                if (IsAttackableEnemy(allocated[i].GetComponent<IDamagable>()))
                    return allocated[i];
            }
            return null;
        }
#endif
        /* 프로파일링용 after 경로: 버퍼와 필터를 재사용 */
        int count = Physics2D.OverlapBox(center, size, 0f, targetFilter, scanBuffer);
        count = Mathf.Min(count, scanBuffer.Length);
        for (int i = 0; i < count; i++)
        {
            var enemyCollider = scanBuffer[i];
            if (enemyCollider == null || enemyCollider == movement.UnitCollider)
                continue;

            if (IsAttackableEnemy(enemyCollider.GetComponent<IDamagable>()))
                return enemyCollider;
        }
        return null;
    }

    private bool IsAttackableEnemy(IDamagable enemy) // 공격 가능한 적 상태인지 확인하는 함수
    {
        if (enemy == null)
            return false;

        if (enemy is Unit unit)
            return unit.IsTargetable;

        return enemy.IsAlive;
    }

    private Bounds CalculateAttackBox(Bounds attackerBounds) // 공격 사거리를 계산하는 함수
    {
        float dir = movement.DirectionMultiplier;
        float forwardX = dir > 0 ? attackerBounds.max.x : attackerBounds.min.x;
        float scanWidth = Mathf.Max(stats.AttackRange, MinScanWidth);

        Vector3 center = new(forwardX + dir * scanWidth * 0.5f, attackerBounds.center.y, attackerBounds.center.z);
        Vector3 size = new(scanWidth, attackerBounds.size.y, attackerBounds.size.z);
        return new Bounds(center, size);
    }

    public bool IsAttackableEnemy(Collider2D enemyCollider) // 공격 가능한 적 콜라이더인지 확인하는 함수
    {
        if (enemyCollider == null)
            return false;

        return IsAttackableEnemy(enemyCollider.GetComponent<IDamagable>());
    }

    public void ApplyDamage(Collider2D mainTargetCollider) // 데미지를 적용하는 함수
    {
        if (mainTargetCollider == null)
            return;

        currentTargetBuffer.Clear();

        if (stats.AoeRadius > 0)
            FindAllEnemiesInAoeRange(mainTargetCollider, currentTargetBuffer);
        else
            currentTargetBuffer.Add(mainTargetCollider);

        ApplyDamageToTargets(currentTargetBuffer);
    }

    private void FindAllEnemiesInAoeRange(Collider2D epicenter, List<Collider2D> results) // AOE 사거리 내의 모든 적을 찾는 함수
    {
        ProfilingCounters.CountPhysicsQuery();

        Vector2 center = epicenter.bounds.center;

#if ENABLE_PROFILING
        /* 프로파일링용 before 경로: 호출마다 새 배열을 반환 */
        if (!ProfilingSwitches.UseNonAllocQueries)
        {
            Collider2D[] allocated = Physics2D.OverlapCircleAll(center, stats.AoeRadius, targetLayerMask);
            for (int i = 0; i < allocated.Length; i++)
            {
                var hit = allocated[i];
                if (hit == null || hit == movement.UnitCollider)
                    continue;

                if (IsAttackableEnemy(hit.GetComponent<IDamagable>()))
                    results.Add(hit);
            }
            return;
        }
#endif
        /* 프로파일링용 after 경로: 버퍼와 필터를 재사용 */
        int count = Physics2D.OverlapCircle(center, stats.AoeRadius, targetFilter, aoeBuffer);
        count = Mathf.Min(count, aoeBuffer.Length);
        for (int i = 0; i < count; i++)
        {
            var enemyCollider = aoeBuffer[i];
            if (enemyCollider == null || enemyCollider == movement.UnitCollider)
                continue;

            if (IsAttackableEnemy(enemyCollider.GetComponent<IDamagable>()))
                results.Add(enemyCollider);
        }
    }

    private void ApplyDamageToTargets(List<Collider2D> targets) // 현재 대상 버퍼에 있는 모든 적들에게 데미지를 적용하는 함수
    {
        foreach (var targetCollider in targets)
        {
            if (!TryGetValidEnemy(targetCollider, out PhotonView targetView, out IDamagable target))
                continue;

            float finalDamage = CalculateDamage(target);
            ProfilingCounters.CountRpcSent();
            targetView.RPC(nameof(Unit.RPC_TakeDamage), RpcTarget.All, finalDamage);
        }
    }

    private bool TryGetValidEnemy(Collider2D col, out PhotonView targetView, out IDamagable target) // 유효한 적 반환을 시도하는 함수
    {
        targetView = col.GetComponent<PhotonView>();
        target = col.GetComponent<IDamagable>();

        if (targetView == null || targetView.Owner == null || target == null)
            return false;

        if (targetView.IsMine == networkSync.PhotonView.IsMine)
            return false;

        return true;
    }

    private float CalculateDamage(IDamagable enemy) // 속성 상성을 적용한 최종 데미지를 계산하는 함수
    {
        if (enemy is Unit enemyUnit)
        {
            var enemyStats = enemyUnit.Stats;
            if (enemyStats != null)
                return stats.CalculateDamage(enemyStats.ElementType);
        }
        return stats.AttackDamage;
    }
}