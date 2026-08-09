public interface IUnitState
{
    UnitStateType Type { get; }

    void EnterState(Unit unit); // 유닛이 특정 상태에 진입할 때 실행되는 함수 
    void UpdateState(Unit unit); // 유닛이 특정 상태를 유지하는 동안 실행되는 함수
    void ExitState(Unit unit); // 유닛이 특정 상태에서 벗어날 때 실행되는 함수
}