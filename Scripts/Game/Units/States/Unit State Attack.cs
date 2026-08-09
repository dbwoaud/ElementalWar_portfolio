using UnityEngine;

[System.Serializable]
public class UnitStateAttack : IUnitState
{
    private enum AttackPhase { WaitingFirst, Attacking, Interval }
    public UnitStateType Type => UnitStateType.Attack;

    [Header("공격 내부 상태 추적")]
    [SerializeField] private AttackPhase phase;
    [SerializeField] private float currentAnimDuration;
    [SerializeField] private Collider2D currentTarget;
    private float phaseTimer;


    public void EnterState(Unit unit) // 유닛이 공격 상태에 진입할 때 실행되는 함수
    {
        if (unit == null)
            return;

        phase = AttackPhase.WaitingFirst;
        phaseTimer = 0f;
        currentTarget = null;
        currentAnimDuration = 0f;

        unit.Animator?.PlayIdle();
        unit.StopMovement();
    }

    public void UpdateState(Unit unit) // 유닛이 공격 상태를 유지하는 동안 실행되는 함수
    {
        if (unit == null) 
            return;

        unit.StopMovement();
        switch (phase)
        {
            case AttackPhase.WaitingFirst: 
                HandleFirstAttackInterval(unit); 
                break;
            case AttackPhase.Attacking: 
                HandleAttacking(unit);
                break;
            case AttackPhase.Interval: 
                HandleAttackInterval(unit); 
                break;
        }
    }

    private void HandleFirstAttackInterval(Unit unit) // 첫 공격 대기 시간을 처리하는 함수
    {
        if (!unit.CheckValidEnemy())
        {
            unit.ChangeState(unit.StateMove);
            return;
        }

        phaseTimer += Time.deltaTime;
        if (phaseTimer >= unit.FirstAttackInterval)
            StartAttack(unit);
    }

    private void StartAttack(Unit unit) // 공격을 시작하는 함수
    {
        if (!unit.IsAlive) 
            return;

        currentTarget = unit.FindValidEnemy();
        if (currentTarget == null)
        {
            unit.ChangeState(unit.StateMove);
            return;
        }

        phase = AttackPhase.Attacking;
        phaseTimer = 0f;
        currentAnimDuration = unit.PlayAttackAnimation();
    }

    private void HandleAttacking(Unit unit) // 공격 애니메이션 진행 및 공격을 처리하는 함수
    {
        phaseTimer += Time.deltaTime;
        if (phaseTimer < currentAnimDuration) 
            return;

        ApplyDamage(unit);

        if (!unit.CheckValidEnemy())
        {
            unit.ChangeState(unit.StateMove);
            return;
        }

        phase = AttackPhase.Interval;
        phaseTimer = 0f;
    }

    private void ApplyDamage(Unit unit) // 목표 검증 후 실제 대미지를 적용하는 함수
    {
        if (currentTarget == null) 
            return;

        if (!unit.IsAttackableEnemy(currentTarget)) 
            return;

        unit.ApplyDamage(currentTarget);
    }

    private void HandleAttackInterval(Unit unit) // 다음 공격 대기 시간을 처리하는 함수
    {
        if (!unit.CheckValidEnemy())
        {
            unit.ChangeState(unit.StateMove);
            return;
        }

        phaseTimer += Time.deltaTime;
        if (phaseTimer < unit.AttackInterval) 
            return;

        StartAttack(unit);
    }

    public void ExitState(Unit unit) // 유닛이 공격 상태에서 벗어날 때 실행되는 함수
    {
        currentTarget = null;
    }
}