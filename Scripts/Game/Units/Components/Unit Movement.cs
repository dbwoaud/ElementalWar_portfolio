using UnityEngine;

[DisallowMultipleComponent]
[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(Collider2D))]
public class UnitMovement : MonoBehaviour
{
    [Header("컴포넌트")]
    [SerializeField] private UnitStats stats;
    [SerializeField] private Rigidbody2D rb;
    [SerializeField] private Collider2D unitCollider;
    public Collider2D UnitCollider => unitCollider;

    [Header("넉백 설정")]
    [SerializeField] private float knockbackPower = 3f;
    [SerializeField] private float knockbackDuration = 0.5f;
    public float KnockbackDuration => knockbackDuration;
    public float DirectionMultiplier { get; private set; } = 1f;
    

    private void Awake()
    {
        if (rb == null)
            rb = GetComponent<Rigidbody2D>();

        if (unitCollider == null)
            unitCollider = GetComponent<Collider2D>();

        if (stats == null)
            stats = GetComponent<UnitStats>();
    }

    public void SetDirection(float multiplier) // 유닛의 방향을 설정하는 함수
    {
        DirectionMultiplier = multiplier;
    }

    public void ResetForReuse() // 재사용을 위해 유닛 이동 관련 설정을 초기화하는 함수
    {
        if (rb != null)
        {
            rb.simulated = true;
            rb.linearVelocity = Vector2.zero;
            rb.angularVelocity = 0f;
        }
        if (unitCollider != null)
            unitCollider.enabled = true;
    }

    public void ApplyMovement() // 유닛 이동을 적용하는 함수
    {
        if (rb == null || stats == null)
            return;

        Vector2 v = rb.linearVelocity;
        v.x = stats.MoveSpeed * DirectionMultiplier;
        rb.linearVelocity = v;
    }

    public void StopMovement() // 유닛 이동을 정지하는 함수
    {
        if (rb == null)
            return;

        rb.linearVelocity = new Vector2(0f, rb.linearVelocity.y);
    }

    public void ApplyKnockback() // 유닛 피격 시 넉백 연출을 적용하는 함수
    {
        if (rb == null || rb.bodyType != RigidbodyType2D.Dynamic)
            return;

        rb.linearVelocity = Vector2.zero;
        var force = new Vector2(-DirectionMultiplier * knockbackPower, knockbackPower * 0.8f);
        rb.AddForce(force, ForceMode2D.Impulse);
    }

    public void SetPhysics(bool isOwnedByLocalPlayer) // 유닛 소환 시 물리 컴포넌트를 설정하는 함수
    {
        if (rb == null)
            return;

        rb.bodyType = isOwnedByLocalPlayer ? RigidbodyType2D.Dynamic : RigidbodyType2D.Kinematic;
        rb.simulated = true;
    }

    public void DisableAllPhysics() // 유닛 사망 시 모든 물리 컴포넌트를 비활성화하는 함수
    {
        if (rb != null)
            rb.simulated = false;

        if (unitCollider != null)
            unitCollider.enabled = false;
    }
}