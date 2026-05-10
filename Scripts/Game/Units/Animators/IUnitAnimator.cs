using UnityEngine;

public interface IUnitAnimator
{
    void PlayIdle();
    void PlayMove();
    float PlayAttack();
    void PlayHit();
    void PlayDead();
    void SetDirection(bool lookLeft);
    void StartFadeOut(float duration);
    void ResetForReuse();
}
