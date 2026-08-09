using Photon.Pun;
using UnityEngine;

[DisallowMultipleComponent]
[RequireComponent(typeof(UnitStats))]
[RequireComponent(typeof(UnitMovement))]
[RequireComponent(typeof(UnitCombat))]
[RequireComponent(typeof(UnitStateMachine))]
[RequireComponent(typeof(UnitNetworkSync))]
public class Unit : MonoBehaviourPun, IDamagable, IPunInstantiateMagicCallback
{
    [Header("유닛 컴포넌트")]
    [SerializeField] private UnitStats stats;
    [SerializeField] private UnitMovement movement;
    [SerializeField] private UnitCombat combat;
    [SerializeField] private UnitStateMachine stateMachine;
    [SerializeField] private UnitNetworkSync networkSync;
    [SerializeField] private IUnitAnimator animator;

    private const int ZOffsetSlotCount = 200;
    private const float ZOffsetRange = 1f;
    private const float KillRewardEnergyRate = 0.25f;

    public UnitStats Stats => stats;
    public UnitMovement Movement => movement;
    public UnitCombat Combat => combat;
    public UnitStateMachine StateMachine => stateMachine;
    public UnitNetworkSync NetworkSync => networkSync;
    public IUnitAnimator Animator => animator;
    public float MaxHP => stats.MaxHP;
    public float CurrentHP => stats.CurrentHP;
    public bool IsAlive => stats.IsAlive && stateMachine.CurrentState != stateMachine.StateDead;
    public bool IsTargetable => IsAlive && stateMachine.CurrentState != stateMachine.StateHit;
    public UnitStateIdle StateIdle => stateMachine.StateIdle;
    public UnitStateMove StateMove => stateMachine.StateMove;
    public UnitStateAttack StateAttack => stateMachine.StateAttack;
    public UnitStateHit StateHit => stateMachine.StateHit;
    public UnitStateDead StateDead => stateMachine.StateDead;
    public float FirstAttackInterval => stats.FirstAttackInterval;
    public float AttackInterval => stats.AttackInterval;
    public float KnockbackDuration => movement.KnockbackDuration;


    private void Awake()
    {
        InitializeComponents();
    }

    private void InitializeComponents() // 모든 유닛 컴포넌트를 초기화하는 함수
    {
        if (stats == null) 
            stats = GetComponent<UnitStats>();

        if (movement == null) 
            movement = GetComponent<UnitMovement>();

        if (combat == null) 
            combat = GetComponent<UnitCombat>();

        if (stateMachine == null) 
            stateMachine = GetComponent<UnitStateMachine>();

        if (networkSync == null) 
            networkSync = GetComponent<UnitNetworkSync>();

        if (animator == null) 
            animator = GetComponent<IUnitAnimator>();
    }

    private void OnEnable()
    {
        if (stats != null)
        {
            stats.OnDied += HandleDied;
            stats.OnKnockBackRequested += HandleTriggerKnockback;
        }
        UnitRegistry.Register(this);
    }

    private void OnDisable()
    {
        if (stats != null)
        {
            stats.OnDied -= HandleDied;
            stats.OnKnockBackRequested -= HandleTriggerKnockback;
        }
        UnitRegistry.Unregister(this);
    }

    private void Update()
    {
        if (ProfilingSwitches.TickOnlyOwnedUnits && !networkSync.IsOwnedByLocalPlayer)
            return;

        ProfilingCounters.CountUnitTick();
        stateMachine.UpdateState();
    }

    public void OnPhotonInstantiate(PhotonMessageInfo info) // 유닛 소환 시 실행되는 함수
    {
        stats?.InitializeUnitStat();
        ResetTransform();
        movement?.ResetForReuse();
        animator?.ResetForReuse();
        networkSync?.ResetForReuse();
        stateMachine?.InitializeState();
    }

