using UnityEngine;

[DisallowMultipleComponent]
[RequireComponent(typeof(UnitMovement))]
[RequireComponent(typeof(UnitStats))]

public class UnitGizmosDrawer : MonoBehaviour
{
    [Header("유닛 컴포넌트")]
    [SerializeField] private UnitMovement movement;
    [SerializeField] private UnitStats stats;


    private void OnValidate()
    {
        if (movement == null) 
            movement = GetComponent<UnitMovement>();

        if (stats == null) 
            stats = GetComponent<UnitStats>();
    }

    private void OnDrawGizmos()
    {
        if (movement == null || movement.UnitCollider == null || stats == null)
            return;

        Bounds b = movement.UnitCollider.bounds;
        DrawColliderBounds(b);
        DrawAttackRangeBox(b);
        DrawAoeBlastRadius(b);
    }

    private void DrawColliderBounds(Bounds b) // 유닛의 콜라이더 경계를 그리는 함수
    {
        Gizmos.color = Color.green;
        Gizmos.DrawWireCube(b.center, b.size);
    }

    private void DrawAttackRangeBox(Bounds b) // 유닛의 공격 사거리를 그리는 함수
    {
        if (stats.AttackRange <= 0)
            return;

        float dir = movement.DirectionMultiplier;
        float forwardX = dir > 0 ? b.max.x : b.min.x;
        float scanWidth = Mathf.Max(stats.AttackRange, 0.05f);

        Vector3 center = new(forwardX + dir * scanWidth * 0.5f, b.center.y, b.center.z);
        Vector3 size = new(scanWidth, b.size.y, b.size.z);

        Gizmos.color = Color.red;
        Gizmos.DrawWireCube(center, new Vector3(size.x, size.y, 0));
    }

    private void DrawAoeBlastRadius(Bounds b) // 유닛의 AOE 반경을 그리는 함수
    {
        if (stats.AoeRadius <= 0)
            return;

        float dir = movement.DirectionMultiplier;
        float forwardX = dir >= 0 ? b.max.x : b.min.x;
        Vector3 center = new Vector3(forwardX + dir * stats.AttackRange, b.center.y, transform.position.z);

        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(center, stats.AoeRadius);
    }
}
