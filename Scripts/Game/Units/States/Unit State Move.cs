
using UnityEngine;

[System.Serializable]
public class UnitStateMove : IUnitState
{
    public UnitStateType Type => UnitStateType.Move;

    private static float ScanInterval => ProfilingSwitches.ScanInterval;
    private float lastScanTime;


    public void EnterState(Unit unit) // 유닛이 이동 상태에 진입할 때 실행되는 함수
    {
        unit.Animator?.PlayMove();
        lastScanTime = -ScanInterval;
    }

    public void UpdateState(Unit unit) // 유닛이 이동 상태를 유지하는 동안 실행되는 함수
    {
        if (unit == null) 
            return;

        if (Time.time - lastScanTime >= ScanInterval)
        {
            lastScanTime = Time.time;
            if (unit.CheckValidEnemy())
            {
                unit.ChangeState(unit.StateAttack);
                return;
            }
        }

        unit.ApplyMovement();
    }

    public void ExitState(Unit unit) // 유닛이 이동 상태에서 벗어날 때 실행되는 함수
    {
        unit.StopMovement();
    }
}