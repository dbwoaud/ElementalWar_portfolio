using System;
using UnityEngine;

[DisallowMultipleComponent]
public class UnitStats : MonoBehaviour, IDamagable
{
    [Header("기본 데이터")]
    [SerializeField] private UnitStat baseStat;
    public UnitStat BaseStat => baseStat;

    [Header("유닛 능력치")]
    [SerializeField] private float maxHP;
    [SerializeField] private float currentHP;
    [SerializeField] private float attackDamage;
    [SerializeField] private float firstAttackDelay;
    [SerializeField] private float attackInterval;
    [SerializeField] private float attackRange;
    [SerializeField] private float moveSpeed;
    [SerializeField] private float aoeRadius;
    [SerializeField] private float spawnCost;
    [SerializeField] private ElementType elementType;

    [Header("넉백 플래그")]
    [SerializeField] private bool hasTriggeredHalfHPHit;
    [SerializeField] private bool hasTriggeredQuarterHPHit;

    public float MaxHP => maxHP;

    public float CurrentHP => currentHP;

    public bool IsAlive => currentHP > 0;

    public float AttackDamage => attackDamage;

    public float FirstAttackDelay => firstAttackDelay;

    public float AttackInterval => attackInterval;

    public float AttackRange => attackRange;

    public float MoveSpeed => moveSpeed;

    public float AoeRadius => aoeRadius;

    public float SpawnCost => spawnCost;

    public ElementType ElementType => elementType;

    public event Action<float> OnDamageTaken;
    public event Action OnDied;
    public event Action OnHpThresholdCrossed;


    public void InitializeFromBaseStat() // 유닛 데이터에서 전투 능력치를 초기화하는 함수
    {
        if (baseStat == null)
            return;

        maxHP = baseStat.maxHP;
        currentHP = maxHP;
        attackDamage = baseStat.attackDamage;
        firstAttackDelay = baseStat.firstAttackDelay;
        attackInterval = baseStat.attackInterval;
        attackRange = baseStat.attackRange;
        moveSpeed = baseStat.moveSpeed;
        aoeRadius = baseStat.aoeRadius;
        spawnCost = baseStat.spawnCost;
        elementType = baseStat.elementType;

        ResetKnockbackFlags();
    }

    public void ResetKnockbackFlags() // 넉백 플래그를 초기화하는 함수
    {
        hasTriggeredHalfHPHit = false;
        hasTriggeredQuarterHPHit = false;
    }

    public void ApplyDamage(float damage) // 데미지를 적용하고 이벤트를 실행하는 함수
    {
        if (currentHP <= 0)
            return;

        currentHP = Mathf.Clamp(currentHP - damage, 0f, maxHP);
        OnDamageTaken?.Invoke(damage);

        if (currentHP <= 0)
        {
            OnDied?.Invoke();
            return;
        }

        TryTriggerKnockback();
    }

    private void TryTriggerKnockback() // 넉백 트리거 발동 시 이벤트를 실행하는 함수
    {
        float ratio = currentHP / maxHP;

        if (ratio <= 0.25f && !hasTriggeredQuarterHPHit)
        {
            hasTriggeredQuarterHPHit = true;
            OnHpThresholdCrossed?.Invoke();
        }
        else if (ratio <= 0.5f && !hasTriggeredHalfHPHit)
        {
            hasTriggeredHalfHPHit = true;
            OnHpThresholdCrossed?.Invoke();
        }
    }

    public float CalculateDamageAgainst(ElementType defenderElement) // 속성 상성을 적용한 데미지를 계산하는 함수
    {
        if (baseStat == null)
            return attackDamage;

        return baseStat.CalculateDamage(elementType, defenderElement, attackDamage);
    }
}