    private void ResetTransform() // 유닛 소환 위치를 초기화하는 함수
    {
        Vector3 pos = transform.position;
        int slot = Mathf.Abs(photonView.ViewID) % ZOffsetSlotCount;
        pos.z = (slot / (float)(ZOffsetSlotCount - 1)) * (ZOffsetRange * 2f) - ZOffsetRange;
        transform.position = pos;
    }

    private void HandleDied() // 유닛 사망을 처리하는 함수
    {
        if (!networkSync.IsOwnedByLocalPlayer)
            EnergyManager.Instance?.AddEnergy(stats.SpawnCost * KillRewardEnergyRate);

        stateMachine.ChangeState(stateMachine.StateDead);
    }

    private void HandleTriggerKnockback() // 넉백 트리거를 처리하는 함수
    {
        if (!IsTargetable)
            return;

        stateMachine.ChangeState(stateMachine.StateHit);
    }

    public void ChangeState(IUnitState nextState, bool isSync = false) // 유닛 상태를 변경하는 함수
    {
        stateMachine.ChangeState(nextState, isSync);
    }

    public bool CheckValidEnemy() // 공격 사거리 내 적 유닛이 있는지 확인하는 함수
    {
       return combat.CheckValidEnemy();
    }

    public Collider2D FindValidEnemy() // 사거리 안의 우선 적을 반환하는 함수
    {
        return combat.FindValidEnemy();
    }

    public bool IsAttackableEnemy(Collider2D col) // 공격 가능한 적 콜라이더인지 확인하는 함수
    {
        return combat.IsAttackableEnemy(col);
    }

    public void ApplyDamage(Collider2D origin) // 데미지를 적용하는 함수
    {
        combat.ApplyDamage(origin);
    }

    public void ApplyMovement() // 유닛 이동을 적용하는 함수
    {
        movement.ApplyMovement();
    }

    public void StopMovement() // 유닛 이동을 정지하는 함수
    {
        movement.StopMovement();
    }

    public void ApplyKnockback() // 유닛 피격 시 넉백 연출을 적용하는 함수
    {
        movement.ApplyKnockback();
    }

    public void DisableAllPhysics() // 유닛 사망 시 모든 물리 컴포넌트를 비활성화하는 함수
    {
        movement.DisableAllPhysics();
    }

    public void Despawn(float delay) // 유닛을 제거하는 함수
    {
        networkSync.Despawn(delay);
    }

    public float PlayAttackAnimation() // 공격 애니메이션을 재생하고 다른 플레이어와 동기화하는 함수
    {
        networkSync.BroadcastAttackAnimation();
        SoundManager.Instance?.Play(SoundKey.UnitAttack);
        return animator?.PlayAttack() ?? 0.5f;
    }

    [PunRPC]
    public void RPC_SyncAnimation(int stateTypeInt) // 애니메이션 재생을 다른 플레이어와 동기화하는 함수
    {
        var targetType = (UnitStateType)stateTypeInt;
        if (!stateMachine.TryGetState(targetType, out IUnitState targetState))
            return;

        if (stateMachine.CurrentState == targetState)
            PlayAnimationByType(targetType);
        else
            stateMachine.ChangeState(targetState, true);
    }

    private void PlayAnimationByType(UnitStateType type) // 열거형에 연결된 애니메이션을 재생하는 함수
    {
        if (animator == null)
            return;

        switch (type)
        {
            case UnitStateType.Idle: animator.PlayIdle(); break;
            case UnitStateType.Move: animator.PlayMove(); break;
            case UnitStateType.Attack: animator.PlayAttack(); break;
            case UnitStateType.Hit: animator.PlayHit(); break;
            case UnitStateType.Dead: animator.PlayDead(); break;
        }
    }

    [PunRPC]
    public void RPC_TakeDamage(float damage) // 유닛에게 적용된 데미지를 다른 플레이어와 동기화하는 함수
    {
        ProfilingCounters.CountRpcReceivedDamage();

        if (!stats.IsAlive)
            return;

        stats.ApplyDamage(damage);
    }
}