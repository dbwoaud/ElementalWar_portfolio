using UnityEngine;

public abstract class BaseSceneController<T> : Singleton<T> where T : MonoBehaviour
{
    protected virtual void Start()
    {
        SetCachedVariable();
        PlayBGM();
        InitializeState(); 
    }

    protected virtual void SetCachedVariable() // 캐싱 변수를 설정하는 함수
    {
        SubscribeEvents();
    }

    protected virtual void SubscribeEvents()
    {
        SetUIManager();
        SetNetworkManager();
    }

    protected abstract void SetUIManager(); // UI 매니저를 설정하는 함수

    protected abstract void SetNetworkManager(); // 네트워크 매니저를 설정하는 함수

    protected abstract void PlayBGM(); // 씬의 배경음악을 재생하는 함수

    protected abstract void InitializeState(); // 씬의 초기상태를 설정하는 함수

    protected virtual void OnDestroy()
    {
        UnsubscribeAll();
    }

    protected virtual void UnsubscribeAll() // 모든 이벤트를 구독 해제하는 함수
    {
        ResetUIManager();
        ResetNetworkManager();
    }

    protected abstract void ResetUIManager(); // UI 매니저를 리셋하는 함수

    protected abstract void ResetNetworkManager(); // 네트워크 매니저를 리셋하는 함수
}
