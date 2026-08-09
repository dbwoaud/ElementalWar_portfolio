using Assets.HeroEditor.Common.CharacterScripts;
using HeroEditor.Common.Enums;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HeroEditorAdapter : BaseUnitAnimator
{
    [Header("어댑터 컴포넌트")]
    [SerializeField] private Character characterScript;

    [Header("캐싱 변수")]
    [SerializeField] private SpriteRenderer[] renderers;
    private readonly Dictionary<SpriteRenderer, Color> originalColors = new Dictionary<SpriteRenderer, Color>();
    private readonly Dictionary<SpriteRenderer, Material> originalMaterials = new Dictionary<SpriteRenderer, Material>();
    private static Material defaultSpriteMaterial;

    [Header("공격 애니메이션 변수")]
    [SerializeField] private string attackTriggerName;
    [SerializeField] private string attackClipName;


    protected override void Awake()
    {
        if (characterScript == null) 
            characterScript = GetComponent<Character>();

        if (defaultSpriteMaterial == null) 
            defaultSpriteMaterial = new Material(Shader.Find("Sprites/Default"));

        base.Awake();
    }

    public override void SetDirection(bool lookLeft) // 유닛의 방향을 설정하는 함수
    {
        transform.localRotation = lookLeft ? Quaternion.Euler(0, 180, 0) : Quaternion.identity;
    }

    public override void PlayIdle() // 대기 애니메이션을 재생하는 함수
    {
        characterScript.SetState(CharacterState.Idle);
    }

    public override void PlayMove() // 이동 애니메이션을 재생하는 함수
    {
        characterScript.SetState(CharacterState.Walk);
    }

    public override float PlayAttack() // 공격 애니메이션을 재생하는 함수
    {
        characterScript.SetState(CharacterState.Idle);
        SetAttackAnimParameter();

        characterScript.Animator.SetTrigger(attackTriggerName);
        return GetAnimationClipDuration(characterScript.Animator, attackClipName);
    }

    private void SetAttackAnimParameter() // 공격 애니메이션 파라미터를 설정하는 함수
    {
        switch (characterScript.WeaponType)
        {
            case WeaponType.Melee1H:
            case WeaponType.Melee2H:
            case WeaponType.MeleePaired:
                attackTriggerName = "Slash";
                attackClipName = "Slash";
                break;
            default:
                attackTriggerName = "Slash";
                attackClipName = "Slash";
                break;
        }
    }

    protected override IEnumerator FadeOutCoroutine(float duration) // 페이드 아웃 효과를 시작하는 코루틴
    {
        ToggleSpriteMasks(false);
        ApplyDefaultMaterial();

        float elapsed = 0f;
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float alpha = Mathf.Lerp(1f, 0f, elapsed / duration);
            ApplyAlpha(alpha);

            if (characterScript.Expression != "Dead")
                characterScript.SetExpression("Dead");

            yield return null;
        }

        dieCoroutine = null;
    }

    protected override void CacheRenderers() // 원본 색상과 머티리얼을 저장하는 함수
    {
        renderers = GetComponentsInChildren<SpriteRenderer>(true);
        foreach (var r in renderers)
        {
            if (r == null)
                continue;

            if (!originalColors.ContainsKey(r))
                originalColors.Add(r, r.color);
            if (!originalMaterials.ContainsKey(r))
                originalMaterials.Add(r, r.sharedMaterial);
        }
    }

    protected override void OnPlayKnockback() // 유닛 피격 시 넉백 연출을 재생하는 함수
    {
        characterScript.SetState(CharacterState.Idle);
        characterScript.Animator.SetTrigger("Hit");
        characterScript.Spring();
    }

    protected override void ApplyFlashColor(Color color) // 원본 색상과 머터리얼에 특정 색을 입히는 함수
    {
        foreach (var r in renderers)
        {
            if (r != null)
                r.color = color;
        }
    }

    protected override void OnPlayDeadInternal() // 유닛 사망 연출을 재생하는 함수
    {
        characterScript.SetExpression("Dead");
        ToggleSpriteMasks(false);
        characterScript.SetState(CharacterState.DeathB);
    }

    protected override void ApplyAlpha(float alpha) // 모든 색상과 머터리얼에 알파 값을 적용하는 함수
    {
        foreach (var r in renderers)
        {
            if (r == null)
                continue;

            Color c = r.color;
            c.a = alpha;
            r.color = c;
        }
    }

    protected override void RestoreOriginalColors() // 원본 색상과 머터리얼을 원래대로 복구하는 함수
    {
        foreach (var kvp in originalColors)
        {
            if (kvp.Key != null)
                kvp.Key.color = kvp.Value;
        }
    }

    protected override void OnResetForReuseInternal() // 재사용을 위해 유닛 상태를 초기화하는 함수
    {
        ToggleSpriteMasks(true);
        RestoreOriginalMaterials();
        CacheRenderers();
        characterScript.SetExpression("Default");
    }

    private void ToggleSpriteMasks(bool isOn) // 자식 SpriteMask들을 활성화/비활성화하는 함수
    {
        foreach (var mask in GetComponentsInChildren<SpriteMask>(true))
        {
            if (mask != null)
                mask.enabled = isOn;
        }
    }

    private void ApplyDefaultMaterial() // 기본 머터리얼을 적용하는 함수
    {
        foreach (var r in renderers)
        {
            if (r != null) 
                r.sharedMaterial = defaultSpriteMaterial;
        }
    }

    private void RestoreOriginalMaterials() // 머티리얼을 원래대로 복구하는 함수
    {
        foreach (var kvp in originalMaterials)
        {
            if (kvp.Key != null) 
                kvp.Key.sharedMaterial = kvp.Value;
        }
    }
}