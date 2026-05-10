using System.Collections;
using UnityEngine;

[RequireComponent(typeof(CanvasGroup))]
public abstract class UIPanel : MonoBehaviour
{
    [Header("애니메이션 설정")]
    [SerializeField] private float animationDuration = 0.2f;
    [SerializeField] private Vector3 hiddenScale = new Vector3(0.8f, 0.8f, 0.8f);
    [SerializeField] private Coroutine animationCoroutine;

    [Header("애니메이션 타겟")]
    [SerializeField] protected Transform contentTransform;
    private Transform CotentTransform => contentTransform != null ? contentTransform : transform;

    [Header("캔버스")]
    [SerializeField] protected CanvasGroup canvasGroup;


    protected virtual void Awake()
    {
        canvasGroup = GetComponent<CanvasGroup>();
    }

    protected virtual void Start()
    {
        InitializeListener();
    }

    protected virtual void OnDestroy()
    {
        UnregisterListener();
    }

    protected abstract void InitializeListener(); // UI 리스너를 초기화하는 함수

    protected virtual void UnregisterListener() { } // UI 리스너를 해제하는 함수

    protected abstract void ResetUI(); // UI를 리셋시키는 함수

    public virtual void Show() // 패널을 활성화시키는 함수
    {
        if (!gameObject.activeInHierarchy)
        {
            if (canvasGroup == null)
                canvasGroup = GetComponent<CanvasGroup>();

            if (canvasGroup != null)
                canvasGroup.alpha = 0f;

            CotentTransform.localScale = hiddenScale;
        }

        gameObject.SetActive(true);
        canvasGroup.blocksRaycasts = true;
        RestartAnimationCoroutine(AnimateToVisible());
    }

    private void RestartAnimationCoroutine(IEnumerator routine) // 현재 애니메이션 코루틴을 재시작하는 함수
    {
        StopAnimationCoroutine();
        animationCoroutine = StartCoroutine(routine);
    }

    private IEnumerator AnimateToVisible() // 패널을 활성화시키는 코루틴
    {
        yield return Animate(1f, Vector3.one);
    }

    private IEnumerator Animate(float targetAlpha, Vector3 targetScale) // 애니메이션을 재생하는 코루틴
    {
        float elapsed = 0f;
        float startAlpha = canvasGroup.alpha;
        Vector3 startScale = CotentTransform.localScale;

        while (elapsed < animationDuration)
        {
            elapsed += Time.unscaledDeltaTime;
            float t = EaseOut(Mathf.Clamp01(elapsed / animationDuration));

            canvasGroup.alpha = Mathf.Lerp(startAlpha, targetAlpha, t);
            CotentTransform.localScale = Vector3.Lerp(startScale, targetScale, t);

            yield return null;
        }

        canvasGroup.alpha = targetAlpha;
        CotentTransform.localScale = targetScale;
    }

    public virtual void Hide() // 패널을 비활성화시키는 함수
    {
        if (!gameObject.activeInHierarchy) 
            return;
        
        canvasGroup.blocksRaycasts = false;
        RestartAnimationCoroutine(AnimateToHidden());
    }

    private IEnumerator AnimateToHidden() // 패널을 숨기도록 하는 코루틴
    {
        yield return Animate(0f, hiddenScale);
        gameObject.SetActive(false);
    }

    public virtual void HideImmediate() // 패널을 즉시 비활성화시키는 함수
    {
        if (canvasGroup == null)
            canvasGroup = GetComponent<CanvasGroup>();

        StopAnimationCoroutine();

        canvasGroup.alpha = 0f;
        CotentTransform.localScale = hiddenScale;
        canvasGroup.blocksRaycasts = false;
        gameObject.SetActive(false);
    }

    private void StopAnimationCoroutine() // 애니메이션 코루틴을 중지하는 함수
    {
        if (animationCoroutine != null)
            StopCoroutine(animationCoroutine);
    }

    private static float EaseOut(float t)  // 부드러운 감속 곡선을 나타내는 함수
    {
        return 1f - Mathf.Pow(1f - t, 3f);
    }
}