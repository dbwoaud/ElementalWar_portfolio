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
    [Header("유닛 관련 변수")]
    [SerializeField] private UnitStats stats;
    [SerializeField] private UnitMovement movement;
    [SerializeField] private UnitCombat combat;
    [SerializeField] private UnitStateMachine stateMachine;
    [SerializeField] private UnitNetworkSync networkSync;
    [SerializeField] private IUnitAnimator animator;

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

    public float FirstAttackDelay => stats.FirstAttackDelay;

    public float AttackInterval => stats.AttackInterval;

    public float KnockbackDuration => movement.KnockbackDuration;


    private void Awake()
    {
        ResolveComponents();
    }

    private void ResolveComponents() // 모든 유닛 관련 변수를 저장하는 함수
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
            stats.OnHpThresholdCrossed += HandleKnockbackHPCrossed;
        }
        UnitRegistry.Register(this);
    }

    private void OnDisable()
    {
        if (stats != null)
        {
            stats.OnDied -= HandleDied;
            stats.OnHpThresholdCrossed -= HandleKnockbackHPCrossed;
        }
        UnitRegistry.Unregister(this);
    }

    private void Update()
    {
        if (!networkSync.IsOwnedByLocalPlayer)
            return;
        stateMachine.Tick();
    }

    public void OnPhotonInstantiate(PhotonMessageInfo info) // 네트워크 풀에서 꺼낼 때마다 호출되는 함수
    {
        stats.InitializeFromBaseStat();
        ResetTransform();
        networkSync.ConfigureNetworkRole();
        movement.ResetForReuse();
        animator?.ResetForReuse();
        stateMachine.StartFromIdle();
    }

    private void ResetTransform() // 유닛 소환 위치를 초기화하는 함수
    {
        Vector3 pos = transform.position;
        pos.z = Random.Range(-1f, 1f);
        transform.position = pos;
    }

    public void ChangeState(IUnitState nextState, bool isSync = false) // 상태를 이동하는 함수
    {
        stateMachine.ChangeState(nextState, isSync);
    }

    public bool HasValidTarget() // 사거리 안에 공격 가능한 적이 있는지 확인하는 함수
    {
       return combat.HasValidTarget();
    }

    public Collider2D AcquirePrimaryTarget() // 사거리 안의 우선 적을 반환하는 함수
    {
        return combat.AcquirePrimaryTarget();
    }

    public bool IsTargetInRange(Collider2D target) // 지정된 타겟이 현재 사거리 내에 있는지 확인하는 함수
    {
        return combat.IsTargetInRange(target);
    }

    public bool IsColliderTargetable(Collider2D col) // 현재 타겟의 콜라이더가 데미지를 받을 수 있는 상태인지 확인하는 함수
    {
        return combat.IsColliderTargetable(col);
    }

    public void ApplyDamageFromAttack(Collider2D origin) // 공격으로부터 실제 데미지를 적용하는 함수
    {
        combat.ApplyDamageFromAttack(origin);
    }

    public void MoveUnit() // 유닛을 앞으로 이동시키는 함수
    {
        movement.MoveForward();
    }

    public void StopUnit() // 유닛을 정지시키는 함수
    {
        movement.Stop();
    }

    public void ApplyKnockback() // 유닛에게 넉백 효과를 적용하는 함수
    {
        movement.ApplyKnockback();
    }

    public void DisableAllPhysics() // 유닛 사망 시 물리 상태를 차단하는 함수
    {
        movement.DisableAllPhysics();
    }

    public void ScheduleDestruction(float delay) // 일정 딜레이 후 오브젝트 파괴를 예약하는 함수
    {
        networkSync.ScheduleDestruction(delay);
    }

    public float PlayAttackAnimation() // 공격 애니메이션을 재생하고 다른 플레이어와 동기화하는 함수
    {
        networkSync.BroadcastAttackAnimation();
        SoundManager.instance?.Play(SoundKey.UnitAttack);
        return animator?.PlayAttack() ?? 0.5f;
    }

    private void HandleDied() // 유닛의 죽음을 처리하는 함수
    {
        if (!networkSync.IsOwnedByLocalPlayer)
            EnergyManager.instance?.AddEnergy(stats.SpawnCost * 0.25f);

        stateMachine.ChangeState(stateMachine.StateDead);
    }

    private void HandleKnockbackHPCrossed() // 넉백 조건 도달 시 피격 상태로 전이하는 함수
    {
        if (stateMachine.CurrentState == stateMachine.StateHit)
            return;

        stateMachine.ChangeState(stateMachine.StateHit);
    }

    [PunRPC]
    public void RPC_SyncAnimation(int stateTypeInt) // 유닛의 상태를 다른 플레이어와 동기화하는 함수
    {
        var targetType = (UnitStateType)stateTypeInt;
        if (!stateMachine.TryGetStateByType(targetType, out IUnitState targetState))
            return;

        if (stateMachine.CurrentState == targetState)
            PlayAnimationByType(targetType);
        
        else
            stateMachine.ChangeState(targetState, isSync: true);
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
    public void RPC_TakeDamage(float damage) // 유닛에게 적용된 데미지를 동기화하는 함수
    {
        if (!IsTargetable)
            return;

        stats.ApplyDamage(damage);
    }
}