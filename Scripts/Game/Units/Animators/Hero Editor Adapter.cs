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
    [SerializeField] private readonly Dictionary<SpriteRenderer, Color> originalColors = new Dictionary<SpriteRenderer, Color>();
    [SerializeField] private readonly Dictionary<SpriteRenderer, Material> originalMaterials = new Dictionary<SpriteRenderer, Material>();
    private static Material defaultSpriteMaterial;

    [Header("공격 애니메이션 매핑")]
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

    public override void PlayIdle() // 유닛을 대기 상태로 만드는 함수
    {
        characterScript.SetState(CharacterState.Idle);
    }

    public override void PlayMove() // 유닛을 이동 상태로 만드는 함수
    {
        characterScript.SetState(CharacterState.Walk);
    }

    public override float PlayAttack() // 유닛을 공격 상태로 만들고 애니메이션 길이를 반환하는 함수
    {
        characterScript.SetState(CharacterState.Idle);
        ResolveAttackAnimByWeapon();

        characterScript.Animator.SetTrigger(attackTriggerName);
        return GetAnimationClipDuration(characterScript.Animator, attackClipName);
    }

    private void ResolveAttackAnimByWeapon() // 무기 종류에 따라 공격 애니메이션 클립명을 결정하는 함수
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

    public override void SetDirection(bool lookLeft) // 유닛의 방향을 설정하는 함수
    {
        transform.localRotation = lookLeft ? Quaternion.Euler(0, 180, 0) : Quaternion.identity;
    }

    protected override void OnPlayHitInternal() // 피격 시 Hit 트리거와 시각적 넉백 연출을 호출하는 함수
    {
        characterScript.SetState(CharacterState.Idle);
        characterScript.Animator.SetTrigger("Hit");
        characterScript.Spring();
    }

    protected override void OnPlayDeadInternal() // 죽음 시 표정/마스크/상태를 처리하는 함수
    {
        characterScript.SetExpression("Dead");
        ToggleSpriteMasks(false);
        characterScript.SetState(CharacterState.DeathB);
    }

    protected override void OnResetForReuseInternal() // 네트워크 풀 재사용 시 머티리얼/마스크/표정을 초기화하는 함수
    {
        ToggleSpriteMasks(true);
        RestoreOriginalMaterials();
        RefreshRenderers();
        characterScript.SetExpression("Default");
    }

    protected override void ApplyFlashColor(Color color) // FlashRed 효과를 적용하는 함수
    {
        foreach (var r in renderers)
        {
            if (r != null) 
                r.color = color;
        }
    }

    protected override void RestoreOriginalColors() // 캐싱한 원본 색상으로 복구하는 함수
    {
        foreach (var kvp in originalColors)
        {
            if (kvp.Key != null) 
                kvp.Key.color = kvp.Value;
        }
    }

    protected override void ApplyAlpha(float alpha) // 페이드 아웃 알파를 적용하는 함수
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

    protected override IEnumerator FadeOutCoroutine(float duration) // 페이드 아웃 효과를 처리하는 코루틴
    {
        ToggleSpriteMasks(false);
        ApplyDefaultMaterial();

        float elapsed = 0f;
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float alpha = Mathf.Lerp(1f, 0f, elapsed / duration);

            if (characterScript.Expression != "Dead")
                characterScript.SetExpression("Dead");

            ApplyAlpha(alpha);
            yield return null;
        }
        dieCoroutine = null;
    }

    private void ApplyDefaultMaterial() // 페이드 시 기본 머티리얼로 교체하는 함수
    {
        foreach (var r in renderers)
        {
            if (r != null) 
                r.sharedMaterial = defaultSpriteMaterial;
        }
    }

    private void RestoreOriginalMaterials() // 저장한 원본 머티리얼로 복구하는 함수
    {
        foreach (var kvp in originalMaterials)
        {
            if (kvp.Key != null) 
                kvp.Key.sharedMaterial = kvp.Value;
        }
    }

    private void RefreshRenderers() // 자식 객체의 원본 색상과 머터리얼을 다시 저장하는 함수
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

    private void ToggleSpriteMasks(bool isOn) // 자식 SpriteMask 들의 활성화 상태를 일괄 변경하는 함수
    {
        foreach (var mask in GetComponentsInChildren<SpriteMask>(true))
        {
            if (mask != null) 
                mask.enabled = isOn;
        }
    }
}