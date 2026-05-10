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

    public event Action<float> OnEnergyChanged;


    protected override void SetUIManager()
    {
        if (EnergyUIManager.instance != null)
        {
            energyUIManager = EnergyUIManager.instance;
            energyUIManager.OnUpgradeRequested += HandleUpgradeRequest;
        }
    }

    protected override void SetNetworkManager() { }

    protected override void ResetUIManager()
    {
        if (energyUIManager != null)
        {
            energyUIManager.OnUpgradeRequested -= HandleUpgradeRequest;
        }
    }

    protected override void ResetNetworkManager() { }

    protected override void PlayBGM() { }

    protected override void InitializeState()
    {
        isStop = false;

        RefreshAllUI();
        CheckUpgradeAvailability(true);
    }

    private void HandleUpgradeRequest() // 업그레이드 버튼 클릭 시 실행되는 함수
    {
        int index = currentLevel - 1;
        if (IsCurrentlyMaxLevel()) 
            return;

        int cost = levelStats[index].upgradeCost;

        if (currentEnergy >= cost)
        {
            currentEnergy -= cost;
            currentLevel++;

            SoundManager.instance?.Play(SoundKey.EnergyConsume);

            RefreshAllUI();
            CheckUpgradeAvailability(true);
            OnEnergyChanged?.Invoke(currentEnergy);
        }
    }

    private bool IsCurrentlyMaxLevel() // 현재 에너지 레벨이 만렙인지 확인하는 함수
    {
        return currentLevel >= levelStats.Length;
    }

    private void RefreshAllUI() // 모든 에너지 UI 텍스트를 업데이트하는 함수
    {
        UpdateEnergyUIOnly();

        int index = Mathf.Min(currentLevel - 1, levelStats.Length - 1);
        bool isMax = IsCurrentlyMaxLevel();

        energyUIManager?.UpdateLevelText(currentLevel, isMax); 
        energyUIManager?.UpdateUpgradeCostText(levelStats[index].upgradeCost, isMax);
    }

    private void UpdateEnergyUIOnly() // 현재 에너지 텍스트를 업데이트하는 함수
    {
        int index = Mathf.Min(currentLevel - 1, levelStats.Length - 1);
        energyUIManager?.UpdateEnergyText((int)currentEnergy, (int)levelStats[index].maxEnergy);
    }

    private void CheckUpgradeAvailability(bool forceUpdate = false) // 에너지 업그레이드가 가능한지 검사하는 함수
    {
        int index = currentLevel - 1;
        if (CheckMaxLevelIndex(index))
            HandleMaxLevelState(forceUpdate);
        
        else
            HandleLevelState(forceUpdate, index);
        
    }

    private bool CheckMaxLevelIndex(int index) // 유효한 인덱스인지 확인하는 함수 
    {
        return index >= levelStats.Length - 1;
    }

    private void HandleMaxLevelState(bool forceUpdate) // 에너지 만렙 상태를 처리하는 함수
    {
        if (wasUpgradeable || forceUpdate)
        {
            energyUIManager?.SetUpgradeButtonState(false, true);
            wasUpgradeable = false;
        }
    }

    private void HandleLevelState(bool forceUpdate, int index) // 만렙이 아닌 에너지 레벨 상태를 처리하는 함수
    {
        bool canUpgrade = currentEnergy >= levelStats[index].upgradeCost;

        if (canUpgrade != wasUpgradeable || forceUpdate)
        {
            wasUpgradeable = canUpgrade;
            energyUIManager?.SetUpgradeButtonState(canUpgrade, false);
        }
    }

    private void Update()
    {
        if (isStop)
            return;

        GenerateEnergy();
        CheckShiftKeyInput();
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
            UpdateEnergyUIOnly();
            OnEnergyChanged?.Invoke(currentEnergy);
            CheckUpgradeAvailability();
        }
    }

    private void CalculateCurrentEnergy(EnergyLevelStat stat) // 현재 에너지를 계산하는 함수
    {
        currentEnergy += stat.energyPerSecond * Time.deltaTime;
        currentEnergy = Mathf.Min(currentEnergy, stat.maxEnergy);
    }

    private void CheckShiftKeyInput() // 시프트 키를 누르면 실행되는 함수
    {
        if (InputGate.IsBlocked)
            return;

        if (Input.GetKeyDown(KeyCode.LeftShift) || Input.GetKeyDown(KeyCode.RightShift))
            HandleUpgradeRequest();
    } 

    public bool TryConsumeEnergy(float amount) // 에너지를 소비할 때 호출되는 함수
    {
        if (currentEnergy >= amount)
        {
            currentEnergy -= amount;
            UpdateEnergyUIOnly();
            CheckUpgradeAvailability(true);
            OnEnergyChanged?.Invoke(currentEnergy);
            return true;
        }
        return false;
    }

    public void AddEnergy(float amount) // 에너지를 추가할 때 호출되는 함수
    {
        int index = Mathf.Min(currentLevel - 1, levelStats.Length - 1);
        float maxEnergy = levelStats[index].maxEnergy;
        currentEnergy = Mathf.Min(currentEnergy + amount, maxEnergy);
        UpdateEnergyUIOnly();
        CheckUpgradeAvailability(true);
        OnEnergyChanged?.Invoke(currentEnergy);
    }

    public void Stop() // 에너지 생성을 멈추는 함수
    {
        isStop = true;
    }
}