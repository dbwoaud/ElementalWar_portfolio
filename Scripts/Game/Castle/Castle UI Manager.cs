using UnityEngine;
using UnityEngine.UI;
using System;

public class CastleUIManager : BaseUIManager<CastleUIManager>
{
    [Header("UI 요소")]
    [SerializeField] private Button attackButton;
    [SerializeField] private Text attackButtonText;
    [SerializeField] private Image attackButtonImage;

    public event Action OnAttackRequested; // 공격 버튼 클릭 이벤트


    protected override void InitUIElements() // UI 요소 초기화 함수
    {
        SetAttackButtonUI(false, 0f);
    }

    protected override void BindButtonEvent() // 버튼 이벤트 할당 함수
    {
        attackButton?.onClick.AddListener(HandleAttackButtonClicked);
    }

    protected override void BindPanelEvent() // 패널 내부 및 데이터 이벤트 할당 함수
    { 

    }

    protected override void UnbindButtonEvent() // 버튼 이벤트 해제 함수
    {
        attackButton?.onClick.RemoveListener(HandleAttackButtonClicked);
    }

    protected override void UnbindPanelEvent() // 패널 내부 및 데이터 이벤트 해제 함수
    {

    }

    private void HandleAttackButtonClicked() // 공격 버튼 클릭 시 실행되는 함수
    {
        OnAttackRequested?.Invoke();
    }

    private void SetAttackButtonUI(bool isReady, float fillAmount) // 공격 버튼 UI를 설정하는 함수
    {
        attackButton.interactable = isReady;
        attackButtonText.gameObject.SetActive(isReady);
        attackButtonImage.fillAmount = fillAmount;
    }

    public void UpdateAttackButtonUI(float progress) // 공격 버튼 UI를 업데이트하는 함수
    {
        attackButtonImage.fillAmount = progress;
        if (progress >= 1f)
            SetAttackButtonUI(true, 1f);
        else
            SetAttackButtonUI(false, progress);
    }
}