using System.Collections.Generic;
using Photon.Pun;
using UnityEngine;

[DisallowMultipleComponent]
public class UnitCombat : MonoBehaviour
{
    [Header("유닛 관련 변수")]
    [SerializeField] private UnitStats stats;
    [SerializeField] private UnitMovement movement;
    [SerializeField] private UnitNetworkSync networkSync;

    [Header("적 유닛 저장 변수")]
    private readonly Collider2D[] scanBuffer = new Collider2D[16];
    private readonly Collider2D[] aoeBuffer = new Collider2D[32];
    [SerializeField] private readonly List<Collider2D> reusableTargets = new List<Collider2D>(8);

    public int TargetLayerMask { get; set; }


    private void Awake()
    {
        if (stats == null) 
            stats = GetComponent<UnitStats>();
        if (movement == null) 
            movement = GetComponent<UnitMovement>();
        if (networkSync == null) 
            networkSync = GetComponent<UnitNetworkSync>();
    }

    public bool HasValidTarget() // 사거리 안에 공격 가능한 적이 있는지 확인하는 함수
    {
        return AcquirePrimaryTarget() != null;
    }

    public Collider2D AcquirePrimaryTarget() // 사거리 안의 우선 적을 반환하는 함수
    {
        var col = movement.UnitCollider;
        if (col == null)
            return null;

        Bounds b = col.bounds;
        return FindOverlappingEnemy(b) ?? FindFrontEnemy(b);
    }

    public bool IsTargetInRange(Collider2D target) // 지정된 타겟이 현재 사거리 내에 있는지 확인하는 함수
    {
        if (target == null || !target.gameObject.activeInHierarchy)
            return false;

        var col = movement.UnitCollider;
        if (col == null)
            return false;

        Bounds attacker = col.bounds;
        Bounds defender = target.bounds;

        if (attacker.Intersects(defender))
            return true;

        return ComputeAttackBox(attacker).Intersects(defender);
    }

    public bool IsColliderTargetable(Collider2D col) // 현재 타겟의 콜라이더가 데미지를 받을 수 있는 상태인지 확인하는 함수
    {
        if (col == null)
            return false;
        return IsAttackableEnemy(col.GetComponent<IDamagable>());
    }

    public void ApplyDamageFromAttack(Collider2D epicenter) // 공격으로부터 실제 데미지를 적용하는 함수
    {
        if (epicenter == null)
            return;

        reusableTargets.Clear();
        if (stats.AoeRadius > 0)
            FindAllEnemiesInAoeRadius(epicenter, reusableTargets);
        else
            reusableTargets.Add(epicenter);

        ApplyDamageToTargets(reusableTargets);
    }

    private void ApplyDamageToTargets(List<Collider2D> targets) // 공격 데미지를 모든 플레이어에게 동기화하는 함수
    {
        foreach (var target in targets)
        {
            if (!TryGetValidEnemy(target, out PhotonView enemyView, out IDamagable enemy))
                continue;

            float finalDamage = CalculateDamage(enemy);
            enemyView.RPC(nameof(Unit.RPC_TakeDamage), RpcTarget.All, finalDamage);
        }
    }

    private bool TryGetValidEnemy(Collider2D col, out PhotonView enemyView, out IDamagable enemy) // 적 유닛의 콜라이더에서 유효한 적 정보를 얻는 함수
    {
        enemyView = col.GetComponent<PhotonView>();
        enemy = col.GetComponent<IDamagable>();

        if (enemyView == null || enemyView.Owner == null || enemy == null)
            return false;
        if (enemyView.IsMine == networkSync.PhotonView.IsMine)
            return false;

        return true;
    }

    private float CalculateDamage(IDamagable enemy) // 속성 상성을 적용한 최종 데미지를 계산하는 함수
    {
        if (enemy is Unit enemyUnit)
        {
            var enemyStats = enemyUnit.Stats;
            if (enemyStats != null)
                return stats.CalculateDamageAgainst(enemyStats.ElementType);
        }
        return stats.AttackDamage;
    }

    private Collider2D FindOverlappingEnemy(Bounds b) // 자신과 겹쳐 있는 적을 찾는 함수
    {
        return FindEnemyInBox(b.center, b.size);
    }

    private Collider2D FindFrontEnemy(Bounds b) // 공격 사거리 내의 적을 찾는 함수
    {
        Bounds attackBox = ComputeAttackBox(b);
        return FindEnemyInBox(attackBox.center, attackBox.size);
    }

    private Bounds ComputeAttackBox(Bounds attackerBounds) // 공격 사거리를 계산하는 함수
    {
        float dir = movement.DirectionMultiplier;
        float forwardX = dir > 0 ? attackerBounds.max.x : attackerBounds.min.x;
        float scanWidth = Mathf.Max(stats.AttackRange, 0.05f);

        Vector3 center = new Vector3(
            forwardX + dir * scanWidth * 0.5f,
            attackerBounds.center.y,
            attackerBounds.center.z);
        Vector3 size = new Vector3(scanWidth, attackerBounds.size.y, attackerBounds.size.z);
        return new Bounds(center, size);
    }

    private Collider2D FindEnemyInBox(Vector2 center, Vector2 size) // 공격 사거리 내에서 첫 번째 공격 가능한 적을 반환하는 함수
    {
        ContactFilter2D filter = new ContactFilter2D();
        filter.useLayerMask = true;
        filter.layerMask = TargetLayerMask;
        int count = Physics2D.OverlapBox(center, size, 0f, filter, scanBuffer);

        for (int i = 0; i < count; i++)
        {
            var candidate = scanBuffer[i];
            if (candidate == null || candidate == movement.UnitCollider)
                continue;

            if (IsAttackableEnemy(candidate.GetComponent<IDamagable>()))
                return candidate;
        }
        return null;
    }

    private bool IsAttackableEnemy(IDamagable enemy) // 적이 데미지를 받을 수 있는 상태인지 확인하는 함수
    {
        if (enemy == null)
            return false;

        if (enemy is Unit unit)
            return unit.IsTargetable;

        return enemy.IsAlive;
    }

    private void FindAllEnemiesInAoeRadius(Collider2D epicenter, List<Collider2D> results) // AOE 범위 내의 모든 적을 찾는 함수
    {
        ContactFilter2D filter = new ContactFilter2D();
        filter.useLayerMask = true;
        filter.layerMask = TargetLayerMask;
        int count = Physics2D.OverlapCircle(epicenter.bounds.center, stats.AoeRadius, filter, aoeBuffer);

        for (int i = 0; i < count; i++)
        {
            var hit = aoeBuffer[i];
            if (hit == null)
                continue;

            if (IsAttackableEnemy(hit.GetComponent<IDamagable>()))
                results.Add(hit);
        }
    }
}