using System.Collections;
using Photon.Pun;
using UnityEngine;

public class ProfilingScenarioRunner : MonoBehaviour
{
    [Header("의존 컴포넌트")]
    [SerializeField] private NetworkPerformanceLogger logger;
    [SerializeField] private UnitDatabase unitDatabase;
    [SerializeField] private GameNetworkManager gameNetworkManager;

    [Header("시나리오 설정")]
    [SerializeField] private string scenarioName = "spawn60";
    [SerializeField] private string spawnUnitName = "불닭 굼벵이";
    [SerializeField] private int totalUnits = 60;
    [SerializeField] private float spawnInterval = 0.25f;
    [SerializeField] private float warmupSeconds = 3f;
    [SerializeField] private float durationSeconds = 60f;

    [Header("입력")]
    [SerializeField] private KeyCode startKey = KeyCode.F9;

    private bool isRunning;


    private void Awake()
    {
        if (logger == null)
            logger = GetComponent<NetworkPerformanceLogger>();
    }

    private void OnEnable()
    {
        if (gameNetworkManager != null)
            gameNetworkManager.OnProfilingStartSignal += HandleStartSignal;
    }

    private void OnDisable()
    {
        if (gameNetworkManager != null)
            gameNetworkManager.OnProfilingStartSignal -= HandleStartSignal;
    }

    private void Update()
    {
        if (!isRunning && Input.GetKeyDown(startKey))
            gameNetworkManager?.BroadcastProfilingStart(0);
    }

    private void HandleStartSignal(int scenarioSeed) // 프로파일링 시작 신호를 처리하는 함수
    {
        if (isRunning)
            return;

        StartCoroutine(RunScenario());
    }

    private IEnumerator RunScenario() // 프로파일링 시나리오를 실행하는 코루틴
    {
        isRunning = true;
        Debug.Log($"[Scenario] 워밍업 {warmupSeconds}초");
        yield return new WaitForSecondsRealtime(warmupSeconds);

        logger.StartLogging(scenarioName);

        UnitStat stat = unitDatabase?.FindByName(spawnUnitName);
        if (stat == null)
        {
            Debug.LogError($"[Scenario] 유닛을 찾을 수 없습니다: {spawnUnitName}");
            logger.StopLoggingAndExport();
            isRunning = false;
            yield break;
        }

        Transform spawnPoint = CastleAttackManager.Instance?.PlayerCastle?.UnitSpawnPoint;
        if (spawnPoint == null)
        {
            Debug.LogError("[Scenario] 소환 지점을 찾을 수 없습니다. 맵 생성 완료 후 실행하세요.");
            logger.StopLoggingAndExport();
            isRunning = false;
            yield break;
        }

        float endTime = Time.realtimeSinceStartup + durationSeconds;

        for (int i = 0; i < totalUnits; i++)
        {
            PhotonNetwork.Instantiate(stat.unitPrefab.name, spawnPoint.position, Quaternion.identity, 0);
            yield return new WaitForSecondsRealtime(spawnInterval);
        }

        Debug.Log($"[Scenario] 소환 완료 ({totalUnits}명). 종료까지 대기");

        while (Time.realtimeSinceStartup < endTime)
            yield return null;

        logger.StopLoggingAndExport();
        isRunning = false;
    }
}
