
using UnityEngine;

[System.Serializable]
public class UnitStateMove : IUnitState
{
    public UnitStateType Type => UnitStateType.Move;

    [Header("스캔 변수")]
    [SerializeField] private const float ScanInterval = 0.1f;
    [SerializeField] private float lastScanTime;

    public void EnterState(Unit unit) // 유닛이 이동 상태에 들어왔을 때 실행되는 함수
    {
        unit.Animator?.PlayMove();
        lastScanTime = -ScanInterval;
    }

    public void UpdateState(Unit unit) // 유닛이 이동 상태 중일 때 실행되는 함수
    {
        if (unit == null) 
            return;

        if (Time.time - lastScanTime >= ScanInterval)
        {
            lastScanTime = Time.time;
            if (unit.HasValidTarget())
            {
                unit.ChangeState(unit.StateAttack);
                return;
            }
        }

        unit?.MoveUnit();
    }

    public void ExitState(Unit unit) // 유닛이 이동 상태가 끝났을 때 실행되는 함수
    {
        unit?.StopUnit();
    }
}