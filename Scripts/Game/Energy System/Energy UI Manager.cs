using UnityEngine;
using UnityEngine.UI;
using System;

public class EnergyUIManager : BaseUIManager<EnergyUIManager>
{
    [Header("UI 요소")]
    [SerializeField] private Text currentEnergyText;
    private string lastEnergy;
    [SerializeField] private Text currentLevelText;
    private string lastLevel;
    [SerializeField] private Button upgradeButton;
    [SerializeField] private Image upgradeButtonImage;
    [SerializeField] private Text upgradeCostText;
    private string lastUpgradeCost;

    public event Action OnUpgradeRequested; // 에너지 업그레이드 버튼 클릭 이벤트


    protected override void InitUIElements() // UI 요소 초기화 함수
    {

    }

    protected override void BindButtonEvent() // 버튼 이벤트 할당 함수
    {
        upgradeButton?.onClick.AddListener(HandleUpgradeButtonClicked);
    }

    protected override void BindPanelEvent() // 패널 내부 및 데이터 이벤트 할당 함수
    { 

    }

    protected override void UnbindButtonEvent() // 버튼 이벤트 해제 함수
    {
        upgradeButton?.onClick.RemoveListener(HandleUpgradeButtonClicked);
    }

    protected override void UnbindPanelEvent() // 패널 내부 및 데이터 이벤트 해제 함수
    {

    }

    private void HandleUpgradeButtonClicked() // 업그레이드 버튼 클릭을 처리하는 함수
    {
        OnUpgradeRequested?.Invoke();
    }

    public void SetEnergyText(int currentEnergy, int maxEnergy) // 현재 에너지 텍스트를 설정하는 함수
    {
        if (currentEnergyText == null)
            return;

        string newEnergy = GameSystem.Energy.GetEnergyText(currentEnergy, maxEnergy);
        if (newEnergy == lastEnergy)
            return;

        lastEnergy = newEnergy;
        currentEnergyText.text = newEnergy;
    }

    public void SetLevelText(int level, bool isMaxLevel) // 현재 에너지 레벨 텍스트를 설정하는 함수
    {
        if (currentLevelText == null)
            return;

        string newLevel = GameSystem.Energy.GetLevelText(level, isMaxLevel);
        if (newLevel == lastLevel)
            return;

        lastLevel = newLevel;
        currentLevelText.text = newLevel;
    }

    public void SetUpgradeCostText(int cost, bool isMaxLevel) // 현재 업그레이드 비용 텍스트를 설정하는 함수
    {
        if (upgradeCostText == null)
            return;

        string newUpgradeCost = GameSystem.Energy.GetUpgradeCostText(cost, isMaxLevel);
        if (newUpgradeCost == lastUpgradeCost)
            return;

        lastUpgradeCost = newUpgradeCost;
        upgradeCostText.text = newUpgradeCost;
    }

    public void SetUpgradeButtonUI(bool canUpgrade, bool isMaxLevel) // 에너지 업그레이드 버튼 UI를 설정하는 함수
    {
        if (isMaxLevel)
        {
            upgradeButton.interactable = false;
            upgradeButtonImage.color = Color.white;
        }
        else
        {
            upgradeButton.interactable = canUpgrade;
            upgradeButtonImage.color = canUpgrade ? Color.white : Color.gray;
        }
    }
}
