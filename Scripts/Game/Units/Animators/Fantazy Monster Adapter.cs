using Assets.FantasyMonsters.Scripts;
using System.Collections.Generic;
using UnityEngine;

public class FantasyMonsterAdapter : BaseUnitAnimator
{
    [Header("어댑터 컴포넌트")]
    [SerializeField] private Monster monsterScript;

    [Header("캐싱 변수")]
    [SerializeField] private SpriteRenderer[] renderers;
    private readonly Dictionary<SpriteRenderer, Color> originalColors = new();

    protected override void Awake() 
    { 
        if (monsterScript == null)
            monsterScript = GetComponent<Monster>();

        base.Awake();
    }

    public override void SetDirection(bool lookLeft) // 유닛의 방향을 설정하는 함수
    {
        transform.localRotation = lookLeft ? Quaternion.Euler(0, 180, 0) : Quaternion.identity;
    }

    public override void PlayIdle() // 대기 애니메이션을 재생하는 함수
    {
        monsterScript.SetState(MonsterState.Idle);
    }

    public override void PlayMove() // 이동 애니메이션을 재생하는 함수
    {
        monsterScript.SetState(MonsterState.Walk);
    }

    public override float PlayAttack() // 공격 애니메이션을 재생하는 함수
    {
        monsterScript.Attack();
        return GetAnimationClipDuration(monsterScript.Animator, "Attack");
    }

    protected override void CacheRenderers() // 원본 색상과 머터리얼을 저장하는 함수
    {
        renderers = GetComponentsInChildren<SpriteRenderer>(true);
        foreach (var r in renderers)
        {
            if (r != null && !originalColors.ContainsKey(r))
                originalColors.Add(r, r.color);
        }
    }

    protected override void OnPlayKnockback() // 유닛 피격 시 넉백 연출을 재생하는 함수
    {
        monsterScript.Spring();
    }

    protected override void ApplyFlashColor(Color color) // 원본 색상과 머터리얼에 특정 색상을 입히는 함수
    {
        foreach (var r in renderers)
        {
            if (r != null)
                r.color = color;
        }
    }

    protected override void OnPlayDeadInternal() // 유닛 사망 연출을 재생하는 함수
    {
        monsterScript.Die();
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
        monsterScript.SetState(MonsterState.Idle);
    }
}
