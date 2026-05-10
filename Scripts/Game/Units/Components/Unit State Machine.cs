using System;
using System.Collections.Generic;
using UnityEngine;

[DisallowMultipleComponent]
public class UnitStateMachine : MonoBehaviour
{
    [Header("유닛 관련 변수")]
    [SerializeField] private Unit unit;

    [Header("상태 변수")]
    [SerializeField] private UnitStateIdle stateIdle = new UnitStateIdle();
    [SerializeField] private UnitStateMove stateMove = new UnitStateMove();
    [SerializeField] private UnitStateAttack stateAttack = new UnitStateAttack();
    [SerializeField] private UnitStateHit stateHit = new UnitStateHit();
    [SerializeField] private UnitStateDead stateDead = new UnitStateDead();

    public UnitStateIdle StateIdle => stateIdle;

    public UnitStateMove StateMove => stateMove;

    public UnitStateAttack StateAttack => stateAttack;

    public UnitStateHit StateHit => stateHit;

    public UnitStateDead StateDead => stateDead;

    private IUnitState currentState;

    public IUnitState CurrentState => currentState;

    private Dictionary<UnitStateType, IUnitState> stateDictionary;
    public event Action<IUnitState> OnStateChanged;


    private void Awake()
    {
        if (unit == null)
            unit = GetComponent<Unit>();

        BuildDictionary();
    }

    private void BuildDictionary() // 상태 열거형과 상태 변수를 연결하는 함수
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

    public void StartFromIdle() // 유닛 생성 후 상태를 대기 상태로 설정하는 함수
    {
        currentState = null;
        ChangeState(stateIdle);
    }

    public void Tick() // 매 프레임마다 상태를 업데이트하는 함수
    {
        currentState?.UpdateState(unit);
    }

    public void ChangeState(IUnitState nextState, bool isSync = false) // 상태를 전이시키는 함수
    {
        if (currentState == nextState)
            return;

        currentState?.ExitState(unit);
        currentState = nextState;
        currentState?.EnterState(unit);

        if (!isSync)
            OnStateChanged?.Invoke(nextState);
    }

    public bool TryGetStateByType(UnitStateType type, out IUnitState state) // 상태 열거형으로 상태 변수를 조회하는 함수
    {
        return stateDictionary.TryGetValue(type, out state);
    }
}