using Photon.Pun;
using System.Collections;
using UnityEngine;

[DisallowMultipleComponent]
[RequireComponent(typeof(PhotonView))]
public class UnitNetworkSync : MonoBehaviour
{
    [Header("유닛 관련 변수")]
    [SerializeField] private Unit unit;
    [SerializeField] private UnitStateMachine stateMachine;
    [SerializeField] private UnitMovement movement;
    [SerializeField] private UnitCombat combat;
    [SerializeField] private IUnitAnimator unitAnimator;

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
        if (unitAnimator == null) 
            unitAnimator = GetComponent<IUnitAnimator>();
    }

    private void OnEnable()
    {
        if (stateMachine != null)
            stateMachine.OnStateChanged += BroadcastStateChange;
    }

    private void OnDisable()
    {
        if (stateMachine != null)
            stateMachine.OnStateChanged -= BroadcastStateChange;

        pendingState = null;
    }

    public void ConfigureNetworkRole() // 네트워크 풀에서 생성된 이후 방향과 레이어를 설정하는 함수
    {
        SetDirection();
        SetLayer();
        SetPhysicsRole();
    }

    private void SetDirection() // 플레이어 유닛의 진행 방향과 캐릭터 좌우반전을 설정하는 함수
    {
        bool ownerIsMaster = PhotonView.Owner.IsMasterClient;
        float dir = ownerIsMaster ? 1f : -1f;
        movement.SetDirection(dir);

        bool unitFacesLeft = !ownerIsMaster;
        unitAnimator?.SetDirection(unitFacesLeft);
    }

    private void SetLayer() // 플레이어 유닛의 레이어를 설정하는 함수
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

    public void BroadcastAttackAnimation() // 공격 애니메이션 재생을 다른 플레이어에 동기화하는 함수
    {
        if (!IsOwnedByLocalPlayer)
            return;

        SendStateRpc(UnitStateType.Attack);
    }

    private void BroadcastStateChange(IUnitState nextState) // 상태 전이를 다른 플레이어에 동기화하는 함수
    {
        if (!IsOwnedByLocalPlayer || nextState == null)
            return;

        if (nextState.Type == UnitStateType.Idle)
        {
            pendingState = nextState;
            pendingSince = Time.time;
            return;
        }

        pendingState = null;
        SendStateRpc(nextState.Type);
    }

    public void ScheduleDestruction(float delay) // 일정 딜레이 후 오브젝트 파괴를 예약하는 함수
    {
        if (IsOwnedByLocalPlayer)
            StartCoroutine(DestroyAfterDelay(delay));
    }

    private IEnumerator DestroyAfterDelay(float delay) // 일정 딜레이 후 오브젝트 파괴를 예약하는 코루틴
    {
        yield return new WaitForSeconds(delay);
        if (PhotonView.IsMine && gameObject.activeInHierarchy)
            PhotonNetwork.Destroy(gameObject);
    }

    private void SetPhysicsRole() // 물리 역할을 설정하는 함수
    {
        movement.SetPhysicsRole(IsOwnedByLocalPlayer);
    }

    [Header("경유 상태 전송 지연")]
    private IUnitState pendingState;
    private float pendingSince;
    private const float IdleBroadcastDelay = 0.05f;

    /* Idle 은 진입 즉시 스캔해 Move 나 Attack 으로 빠지는 경유 상태다.
       1프레임만 머무는 전이까지 전송하면 상대 화면에 보이지도 않는 RPC 가 낭비되므로,
       짧은 지연 후에도 여전히 Idle 이면 그때 전송한다. */


    private void Update()
    {
        if (pendingState == null)
            return;

        if (Time.time - pendingSince < IdleBroadcastDelay)
            return;

        SendStateRpc(pendingState.Type);
        pendingState = null;
    }

    private void SendStateRpc(UnitStateType type) // 상태 전이 RPC 를 실제로 발행하는 함수
    {
        ProfilingCounters.CountRpcSent();
        PhotonView.RPC(nameof(Unit.RPC_SyncAnimation), RpcTarget.Others, (int)type);
    }
}
