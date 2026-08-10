using Photon.Pun;
using System.Collections;
using UnityEngine;

[DisallowMultipleComponent]
[RequireComponent(typeof(PhotonView))]
[RequireComponent(typeof(Unit))]
[RequireComponent(typeof(UnitStateMachine))]
[RequireComponent(typeof(UnitMovement))]
[RequireComponent(typeof(UnitCombat))]
public class UnitNetworkSync : MonoBehaviour
{
    [Header("소유 유닛")]
    [SerializeField] private Unit unit;

    [Header("유닛 컴포넌트")]
    [SerializeField] private UnitStateMachine stateMachine;
    [SerializeField] private UnitMovement movement;
    [SerializeField] private UnitCombat combat;
    private IUnitAnimator unitAnimator;

    public PhotonView PhotonView { get; private set; }
    public bool IsOwnedByLocalPlayer => PhotonView.IsMine;
    public int OwnLayerMask { get; private set; }
    public int TargetLayerMask { get; private set; }


    private void Awake()
    {
        PhotonView = GetComponent<PhotonView>();

        if (unit == null) 
            unit = GetComponent<Unit>();

        if (stateMachine == null) 
            stateMachine = GetComponent<UnitStateMachine>();

        if (movement == null) 
            movement = GetComponent<UnitMovement>();

        if (combat == null) 
            combat = GetComponent<UnitCombat>();

        unitAnimator ??= GetComponent<IUnitAnimator>();
    }

    private void OnEnable()
    {
        if (stateMachine != null)
            stateMachine.OnStateChanged += HandleStateChange;
    }

    private void OnDisable()
    {
        if (stateMachine != null)
            stateMachine.OnStateChanged -= HandleStateChange;
    }

    private void HandleStateChange(IUnitState nextState) // 유닛의 상태 변경을 처리하는 함수
    {
        if (!IsOwnedByLocalPlayer || nextState == null)
            return;
        
        BroadcastStateRpc(nextState.Type);
    }

    private void BroadcastStateRpc(UnitStateType type) // 다른 플레이어에게 변경된 유닛 상태를 전파하는 함수
    {
        ProfilingCounters.CountRpcSent();
        PhotonView.RPC(nameof(Unit.RPC_SyncAnimation), RpcTarget.Others, (int)type);
    }

    public void ResetForReuse() // 재사용을 위해 유닛 설정을 초기화하는 함수
    {
        SetDirection();
        SetLayer();
        SetPhysics();
    }

    private void SetDirection() // 유닛의 방향을 설정하는 함수
    {
        bool isMaster = PhotonView.Owner?.IsMasterClient ?? PhotonView.IsMine;
        movement?.SetDirection(isMaster ? 1f : -1f);
        unitAnimator?.SetDirection(!isMaster);
    }

    private void SetLayer() // 유닛의 레이어를 설정하는 함수
    {
        if (IsOwnedByLocalPlayer)
        {
            OwnLayerMask = LayerMask.NameToLayer(GameSystem.UnitConstants.PlayerLayer);
            TargetLayerMask = LayerMask.GetMask(GameSystem.UnitConstants.EnemyLayer, GameSystem.CastleConstants.EnemyLayer);
        }
        else
        {
            OwnLayerMask = LayerMask.NameToLayer(GameSystem.UnitConstants.EnemyLayer);
            TargetLayerMask = LayerMask.GetMask(GameSystem.UnitConstants.PlayerLayer, GameSystem.CastleConstants.PlayerLayer);
        }

        gameObject.layer = OwnLayerMask;
        combat.TargetLayerMask = TargetLayerMask;
    }

    private void SetPhysics() // 유닛의 물리를 설정하는 함수
    {
        movement.SetPhysics(IsOwnedByLocalPlayer);
    }

    public void BroadcastAttackAnimation() // 공격 애니메이션 재생을 다른 플레이어와 동기화하는 함수
    {
        if (!IsOwnedByLocalPlayer)
            return;

        BroadcastStateRpc(UnitStateType.Attack);
    }

    public void Despawn(float delay) // 유닛을 제거하는 함수
    {
        if (IsOwnedByLocalPlayer)
            StartCoroutine(DespawnCoroutine(delay));
    }

    private IEnumerator DespawnCoroutine(float delay) // 유닛을 제거하는 코루틴
    {
        yield return new WaitForSeconds(delay);
        if (IsOwnedByLocalPlayer && gameObject.activeInHierarchy)
            PhotonNetwork.Destroy(gameObject);
    }
}