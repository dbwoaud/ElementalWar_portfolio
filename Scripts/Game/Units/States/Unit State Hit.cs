using UnityEngine;

[System.Serializable]
public class UnitStateHit : IUnitState
{
    public UnitStateType Type => UnitStateType.Hit;

    private float hitTimer;


    public void EnterState(Unit unit) // 유닛이 피격 상태에 진입할 때 실행되는 함수
    {
        if (unit == null)
            return;

        hitTimer = 0f;
        unit.Animator?.PlayHit();
        SoundManager.Instance?.Play(SoundKey.UnitHit);
        unit.ApplyKnockback();
    }

    public void UpdateState(Unit unit) // 유닛이 피격 상태를 유지하는 동안 실행되는 함수
    {
        if (unit == null)
            return;

        hitTimer += Time.deltaTime;
        if (hitTimer >= unit.KnockbackDuration)
            unit?.ChangeState(unit.StateIdle);
    }

    public void ExitState(Unit unit) // 유닛이 피격 상태에서 벗어날 때 실행되는 함수
    {
        unit?.StopMovement();
    }
}