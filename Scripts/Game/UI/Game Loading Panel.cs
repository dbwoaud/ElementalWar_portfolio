using UnityEngine;
using UnityEngine.UI;

public class GameLoadingPanel : UIPanel
{
    [Header("UI 요소")]
    [SerializeField] private Text loadingText;
    [SerializeField] private Image progressFill;

    [Header("연출 설정")]
    [SerializeField] private string defaultMessage = "맵을 불러오는 중입니다";
     

    protected override void InitializeListener() { }

    protected override void ResetUI()
    {
        if (progressFill != null)
            progressFill.fillAmount = 0f;
    }

    public void ShowImmediate(string message = null) // 페이드 연출 없이 즉시 패널을 보여주는 함수
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
