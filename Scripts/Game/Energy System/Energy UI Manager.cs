using UnityEngine;
using UnityEngine.UI;
using System;

public class EnergyUIManager : BaseUIManager<EnergyUIManager>
{
    [Header("UI 연결")]
    [SerializeField] private Text currentEnergyText;
    [SerializeField] private string lastEnergyText;
    [SerializeField] private Text currentLevelText;
    [SerializeField] private string lastLevelText;
    [SerializeField] private Button upgradeButton;
    [SerializeField] private Image upgradeButtonImage;
    [SerializeField] private Text upgradeCostText;
    [SerializeField] private string lastUpgradeCostText;

    public event Action OnUpgradeRequested;


    protected override void InitUIElements() { }

    protected override void BindButtonEvent()
    {
        upgradeButton?.onClick.AddListener(HandleUpgradeButtonClicked);
    }

    protected override void BindPanelEvent() { }

    protected override void UnbindButtonEvent()
    {
        upgradeButton?.onClick.RemoveListener(HandleUpgradeButtonClicked);
    }

    private void HandleUpgradeButtonClicked() // 업그레이드 버튼 클릭 시 실행되는 함수
    {
        OnUpgradeRequested?.Invoke();
    }

    public void UpdateEnergyText(int currentEnergy, int maxEnergy) // 현재 에너지 텍스트를 업데이트하는 함수
    {
        if (currentEnergyText == null)
            return;

        string newText = GameSystem.Energy.GetEnergyText(currentEnergy, maxEnergy);
        if (newText == lastEnergyText)
            return;

        lastEnergyText = newText;
        currentEnergyText.text = newText;
    }

    public void UpdateLevelText(int level, bool isMaxLevel) // 현재 에너지 레벨 텍스트를 업데이트하는 함수
    {
        if (currentLevelText == null)
            return;

        string newText = GameSystem.Energy.GetLevelText(level, isMaxLevel);
        if (newText == lastLevelText)
            return;

        lastLevelText = newText;
        currentLevelText.text = newText;
    }

    public void UpdateUpgradeCostText(int cost, bool isMaxLevel) // 현재 업그레이드 비용 텍스트를 업데이트하는 함수
    {
        if (upgradeCostText == null)
            return;

        string newText = GameSystem.Energy.GetUpgradeCostText(cost, isMaxLevel);
        if (newText == lastUpgradeCostText)
            return;

        lastUpgradeCostText = newText;
        upgradeCostText.text = newText;
    }

    public void SetUpgradeButtonState(bool canUpgrade, bool isMaxLevel) // 업그레이드 버튼 UI를 설정하는 함수
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
