using UnityEngine;

[System.Serializable]
public class UnitStateDead : IUnitState
{
    public UnitStateType Type => UnitStateType.Dead;

    private const float FadeOutDuration = 1.5f;
    private const float DestroyDelay = 2.0f;


    public void EnterState(Unit unit) // 유닛이 사망 상태에 진입할 때 실행되는 함수
    {
        if (unit == null)
            return;

        unit.Animator?.PlayDead();
        SoundManager.Instance?.Play(SoundKey.UnitDie);
        unit.DisableAllPhysics();
        unit.Animator?.StartFadeOut(FadeOutDuration);
        unit.Despawn(DestroyDelay);
    }

    public void UpdateState(Unit unit) // 유닛이 사망 상태를 유지하는 동안 실행되는 함수
    { 

    } 

    public void ExitState(Unit unit) // 유닛이 사망 상태에서 벗어날 때 실행되는 함수
    {

    }
}