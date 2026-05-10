using UnityEngine;

[System.Serializable]
public class UnitStateAttack : IUnitState
{
    public UnitStateType Type => UnitStateType.Attack;

    private enum AttackPhase { WaitingFirst, Attacking, Interval }

    [Header("내부 상태 추적")]
    [SerializeField] private AttackPhase phase;
    [SerializeField] private float phaseTimer;
    [SerializeField] private float currentAnimDuration;
    [SerializeField] private Collider2D currentTarget;

    public void EnterState(Unit unit) // 유닛이 공격 상태에 들어왔을 때 실행되는 함수
    {
        if (unit == null)
            return;

        phase = AttackPhase.WaitingFirst;
        phaseTimer = 0f;
        currentTarget = null;
        currentAnimDuration = 0f;

        unit.Animator?.PlayIdle();
        unit.StopUnit();
    }

    public void UpdateState(Unit unit) // 유닛이 공격 상태 중일 때 실행되는 함수
    {
        if (unit == null) 
            return;

        unit.StopUnit();
        switch (phase)
        {
            case AttackPhase.WaitingFirst: 
                TickWaitingFirst(unit); 
                break;
            case AttackPhase.Attacking: 
                TickAttacking(unit);
                break;
            case AttackPhase.Interval: 
                TickInterval(unit); 
                break;
        }
    }

    private void TickWaitingFirst(Unit unit) // 적 인식 후 첫 공격까지의 딜레이를 처리하는 함수
    {
        if (!unit.HasValidTarget())
        {
            unit.ChangeState(unit.StateMove);
            return;
        }

        phaseTimer += Time.deltaTime;
        if (phaseTimer >= unit.FirstAttackDelay)
            BeginAttackCycle(unit);
    }

    private void BeginAttackCycle(Unit unit) // 새로운 공격 사이클을 시작하고 타겟을 저장하는 함수
    {
        if (!unit.IsAlive) 
            return;

        currentTarget = unit.AcquirePrimaryTarget();
        if (currentTarget == null)
        {
            unit.ChangeState(unit.StateMove);
            return;
        }

        phase = AttackPhase.Attacking;
        phaseTimer = 0f;
        currentAnimDuration = unit.PlayAttackAnimation();
    }

    private void TickAttacking(Unit unit) // 공격 애니메이션 진행을 처리하는 함수
    {

        phaseTimer += Time.deltaTime;
        if (phaseTimer < currentAnimDuration) 
            return;

        TryApplyDamage(unit);

        if (!unit.HasValidTarget())
        {
            unit.ChangeState(unit.StateMove);
            return;
        }

        phase = AttackPhase.Interval;
        phaseTimer = 0f;
    }

    private void TryApplyDamage(Unit unit) // 공격 애니메이션 종료 시 데미지 적용 가능 여부를 검증하고 적용하는 함수
    {
        if (currentTarget == null) 
            return;

        if (!unit.IsColliderTargetable(currentTarget)) 
            return;

        unit.ApplyDamageFromAttack(currentTarget);
    }

    private void TickInterval(Unit unit) // 공격 애니메이션 종료 후 다음 공격까지의 딜레이를 처리하는 함수
    {
        if (!unit.HasValidTarget())
        {
            unit.ChangeState(unit.StateMove);
            return;
        }

        phaseTimer += Time.deltaTime;
        if (phaseTimer < unit.AttackInterval) 
            return;

        BeginAttackCycle(unit);
    }

    public void ExitState(Unit unit) // 유닛이 공격 상태가 끝났을 때 실행되는 함수
    {
        currentTarget = null;
    }
}