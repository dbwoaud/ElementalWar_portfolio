using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class BaseUnitAnimator : MonoBehaviour, IUnitAnimator
{
    [Header("피격 연출 설정")]
    [SerializeField] protected Color flashColor = Color.red;
    [SerializeField] protected float flashDuration = 0.1f;
    private readonly Dictionary<string, float> animationLengthCache = new(8);

    protected Coroutine hitCoroutine;
    protected Coroutine dieCoroutine;


    protected virtual void Awake()
    {
        CacheRenderers();
    }

    public virtual void ResetForReuse() // 재사용을 위해 유닛 설정을 초기화하는 함수
    {
        StopAllAnimationCoroutines();
        ApplyAlpha(1f);
        RestoreOriginalColors();
        OnResetForReuseInternal();
        PlayIdle();
    }

    public abstract void SetDirection(bool lookLeft); // 유닛의 방향을 설정하는 함수

    public abstract void PlayIdle(); // 대기 애니메이션을 재생하는 함수

    public abstract void PlayMove(); // 이동 애니메이션을 재생하는 함수

    public abstract float PlayAttack(); // 공격 애니메이션을 재생하는 함수

    public virtual void PlayHit() // 피격 애니메이션을 재생하는 함수
    {
        StopHitCoroutine();
        OnPlayKnockback();
        hitCoroutine = StartCoroutine(FlashRedCoroutine());
    }

    public virtual void PlayDead() // 사망 애니메이션을 재생하는 함수
    {
        StopHitCoroutine();
        RestoreOriginalColors();
        OnPlayDeadInternal();
    }

    public virtual void StartFadeOut(float duration) // 페이드 아웃 효과를 시작하는 함수
    {
        StopDieCoroutine();
        dieCoroutine = StartCoroutine(FadeOutCoroutine(duration));
    }

    protected virtual IEnumerator FadeOutCoroutine(float duration) // 페이드 아웃 효과를 시작하는 코루틴
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

    protected abstract void CacheRenderers(); // 원본 색상과 머터리얼을 저장하는 함수

    protected void StopAllAnimationCoroutines() // 모든 애니메이션 코루틴을 종료하는 함수
    {
        StopHitCoroutine();
        StopDieCoroutine();
    }

    protected virtual void OnPlayKnockback() { } // 유닛 피격 시 넉백 연출을 재생하는 함수

    private IEnumerator FlashRedCoroutine() // 유닛 피격 연출를 재생하는 코루틴
    {
        ApplyFlashColor(flashColor);
        yield return new WaitForSeconds(flashDuration);
        RestoreOriginalColors();
        hitCoroutine = null;
    }

    protected abstract void ApplyFlashColor(Color color); // 원본 색상과 머터리얼에 특정 색을 입히는 함수

    protected void StopHitCoroutine() // 피격 연출 코루틴을 중지하는 함수
    {
        if (hitCoroutine == null) 
            return;

        StopCoroutine(hitCoroutine);
        hitCoroutine = null;
        RestoreOriginalColors();
    }

    protected virtual void OnPlayDeadInternal() { } // 유닛 사망 연출을 재생하는 함수

    protected void StopDieCoroutine() // 사망 연출 코루틴을 중지하는 함수
    {
        if (dieCoroutine == null)
            return;

        StopCoroutine(dieCoroutine);
        dieCoroutine = null;
    }

    protected abstract void ApplyAlpha(float alpha); // 모든 색상과 머터리얼에 알파 값을 적용하는 함수

    protected abstract void RestoreOriginalColors(); // 원본 색상과 머터리얼을 원래대로 복구하는 함수

    protected virtual void OnResetForReuseInternal() { } // 재사용을 위해 유닛 상태를 초기화하는 함수

    protected float GetAnimationClipDuration(Animator animator, string clipName, float fallback = 0.5f) // 애니메이션 클립의 길이를 반환하는 함수
    {
        if (animator == null || animator.runtimeAnimatorController == null)
            return fallback;

        /* 캐시에 있으면, 캐시에서 불러오기 */
        if (animationLengthCache.TryGetValue(clipName, out float length))
            return length;

        float result = fallback;

        /* 캐시에 없으면, 직접 찾기 */
        foreach (AnimationClip clip in animator.runtimeAnimatorController.animationClips)
        {
            if (clip != null && clip.name.Contains(clipName))
            {
                result = clip.length;
                break;
            }
        }
        animationLengthCache[clipName] = result;
        return result;
    }
}