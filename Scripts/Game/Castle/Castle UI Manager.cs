using UnityEngine;
using UnityEngine.UI;
using System;

public class CastleUIManager : BaseUIManager<CastleUIManager>
{
    [Header("UI 요소")]
    [SerializeField] private Button attackButton;
    [SerializeField] private Image attackButtonImage;
    [SerializeField] private Text fireText;

    public event Action OnFireRequested;


    protected override void InitUIElements()
    {
        SetUIState(false, 0f);
    }

    protected override void BindButtonEvent()
    {
        attackButton?.onClick.AddListener(HandleAttackButtonClicked);
    }

    protected override void BindPanelEvent() { }

    protected override void UnbindButtonEvent()
    {
        attackButton?.onClick.RemoveListener(HandleAttackButtonClicked);
    }

    private void HandleAttackButtonClicked() // 공격 버튼 클릭 시 실행되는 함수
    {
        OnFireRequested?.Invoke();
    }

    public void UpdateCoolTimeUI(float progress) // 공격 버튼의 UI를 업데이트하는 함수
    {
        attackButtonImage.fillAmount = progress;

        if (progress >= 1f)
            SetUIState(true, 1f);
        else
            SetUIState(false, progress);
    }

    private void SetUIState(bool isReady, float fillAmount) // 공격 버튼의 UI 상태를 설정하는 함수
    {
        attackButton.interactable = isReady;
        fireText.gameObject.SetActive(isReady);
        attackButtonImage.fillAmount = fillAmount;
    }
}
