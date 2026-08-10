using Photon.Pun;
using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class Castle : MonoBehaviourPun, IDamagable, IPunInstantiateMagicCallback
{
    [Header("체력 정보")]
    [SerializeField] private float maxHP = 150000f;
    [SerializeField] private float currentHP;
    private string lastHP;
    [SerializeField] private bool isDestroyed = false;

    [Header("UI 요소")]
    [SerializeField] private Transform sprite;
    [SerializeField] private RectTransform canvas;
    [SerializeField] private Text castleHPText;

    [Header("위치 및 방향 설정")]
    [SerializeField] private BoxCollider2D castleCollider;
    [SerializeField] private Vector2 defaultColliderOffset;
    [SerializeField] private float horizontalOffset = 4f;
    [SerializeField] private float verticalOffset = 3f;
    [SerializeField] private Transform unitSpawnPoint;
    public Transform UnitSpawnPoint => unitSpawnPoint;

    [Header("대포 발사 설정")]
    [SerializeField] private GameObject cannonballPrefab;
    [SerializeField] private Transform firePoint;
    [SerializeField] private float directionMultiplier = 1f;
    [SerializeField] private float fireAngle = 30f;

    private bool IsOwnedByLocalPlayer => photonView.IsMine;

    public float MaxHP => maxHP;
    public float CurrentHP => currentHP;
    public bool IsAlive => !isDestroyed;

    public static event Action<bool, Vector3> OnAnyCastleDestroyed; // 성 파괴 시 실행되는 이벤트 


    private void Awake()
    {
        SetColliderPositionOffset();
    }

    private void SetColliderPositionOffset() // 성 콜라이더의 상대 위치를 설정하는 함수 
    {
        if (castleCollider != null)
            defaultColliderOffset = castleCollider.offset;
    }

    public void OnPhotonInstantiate(PhotonMessageInfo info)
    { 
        InitializeCastle();
    }

    private void InitializeCastle() // 성 정보를 초기화하는 함수
    {
        currentHP = maxHP;
        SetHPText();
        bool isRightCastle = !(photonView.Owner?.IsMasterClient ?? photonView.IsMine);
        SetDirection(isRightCastle);
        SetLayer();
        RegisterCastleAttackManager();
    }

    private void SetHPText() // 성 HP 텍스트를 설정하는 함수
    {
        if (castleHPText == null)
            return;

        string newHP = GameSystem.CastleConstants.GetHPText(currentHP, maxHP);
        if (newHP == lastHP)
            return;

        lastHP = newHP;
        castleHPText.text = newHP;
    }

    public void SetDirection(bool isRightCastle) // 성의 방향을 설정하는 함수
    {
        directionMultiplier = isRightCastle ? -1f : 1f;
        sprite.localRotation = isRightCastle ? Quaternion.Euler(0, 180, 0) : Quaternion.identity;
        castleCollider.offset = isRightCastle ? new Vector2(-defaultColliderOffset.x, defaultColliderOffset.y) : defaultColliderOffset;
        canvas.anchoredPosition = new Vector2((isRightCastle ? -1 : 1) * horizontalOffset, verticalOffset);
    }

    private void SetLayer() // 성의 레이어를 설정하는 함수
    {
        int layer = IsOwnedByLocalPlayer
        ? LayerMask.NameToLayer(GameSystem.CastleConstants.PlayerLayer)
        : LayerMask.NameToLayer(GameSystem.CastleConstants.EnemyLayer);
        gameObject.layer = layer;
    }

    private void RegisterCastleAttackManager() // 성 공격 매니저에 성을 등록하는 함수
    {
        if (!IsOwnedByLocalPlayer) 
            return;

        if (CastleAttackManager.Instance != null)
            CastleAttackManager.Instance.SetPlayerCastle(this);
        else
            StartCoroutine(RetryRegistration());
    }

    private IEnumerator RetryRegistration() // 성 공격 매니저에 성 등록을 계속 시도하는 코루틴
    {
        while (CastleAttackManager.Instance == null)
            yield return null;

        CastleAttackManager.Instance.SetPlayerCastle(this);
    }

    public void FireCannon() // 지형의 중앙에 대포를 발사하는 함수
    {
        Collider2D groundCollider = MapManager.Instance?.GroundCollider;
        if (groundCollider == null)
            return;

        float targetDistance = groundCollider.bounds.size.x / 2f;
        float groundSurfaceY = groundCollider.bounds.max.y;
        Vector2 targetPos = new Vector2(firePoint.position.x + targetDistance * directionMultiplier, groundSurfaceY);
        LaunchCannonBall(targetPos);
    }

    public void LaunchCannonBall(Vector2 targetPosition) // 대포를 목표 위치에 발사하는 함수
    {
        if (cannonballPrefab == null || firePoint == null) 
            return;

        Vector2 calculatedForce = TrajectoryCalculator.CalculateLaunchForce(firePoint.position, targetPosition, fireAngle);

        photonView.RPC(nameof(RPC_CreateCannonball), RpcTarget.All, (Vector2)firePoint.position, directionMultiplier, calculatedForce);
    }

    [PunRPC]
    private void RPC_CreateCannonball(Vector2 spawnPosition, float direction, Vector2 force) // 대포를 발사하고 모든 플레이어에게 동기화하는 함수
    { 
        GameObject ball = Instantiate(cannonballPrefab, spawnPosition, Quaternion.identity);
        SoundManager.Instance?.Play(SoundKey.FireCannon);

        Cannonball ballScript = ball.GetComponent<Cannonball>();
        if(IsOwnedByLocalPlayer)
            ballScript?.Init(photonView);

        if (ball.TryGetComponent<Rigidbody2D>(out var rb))
            rb.linearVelocity = new Vector2(force.x * direction, force.y);
    }

    [PunRPC]
    public void RPC_ShowExplosionEffect(Vector2 hitPoint) // 연쇄 폭발 효과를 보여주고 네트워크에 동기화하는 함수
    {
        ExplosionEffectManager.Instance?.PlayChainExplosion(hitPoint);
    }

    [PunRPC]
    public void RPC_TakeDamage(float damage) // 성 피격을 처리하고 네트워크에 동기화하는 함수
    {
        if (isDestroyed || currentHP <= 0) 
            return;

        currentHP = Mathf.Clamp(currentHP - damage, 0f, maxHP);
        SetHPText();
        SoundManager.Instance?.Play(SoundKey.CastleHit);

        if (currentHP <= 0)
            HandleCastleDestruction();   
    }

    private void HandleCastleDestruction() // 성 파괴를 처리하는 함수
    {
        isDestroyed = true;
        if (IsOwnedByLocalPlayer)
            photonView.RPC(nameof(RPC_MyCastleDestroyed), RpcTarget.All);
    }

    [PunRPC]
    private void RPC_MyCastleDestroyed() // 자신의 성 파괴를 네트워크에 동기화하는 함수
    {
        bool localPlayerLost = photonView.IsMine;
        Vector3 castlePosition = transform.position;
        OnAnyCastleDestroyed?.Invoke(localPlayerLost, castlePosition);
    }
}