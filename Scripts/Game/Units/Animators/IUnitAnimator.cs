public interface IUnitAnimator
{
    void ResetForReuse(); // 재사용을 위해 유닛 설정을 초기화하는 함수
    void SetDirection(bool lookLeft); // 유닛의 방향을 설정하는 함수
    void PlayIdle(); // 대기 애니메이션을 재생하는 함수
    void PlayMove(); // 이동 애니메이션을 재생하는 함수
    float PlayAttack(); // 공격 애니메이션을 재생하는 함수
    void PlayHit(); // 피격 애니메이션을 재생하는 함수
    void PlayDead(); // 사망 애니메이션을 재생하는 함수
    void StartFadeOut(float duration); // 페이드 아웃 효과를 시작하는 함수
}