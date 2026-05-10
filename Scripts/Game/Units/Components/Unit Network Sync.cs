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
    }

    public void ConfigureNetworkRole() // 네트워크 풀에서 생성된 이후 방향과 레이어를 설정하는 함수
    {
        SetDirection();
        SetLayer();
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
            if (movement.Body != null)
                movement.Body.simulated = true;
        }
        else
        {
            OwnLayerMask = LayerMask.NameToLayer(GameSystem.UnitConstants.EnemyLayer);
            TargetLayerMask = LayerMask.GetMask(GameSystem.UnitConstants.PlayerLayer, GameSystem.CastleConstants.PlayerLayer);
            if (movement.Body != null)
                movement.Body.bodyType = RigidbodyType2D.Kinematic;
        }

        gameObject.layer = OwnLayerMask;
        combat.TargetLayerMask = TargetLayerMask;
    }

    public void BroadcastAttackAnimation() // 공격 애니메이션 재생을 다른 플레이어에 동기화하는 함수
    {
        if (!IsOwnedByLocalPlayer)
            return;
        PhotonView.RPC(nameof(Unit.RPC_SyncAnimation), RpcTarget.Others, (int)UnitStateType.Attack);
    }

    private void BroadcastStateChange(IUnitState nextState) // 상태 전이를 다른 플레이어에 동기화하는 함수
    {
        if (!IsOwnedByLocalPlayer || nextState == null)
            return;
        PhotonView.RPC(nameof(Unit.RPC_SyncAnimation), RpcTarget.Others, (int)nextState.Type);
    }

    public void ScheduleDestruction(float delay) // 일정 딜레이 후 오브젝트 파괴를 예약하는 함수
    {
        if (IsOwnedByLocalPlayer)
            StartCoroutine(DestroyAfterDelay(delay));
    }

    private IEnumerator DestroyAfterDelay(float delay) // 일정 딜레이 후 오브젝트 파괴를 예약하는 코루틴
    {
        yield return new WaitForSeconds(delay);
        PhotonNetwork.Destroy(gameObject);
    }
}