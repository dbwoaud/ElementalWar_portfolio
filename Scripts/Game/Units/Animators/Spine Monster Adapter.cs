using Spine.Unity;
using UnityEngine;

public class SpineMonsterAdapter : BaseUnitAnimator
{

    [Header("어댑터 컴포넌트")]
    [SerializeField] private SkeletonAnimation skeletonAnim;

    [Header("애니메이션 클립명")]
    [SerializeField] private string idleAnim = "Idle";
    [SerializeField] private string walkAnim = "Walk";
    [SerializeField] private string attackAnim = "Attack";
    [SerializeField] private string hitAnim = "Hit";
    [SerializeField] private string deadAnim = "Dead";


    protected override void Awake()
    {
        if (skeletonAnim == null) 
            skeletonAnim = GetComponent<SkeletonAnimation>();
        base.Awake();
    }

    protected override void CacheRenderers()
    {
        
    }

    public override void PlayIdle() // 유닛을 대기 상태로 만드는 함수
    {
        SetSpineAnim(idleAnim, true);
    }

    public override void PlayMove() // 유닛을 이동 상태로 만드는 함수
    {
        SetSpineAnim(walkAnim, true);
    }

    public override float PlayAttack() // 유닛을 공격 상태로 만들고 애니메이션 길이를 반환하는 함수
    {
        var trackEntry = skeletonAnim.AnimationState.SetAnimation(0, attackAnim, false);
        return trackEntry != null ? trackEntry.Animation.Duration : 0.5f;
    }

    public override void SetDirection(bool lookLeft) // 유닛의 방향을 설정하는 함수
    {
        skeletonAnim.skeleton.ScaleX = lookLeft ? -1f : 1f;
    }

    protected override void OnPlayHitInternal() // 피격 시 Hit 애니메이션을 시각적 넉백 연출로 사용하는 함수
    {
        if (HasAnimation(hitAnim))
            skeletonAnim.AnimationState.SetAnimation(0, hitAnim, false);
    }

    protected override void OnPlayDeadInternal() // 죽음 시 Dead 애니메이션을 재생하는 함수
    {
        SetSpineAnim(deadAnim, loop: false);
    }

    protected override void OnResetForReuseInternal() // 네트워크 풀 재사용 시 Idle 애니메이션으로 복귀하는 함수
    {
        SetSpineAnim(idleAnim, loop: true);
    }

    protected override void ApplyFlashColor(Color color) // FlashRed 효과를 skeleton 단위로 적용하는 함수
    {
        skeletonAnim.skeleton.SetColor(color);
    }

    protected override void RestoreOriginalColors() // skeleton 색상을 흰색원본으로 복구하는 함수
    {
        skeletonAnim.skeleton.SetColor(Color.white);
    }

    protected override void ApplyAlpha(float alpha) // 페이드 아웃 알파를 skeleton 에 적용하는 함수
    {
        skeletonAnim.skeleton.A = alpha;
    }

    private void SetSpineAnim(string animName, bool loop) // Spine 애니메이션을 안전하게 설정하는 함수
    {
        if (skeletonAnim.AnimationName == animName) 
            return;
        skeletonAnim.AnimationState.SetAnimation(0, animName, loop);
    }

    private bool HasAnimation(string animName) // 지정된 이름의 애니메이션이 데이터에 존재하는지 확인하는 함수
    {
        return skeletonAnim != null
            && skeletonAnim.skeleton != null
            && skeletonAnim.skeleton.Data.FindAnimation(animName) != null;
    }
}