using UnityEngine;

public class CastleAttackManager : BaseSceneController<CastleAttackManager>
{
    [Header("캐싱 변수")]
    [SerializeField] private CastleUIManager castleUIManager;
    [SerializeField] private Castle playerCastle;
    public Castle PlayerCastle => playerCastle;

    [Header("공격 설정")]
    [SerializeField] private float coolTime = 50f;
    [SerializeField] private float currentTimer = 0f;
    [SerializeField] private bool isReady = false;
    [SerializeField] private bool isRegistered = false;

    [Header("게임 진행 상태")]
    [SerializeField] private bool isStop;

    protected override void SetUIManager() // UI 매니저를 설정하는 함수
    {
        if(CastleUIManager.Instance != null)
        {
            castleUIManager = CastleUIManager.Instance;
            castleUIManager.OnAttackRequested += HandleCannonFireRequest;
        }
    }

    protected override void SetNetworkManager() // 네트워크 매니저를 설정하는 함수
    { 

    }

    protected override void ResetUIManager() // UI 매니저를 리셋하는 함수
    {
        if (castleUIManager != null)
        {
            castleUIManager.OnAttackRequested -= HandleCannonFireRequest;
        }
    }

    protected override void ResetNetworkManager() // 네트워크 매니저를 리셋하는 함수
    {

    }

    protected override void PlayBGM() // 씬의 배경음악을 재생하는 함수
    { 

    }

    protected override void InitializeState() // 씬의 초기상태를 설정하는 함수
    {
        isStop = false;
    }

    private void Update()
    {
        if (isStop)
            return;

        UpdateCoolDownTimer();
        CheckCannonFireInput();
    }

    private void UpdateCoolDownTimer() // 대포 발사 쿨타임을 계산하고 UI를 업데이트하는 함수
    {
        if (!isReady && isRegistered)
        {
            currentTimer += Time.deltaTime;
            float progress = Mathf.Clamp01(currentTimer / coolTime);
            castleUIManager?.UpdateAttackButtonUI(progress);
            if (progress >= 1f)
                isReady = true;
        }
    }

    private void CheckCannonFireInput() // 대포 발사 키 입력을 확인하는 함수
    {
        if (InputGate.IsBlocked)
            return;

        if (isReady && Input.GetKeyDown(InputBindings.CannonFireKey))
            HandleCannonFireRequest();     
    }

    private void HandleCannonFireRequest() // 대포 발사 요청을 처리하는 함수
    {
        if (!isReady || playerCastle == null)
            return;

        ResetFireState();
        playerCastle?.FireCannon();
    }

    private void ResetFireState() // 대포 상태를 초기화하는 함수
    {
        isReady = false;
        currentTimer = 0f;
        castleUIManager?.UpdateAttackButtonUI(0f);
    }

    public void SetPlayerCastle(Castle castle) // 플레이어 성을 설정하는 함수
    {
        playerCastle = castle;
        isRegistered = true;
        isReady = false;
        currentTimer = 0f;
    }

    public void StopAttackSystem() // 대포 공격 시스템 동작을 중지하는 함수
    {
        isStop = true;
    }
}