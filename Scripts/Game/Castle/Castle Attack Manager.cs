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

    protected override void SetUIManager()
    {
        if(CastleUIManager.instance != null)
        {
            castleUIManager = CastleUIManager.instance;
            castleUIManager.OnFireRequested += HandleFireRequest;
        }
    }

    protected override void SetNetworkManager() { }

    protected override void ResetUIManager()
    {
        if (castleUIManager != null)
        {
            castleUIManager.OnFireRequested -= HandleFireRequest;
        }
    }

    protected override void ResetNetworkManager() { }

    protected override void PlayBGM() { }

    protected override void InitializeState() 
    {
        isStop = false;
    }

    private void Update()
    {
        if (isStop)
            return;

        UpdateCoolTimeTimer();
        CheckSpaceBarInput();
    }

    private void UpdateCoolTimeTimer() // 대포 발사 쿨타임을 계산하고 UI를 갱신하는 함수
    {
        if (!isReady && isRegistered)
        {
            currentTimer += Time.deltaTime;
            float progress = Mathf.Clamp01(currentTimer / coolTime);
            castleUIManager?.UpdateCoolTimeUI(progress);
            if (progress >= 1f)
                isReady = true;
        }
    }

    private void CheckSpaceBarInput() // 스페이스 바 입력을 확인하는 함수
    {
        if (InputGate.IsBlocked)
            return;

        if (isReady && Input.GetKeyDown(KeyCode.Space))
            HandleFireRequest();     
    }

    private void HandleFireRequest() // 대포 발사 요청을 처리하는 함수
    {
        if (!isReady || playerCastle == null)
            return;

        ResetFireState();
        playerCastle?.FireCannon();
    }

    private void ResetFireState() // 대포 발사 상태를 초기화하는 함수
    {
        isReady = false;
        currentTimer = 0f;
        castleUIManager?.UpdateCoolTimeUI(0f);
    }

    public void SetPlayerCastle(Castle castle) // 플레이어 성을 설정하는 함수
    {
        playerCastle = castle;
        isRegistered = true;
        isReady = false;
        currentTimer = 0f;
    }

    public void Stop() // 대포 시스템 동작을 중단하는 함수
    {
        isStop = true;
    }
}