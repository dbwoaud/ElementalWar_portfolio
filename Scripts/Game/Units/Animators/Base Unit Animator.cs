using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class BaseUnitAnimator : MonoBehaviour, IUnitAnimator
{
    [Header("피격 연출 설정")]
    [SerializeField] protected Color flashColor = Color.red;
    [SerializeField] protected float flashDuration = 0.1f;

    protected Coroutine hitCoroutine;
    protected Coroutine dieCoroutine;

    [Header("캐싱 변수")]
    [SerializeField] private readonly Dictionary<string, float> animationDurationCache = new Dictionary<string, float>(8);

    protected virtual void Awake()
    {
        CacheRenderers();
    }

    protected abstract void CacheRenderers(); // 원본 색상과 머터리얼을 저장하는 함수

    protected abstract void ApplyFlashColor(Color color); // 원본 색상과 머터리얼에 특정 색을 입히는 함수

    protected abstract void RestoreOriginalColors(); // 원본 색상과 머터리얼을 원래대로 복구하는 함수

    protected abstract void ApplyAlpha(float alpha); // 모든 색상과 머터리얼에 알파 값을 적용하는 함수

    public abstract void PlayIdle();

    public abstract void PlayMove();

    public abstract float PlayAttack();

    public abstract void SetDirection(bool lookLeft);

    protected virtual void OnPlayHitInternal() { } // 넉백 연출을 처리하는 함수

    protected virtual void OnPlayDeadInternal() { } // 유닛의 죽음을 처리하는 함수

    protected virtual void OnResetForReuseInternal() { } // 유닛의 재사용을 위해 초기화하는 함수

    public virtual void PlayHit() // 유닛을 피격 상태로 만드는 함수
    {
        StopHitCoroutine();
        OnPlayHitInternal();
        hitCoroutine = StartCoroutine(FlashRedCoroutine());
    }

    protected void StopHitCoroutine() // Hit 코루틴을 안전하게 종료하는 함수
    {
        if (hitCoroutine == null) return;

        StopCoroutine(hitCoroutine);
        hitCoroutine = null;
        RestoreOriginalColors();
    }

    private IEnumerator FlashRedCoroutine() // FlashRed 효과를 처리하는 코루틴
    {
        ApplyFlashColor(flashColor);
        yield return new WaitForSeconds(flashDuration);
        RestoreOriginalColors();
        hitCoroutine = null;
    }

    public virtual void PlayDead() // 유닛을 죽음 상태로 만드는 함수
    {
        StopHitCoroutine();
        RestoreOriginalColors();
        OnPlayDeadInternal();
    }

    public virtual void StartFadeOut(float duration) // 페이드 아웃 코루틴을 시작하는 함수
    {
        StopDieCoroutine();
        dieCoroutine = StartCoroutine(FadeOutCoroutine(duration));
    }

    protected virtual IEnumerator FadeOutCoroutine(float duration) // 페이드 아웃 효과를 처리하는 코루틴
    {
        float elapsed = 0f;
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float alpha = Mathf.Lerp(1f, 0f, elapsed / duration);
            ApplyAlpha(alpha);
            yield return null;
        }
        dieCoroutine = null;
    }

    public virtual void ResetForReuse() // 네트워크 오브젝트 풀에서 재사용 시 모든 시각 상태를 초기화하는 함수
    {
        StopAllAnimatorCoroutines();
        ApplyAlpha(1f);
        RestoreOriginalColors();
        OnResetForReuseInternal();
        PlayIdle();
    }

    protected void StopDieCoroutine() // 죽음 코루틴을 안전하게 종료하는 함수
    {
        if (dieCoroutine == null) 
            return;

        StopCoroutine(dieCoroutine);
        dieCoroutine = null;
    }

    protected void StopAllAnimatorCoroutines() // 모든 애니메이터 코루틴을 종료하는 함수
    {
        StopHitCoroutine();
        StopDieCoroutine();
    }

    protected float GetAnimationClipDuration(Animator animator, string clipNameKeyword, float fallback = 0.5f) // 애니메이션 클립의 길이를 반환하는 함수
    {
        if (animator == null || animator.runtimeAnimatorController == null) 
            return fallback;

        if (animationDurationCache.TryGetValue(clipNameKeyword, out float cached))
            return cached;

        float result = fallback;
        foreach (AnimationClip clip in animator.runtimeAnimatorController.animationClips)
        {
            if (clip != null && clip.name.Contains(clipNameKeyword))
            {
                result = clip.length;
                break;
            }
                
        }
        animationDurationCache[clipNameKeyword] = result;
        return result;
    }
}
