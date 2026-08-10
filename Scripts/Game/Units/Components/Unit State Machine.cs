using System;
using System.Collections.Generic;
using UnityEngine;

[DisallowMultipleComponent]
public class UnitStateMachine : MonoBehaviour
{
    [Header("소유 유닛")]
    [SerializeField] private Unit unit;

    [Header("상태 변수")]
    [SerializeField] private UnitStateIdle stateIdle = new();
    [SerializeField] private UnitStateMove stateMove = new();
    [SerializeField] private UnitStateAttack stateAttack = new();
    [SerializeField] private UnitStateHit stateHit = new();
    [SerializeField] private UnitStateDead stateDead = new();

    [Header("현재 상태")]
    [SerializeField] private UnitStateType currentStateType;
    private IUnitState currentState;

    public UnitStateIdle StateIdle => stateIdle;
    public UnitStateMove StateMove => stateMove;
    public UnitStateAttack StateAttack => stateAttack;
    public UnitStateHit StateHit => stateHit;
    public UnitStateDead StateDead => stateDead;
    public IUnitState CurrentState => currentState;

    private Dictionary<UnitStateType, IUnitState> stateDictionary;

    public event Action<IUnitState> OnStateChanged; // 유닛 상태 변경 이벤트


    private void Awake()
    {
        if (unit == null)
            unit = GetComponent<Unit>();

        InitializeDictionary();
    }

    private void InitializeDictionary() // 상태 열거형과 상태 변수를 연결하는 함수
    {
        stateDictionary = new Dictionary<UnitStateType, IUnitState>(5)
        {
            { UnitStateType.Idle,   stateIdle   },
            { UnitStateType.Move,   stateMove   },
            { UnitStateType.Attack, stateAttack },
            { UnitStateType.Hit,    stateHit    },
            { UnitStateType.Dead,   stateDead   },
        };
    }

    public void InitializeState() // 유닛 상태를 초기화하는 함수
    {
        currentState = null;
        currentStateType = UnitStateType.Idle;
        ChangeState(stateIdle);
    }

    public void UpdateState() // 유닛 상태를 업데이트하는 함수
    {
        currentState?.UpdateState(unit);
    }

    public void ChangeState(IUnitState nextState, bool isSync = false) // 유닛 상태를 변경하는 함수
    {
        if (nextState == null || currentState == nextState)
            return;

        currentState?.ExitState(unit);
        currentState = nextState;
        currentStateType = nextState.Type;
        currentState.EnterState(unit);

        if (!isSync)
            OnStateChanged?.Invoke(nextState);
    }

    public bool TryGetState(UnitStateType type, out IUnitState state) // 상태 열거형으로 상태 변수를 조회하는 함수
    {
        return stateDictionary.TryGetValue(type, out state);
    }
}