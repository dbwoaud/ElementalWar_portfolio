using System;
using UnityEngine;

[DisallowMultipleComponent]
public class UnitStats : MonoBehaviour
{
    [Header("기본 데이터")]
    [SerializeField] private UnitStat baseStat;

    [Header("유닛 능력치")]
    [SerializeField] private float maxHP;
    [SerializeField] private float currentHP;
    [SerializeField] private float attackDamage;
    [SerializeField] private float firstAttackInterval;
    [SerializeField] private float attackInterval;
    [SerializeField] private float attackRange;
    [SerializeField] private float moveSpeed;
    [SerializeField] private float aoeRadius;
    [SerializeField] private float spawnCost;
    [SerializeField] private ElementType elementType;

    private bool hasTriggeredHalfHPHit;
    private bool hasTriggeredQuarterHPHit;

    public float MaxHP => maxHP;
    public float CurrentHP => currentHP;
    public bool IsAlive => currentHP > 0;
    public float AttackDamage => attackDamage;
    public float FirstAttackInterval => firstAttackInterval;
    public float AttackInterval => attackInterval;
    public float AttackRange => attackRange;
    public float MoveSpeed => moveSpeed;
    public float AoeRadius => aoeRadius;
    public float SpawnCost => spawnCost;
    public ElementType ElementType => elementType;

    public event Action OnKnockBackRequested; // 유닛 넉백 요청 이벤트
    public event Action OnDied; // 유닛 사망 이벤트
 

    public void InitializeUnitStat() // 유닛 능력치를 초기화하는 함수
    {
        if (baseStat == null)
            return;

        maxHP = baseStat.MaxHP;
        currentHP = maxHP;
        attackDamage = baseStat.AttackDamage;
        firstAttackInterval = baseStat.FirstAttackDelay;
        attackInterval = baseStat.AttackInterval;
        attackRange = baseStat.AttackRange;
        moveSpeed = baseStat.MoveSpeed;
        aoeRadius = baseStat.AoeRadius;
        spawnCost = baseStat.SpawnCost;
        elementType = baseStat.ElementType;
        ResetKnockbackFlags();
    }

    private void ResetKnockbackFlags() // 넉백 플래그를 초기화하는 함수
    {
        hasTriggeredHalfHPHit = false;
        hasTriggeredQuarterHPHit = false;
    }

    public void ApplyDamage(float damage) // 유닛에게 데미지를 적용하는 함수
    {
        if (currentHP <= 0)
            return;

        currentHP = Mathf.Clamp(currentHP - damage, 0f, maxHP);
        if (currentHP <= 0)
        {
            OnDied?.Invoke();
            return;
        }

        TriggerKnockback();
    }

    private void TriggerKnockback() // 넉백 트리거를 발동시키는 함수
    {
        float ratio = currentHP / maxHP;
        if (ratio <= 0.25f && !hasTriggeredQuarterHPHit)
        {
            hasTriggeredQuarterHPHit = true;
            OnKnockBackRequested?.Invoke();
        }
        else if (ratio <= 0.5f && !hasTriggeredHalfHPHit)
        {
            hasTriggeredHalfHPHit = true;
            OnKnockBackRequested?.Invoke();
        }
    }

    public float CalculateDamage(ElementType targetUnitElementType) // 속성 상성을 적용한 데미지를 계산하는 함수
    {
        return UnitStat.CalculateDamage(elementType, targetUnitElementType, attackDamage);
    }
}