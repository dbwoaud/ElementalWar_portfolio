using UnityEngine;

[DisallowMultipleComponent]
[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(Collider2D))]
public class UnitMovement : MonoBehaviour
{
    [Header("컴포넌트")]
    [SerializeField] private Rigidbody2D rb;
    public Rigidbody2D Body => rb;
    [SerializeField] private Collider2D unitCollider;
    public Collider2D UnitCollider => unitCollider;

    [Header("유닛 관련 변수")]
    [SerializeField] private UnitStats stats;

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

    public void ResetForReuse() // 네트워크 풀에서 재사용 시 물리 상태를 초기화하는 함수
    {
        if (rb != null)
            rb.simulated = true;
        if (unitCollider != null)
            unitCollider.enabled = true;
    }

    public void MoveForward() // 정면 방향으로 유닛을 등속 이동시키는 함수
    {
        if (rb == null || stats == null)
            return;

        Vector2 v = rb.linearVelocity;
        v.x = stats.MoveSpeed * DirectionMultiplier;
        rb.linearVelocity = v;
    }

    public void Stop() // 수평 속도를 0으로 설정하는 함수
    {
        if (rb == null)
            return;

        rb.linearVelocity = new Vector2(0f, rb.linearVelocity.y);
    }

    public void ApplyKnockback() // 피격 시 뒤로 넉백 효과를 적용하는 함수
    {
        if (rb == null)
            return;

        rb.linearVelocity = Vector2.zero;
        var force = new Vector2(-DirectionMultiplier * knockbackPower, knockbackPower * 0.8f);
        rb.AddForce(force, ForceMode2D.Impulse);
    }

    public void DisableAllPhysics() // 유닛 사망 시 물리 상태를 차단하는 함수
    {
        if (rb != null)
            rb.simulated = false;
        if (unitCollider != null)
            unitCollider.enabled = false;
    }
}