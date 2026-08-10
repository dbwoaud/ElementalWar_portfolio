using UnityEngine;
using System;


public class EnergyManager : BaseSceneController<EnergyManager>
{
    [Header("캐싱 변수")]
    [SerializeField] private EnergyUIManager energyUIManager;

    [Header("데이터 베이스")]
    [SerializeField] private EnergyLevelStat[] levelStats;

    [Header("에너지 시스템 관련 변수")]
    [SerializeField] private float currentEnergy = 0f;
    public float CurrentEnergy => currentEnergy;
    [SerializeField] private int currentLevel = 1;
    [SerializeField] private bool wasUpgradeable = false;

    [Header("게임 진행 상태")]
    [SerializeField] private bool isStop;

    public event Action<float> OnEnergyChanged; // 현재 에너지 변화 시 실행되는 이벤트


    protected override void SetUIManager() // UI 매니저를 설정하는 함수
    {
        if (EnergyUIManager.Instance != null)
        {
            energyUIManager = EnergyUIManager.Instance;
            energyUIManager.OnUpgradeRequested += HandleUpgradeRequest;
        }
    }

    protected override void SetNetworkManager() // 네트워크 매니저를 설정하는 함수
    {

    }

    protected override void ResetUIManager() // UI 매니저를 리셋하는 함수
    {
        if (energyUIManager != null)
        {
            energyUIManager.OnUpgradeRequested -= HandleUpgradeRequest;
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
        SetAllEnergyText();
        UpdateUpgradeButtonUI(true);
    }

    private void HandleUpgradeRequest() // 에너지 업그레이드 버튼 클릭을 처리하는 함수
    {
        if (IsMaxLevel()) 
            return;

        int index = currentLevel - 1;
        int cost = levelStats[index].upgradeCost;
        if (currentEnergy >= cost)
        {
            currentEnergy -= cost;
            currentLevel++;
            SoundManager.Instance?.Play(SoundKey.EnergyConsume);
            SetAllEnergyText();
            UpdateUpgradeButtonUI(true);
            OnEnergyChanged?.Invoke(currentEnergy);
        }
    }

    private bool IsMaxLevel() // 현재 에너지 레벨이 최고 레벨인지 확인하는 함수
    {
        return currentLevel >= levelStats.Length;
    }

    private void SetAllEnergyText() // 에너지 관련 모든 텍스트를 업데이트하는 함수
    {
        SetEnergyText();

        int index = Mathf.Min(currentLevel - 1, levelStats.Length - 1);
        bool isMax = IsMaxLevel();

        energyUIManager?.SetLevelText(currentLevel, isMax); 
        energyUIManager?.SetUpgradeCostText(levelStats[index].upgradeCost, isMax);
    }

    private void SetEnergyText() // 현재 에너지 텍스트를 업데이트하는 함수
    {
        int index = Mathf.Min(currentLevel - 1, levelStats.Length - 1);
        energyUIManager?.SetEnergyText((int)currentEnergy, (int)levelStats[index].maxEnergy);
    }

    private void UpdateUpgradeButtonUI(bool forceUpdate = false) // 에너지 업그레이드 버튼 UI를 업데이트하는 함수
    {
        int index = currentLevel - 1;
        if (CheckMaxLevelIndex(index))
            HandleMaxLevelState(forceUpdate);
        else
            HandleLevelState(forceUpdate, index);
    }

    private bool CheckMaxLevelIndex(int index) // 유효한 레벨 인덱스인지 확인하는 함수 
    {
        return index >= levelStats.Length - 1;
    }

    private void HandleMaxLevelState(bool forceUpdate) // 에너지 최고 레벨 상태를 처리하는 함수
    {
        if (wasUpgradeable || forceUpdate)
        {
            energyUIManager?.SetUpgradeButtonUI(false, true);
            wasUpgradeable = false;
        }
    }

    private void HandleLevelState(bool forceUpdate, int index) //  최고 레벨을 제외한 에너지 레벨 상태를 처리하는 함수
    {
        bool canUpgrade = currentEnergy >= levelStats[index].upgradeCost;

        if (canUpgrade != wasUpgradeable || forceUpdate)
        {
            wasUpgradeable = canUpgrade;
            energyUIManager?.SetUpgradeButtonUI(canUpgrade, false);
        }
    }

    private void Update()
    {
        if (isStop)
            return;

        GenerateEnergy();
        CheckEnergyUpgradeInput();
    }

    private void GenerateEnergy() // 에너지를 생성하는 함수
    {
        int index = currentLevel - 1;
        if (index >= levelStats.Length)
            return;

        EnergyLevelStat stat = levelStats[index];

        if (currentEnergy < stat.maxEnergy)
        {
            CalculateCurrentEnergy(stat);
            SetEnergyText();
            OnEnergyChanged?.Invoke(currentEnergy);
            UpdateUpgradeButtonUI();
        }
    }

    private void CalculateCurrentEnergy(EnergyLevelStat stat) // 현재 에너지를 계산하는 함수
    {
        currentEnergy += stat.energyGenerationRate * Time.deltaTime;
        currentEnergy = Mathf.Min(currentEnergy, stat.maxEnergy);
    }

    private void CheckEnergyUpgradeInput() // 에너지 업그레이드 키 입력을 확인하는 함수
    {
        if (InputGate.IsBlocked)
            return;

        var keys = InputBindings.EnergyUpgradeKeys;
        for (int i = 0; i < keys.Length; i++)
        {
            if (Input.GetKeyDown(keys[i]))
            {
                HandleUpgradeRequest();
                return;
            }
        }
    } 

    public bool TryConsumeEnergy(float amount) // 에너지 소비를 시도하는 함수
    {
        if (currentEnergy >= amount)
        {
            currentEnergy -= amount;
            SetEnergyText();
            UpdateUpgradeButtonUI(true);
            OnEnergyChanged?.Invoke(currentEnergy);
            return true;
        }
        return false;
    }

    public void AddEnergy(float amount) // 에너지를 획득하는 함수
    {
        if (isStop)
            return;

        int index = Mathf.Min(currentLevel - 1, levelStats.Length - 1);
        float maxEnergy = levelStats[index].maxEnergy;
        currentEnergy = Mathf.Min(currentEnergy + amount, maxEnergy);
        SetEnergyText();
        UpdateUpgradeButtonUI(true);
        OnEnergyChanged?.Invoke(currentEnergy);
    }

    public void StopEnergySystem() // 에너지 시스템 동작을 중지하는 함수
    {
        isStop = true;
    }
}