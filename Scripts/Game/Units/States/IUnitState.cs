public interface IUnitState
{
    UnitStateType Type { get; }

    void EnterState(Unit unit);   
    void UpdateState(Unit unit);
    void ExitState(Unit unit);
}