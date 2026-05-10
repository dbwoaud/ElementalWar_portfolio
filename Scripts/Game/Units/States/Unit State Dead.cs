using UnityEngine;

[System.Serializable]
public class UnitStateDead : IUnitState
{
    public UnitStateType Type => UnitStateType.Dead;

    [Header("죽음 연출 설정")]
    [SerializeField] private const float FadeOutDuration = 1.5f;
    [SerializeField] private const float DestroyDelay = 2.0f;


    public void EnterState(Unit unit) // 유닛이 죽음 상태에 들어왔을 때 실행되는 함수
    {
        if (unit == null)
            return;

        unit.Animator?.PlayDead();
        SoundManager.instance?.Play(SoundKey.UnitDie);
        unit.DisableAllPhysics();
        unit.Animator?.StartFadeOut(FadeOutDuration);
        unit.ScheduleDestruction(DestroyDelay);
    }

    public void UpdateState(Unit unit) { } // 유닛이 죽음 상태 중일 때 실행되는 함수
    public void ExitState(Unit unit) { } // 유닛이 죽음 상태가 끝났을 때 실행되는 함수
}