using UnityEngine;

[System.Serializable]
public class UnitStateIdle : IUnitState
{
    public UnitStateType Type => UnitStateType.Idle;

    private static float ScanInterval => ProfilingSwitches.ScanInterval;
    private float lastScanTime;


    public void EnterState(Unit unit) // 유닛이 대기 상태에 진입할 때 실행되는 함수
    {
        if (unit == null)
            return;

        unit.Animator?.PlayIdle();
        unit.StopMovement();
        lastScanTime = -ScanInterval;
    }

    public void UpdateState(Unit unit) // 유닛이 대기 상태를 유지하는 동안 실행되는 함수
    {
        if (unit == null) 
            return;

        if (Time.time - lastScanTime < ScanInterval)
            return;

        lastScanTime = Time.time;

        if (unit.CheckValidEnemy())
            unit.ChangeState(unit.StateAttack);
        else
            unit.ChangeState(unit.StateMove);
    }

    public void ExitState(Unit unit) // 유닛이 대기 상태에서 벗어날 때 실행되는 함수
    {
        
    } 
}