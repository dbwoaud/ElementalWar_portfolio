using Spine.Unity;
using UnityEngine;

public class SpineMonsterAdapter : BaseUnitAnimator
{
    [Header("어댑터 컴포넌트")]
    [SerializeField] private SkeletonAnimation skeletonAnim;

    [Header("애니메이션 클립 이름")]
    [SerializeField] private string idleAnimClipName = "Idle";
    [SerializeField] private string walkAnimClipName = "Walk";
    [SerializeField] private string attackAnimClipName = "Attack";
    [SerializeField] private string hitAnimClipName = "Hit";
    [SerializeField] private string deadAnimClipName = "Dead";


    protected override void Awake()
    {
        if (skeletonAnim == null) 
            skeletonAnim = GetComponent<SkeletonAnimation>();

        base.Awake();
    }

    public override void SetDirection(bool lookLeft) // 유닛의 방향을 설정하는 함수
    {
        skeletonAnim.skeleton.ScaleX = lookLeft ? -1f : 1f;
    }

    public override void PlayIdle() // 대기 애니메이션을 재생하는 함수
    {
        SetSpineAnim(idleAnimClipName, true);
    }

    public override void PlayMove() // 이동 애니메이션을 재생하는 함수
    {
        SetSpineAnim(walkAnimClipName, true);
    }

    public override float PlayAttack() // 공격 애니메이션을 재생하는 함수
    {
        var trackEntry = skeletonAnim.AnimationState.SetAnimation(0, attackAnimClipName, false);
        return trackEntry != null ? trackEntry.Animation.Duration : 0.5f;
    }

    protected override void CacheRenderers() // 원본 색상과 머터리얼을 저장하는 함수
    {

    }

    protected override void OnPlayKnockback() // 유닛 피격 시 넉백 연출을 재생하는 함수
    {
        SetSpineAnim(hitAnimClipName, false);
    }

    protected override void ApplyFlashColor(Color color) // 원본 색상과 머터리얼에 특정 색을 입히는 함수
    {
        skeletonAnim.skeleton.SetColor(color);
    }

    protected override void OnPlayDeadInternal() // 유닛 사망 연출을 재생하는 함수
    {
        SetSpineAnim(deadAnimClipName, false);
    }

    protected override void ApplyAlpha(float alpha) // 모든 색상과 머터리얼에 알파 값을 적용하는 함수
    {
        skeletonAnim.skeleton.A = alpha;
    }

    protected override void RestoreOriginalColors() // 원본 색상과 머터리얼을 원래대로 복구하는 함수
    {
        skeletonAnim.skeleton.SetColor(Color.white);
    }

    protected override void OnResetForReuseInternal() // 재사용을 위해 유닛 상태를 초기화하는 함수
    {
        if (skeletonAnim != null && skeletonAnim.skeleton != null)
        {
            skeletonAnim.AnimationState.ClearTracks();
            skeletonAnim.skeleton.SetToSetupPose();
        }
        SetSpineAnim(idleAnimClipName, true);
    }

    private void SetSpineAnim(string animName, bool loop) // Spine 애니메이션을 설정하는 함수
    {
        if (!HasAnimation(animName))
            return;

        if (skeletonAnim.AnimationName == animName) 
            return;

        skeletonAnim.AnimationState.SetAnimation(0, animName, loop);
    }

    private bool HasAnimation(string animName) // 애니메이션이 존재하는지 확인하는 함수
    {
        return skeletonAnim != null && skeletonAnim.skeleton != null && skeletonAnim.skeleton.Data.FindAnimation(animName) != null;
    }
}