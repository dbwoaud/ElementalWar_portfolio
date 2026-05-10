using Assets.FantasyMonsters.Scripts;
using System.Collections.Generic;
using UnityEngine;


public class FantasyMonsterAdapter : BaseUnitAnimator
{
    [Header("어댑터 컴포넌트")]
    [SerializeField] private Monster monsterScript;

    [Header("캐싱 변수")]
    [SerializeField] private SpriteRenderer[] renderers;
    [SerializeField] private readonly Dictionary<SpriteRenderer, Color> originalColors = new Dictionary<SpriteRenderer, Color>();

    protected override void Awake() 
    { 
        if (monsterScript == null)
            monsterScript = GetComponent<Monster>();
        base.Awake();
    }

    protected override void CacheRenderers() // 렌더러와 원본 색상을 캐싱하는 함수
    {
        renderers = GetComponentsInChildren<SpriteRenderer>(true);
        foreach (var r in renderers)
        {
            if (r != null && !originalColors.ContainsKey(r))
                originalColors.Add(r, r.color);
        }
    }

    public override void PlayIdle() // 유닛을 대기 상태로 만드는 함수
    {
        monsterScript.SetState(MonsterState.Idle);
    }

    public override void PlayMove() // 유닛을 이동 상태로 만드는 함수
    {
        monsterScript.SetState(MonsterState.Walk);
    }

    public override float PlayAttack() // 유닛을 공격 상태로 만들고 애니메이션 길이를 반환하는 함수
    {
        monsterScript.Attack();
        return GetAnimationClipDuration(monsterScript.Animator, "Attack");
    }

    public override void SetDirection(bool lookLeft) // 유닛의 방향을 설정하는 함수
    {
        transform.localRotation = lookLeft ? Quaternion.Euler(0, 180, 0) : Quaternion.identity;
    }

    protected override void OnPlayHitInternal() // 피격 시 시각적 넉백 연출을 호출하는 함수
    {
        monsterScript.Spring();
    }

    protected override void OnPlayDeadInternal() // 죽음 시 캐릭터 고유의 처리를 호출하는 함수
    {
        monsterScript.Die();
    }

    protected override void OnResetForReuseInternal() // 네트워크 풀 재사용 시 캐릭터 상태를 초기화하는 함수
    {
        monsterScript.SetState(MonsterState.Idle);
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
            if (r == null) continue;
            Color c = r.color;
            c.a = alpha;
            r.color = c;
        }
    }
}