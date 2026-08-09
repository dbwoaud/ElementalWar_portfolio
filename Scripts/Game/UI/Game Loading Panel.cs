using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(CanvasGroup))]
public class GameLoadingPanel : UIPanel
{
    [Header("UI 요소")]
    [SerializeField] private Text loadingText;
    [SerializeField] private Image progressFill;

    [Header("연출 설정")]
    [SerializeField] private string defaultMessage = "맵을 불러오는 중입니다";
     

    protected override void RegisterListener() // UI 리스너를 등록하는 함수
    { 

    }

    protected override void ResetUI() // UI를 리셋하는 함수
    {
        if (progressFill != null)
            progressFill.fillAmount = 0f;
    }

    public void ShowImmediate(string message = null) // 즉시 패널을 활성화하는 함수
    {
        if (canvasGroup == null)
            canvasGroup = GetComponent<CanvasGroup>();

        gameObject.SetActive(true);

        canvasGroup.alpha = 1f;
        canvasGroup.blocksRaycasts = true;

        if (loadingText != null)
            loadingText.text = string.IsNullOrEmpty(message) ? defaultMessage : message;

        ResetUI();
    }

    public void UpdateProgress(float normalized) // 진행도를 갱신하는 함수
    {   
        if (progressFill != null)
            progressFill.fillAmount = Mathf.Clamp01(normalized);
    }
}
