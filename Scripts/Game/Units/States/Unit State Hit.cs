using UnityEngine;

[System.Serializable]
public class UnitStateHit : IUnitState
{
    public UnitStateType Type => UnitStateType.Hit;


    [Header("내부 상태 추적")]
    [SerializeField] private float hitTimer;


    public void EnterState(Unit unit) // 유닛이 피격 상태에 들어왔을 때 실행되는 함수
    {
        if (unit == null)
            return;

        hitTimer = 0f;
        unit.Animator?.PlayHit();
        SoundManager.instance?.Play(SoundKey.UnitHit);
        unit.ApplyKnockback();
    }

    public void UpdateState(Unit unit) // 유닛이 피격 상태 중일 때 실행되는 함수
    {
        if (unit == null)
            return;

        hitTimer += Time.deltaTime;
        if (hitTimer >= unit.KnockbackDuration)
            unit?.ChangeState(unit.StateIdle);
    }

    public void ExitState(Unit unit) // 유닛이 공격 상태가 끝났을 때 실행되는 함수
    {
        unit?.StopUnit();
    }
}