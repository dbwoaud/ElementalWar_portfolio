using System;
using UnityEngine;

public abstract class BaseUIManager<T> : Singleton<T> where T : MonoBehaviour
{
    protected virtual void Start()
    {
        InitUIElements();
        BindUIEvents();
    }

    protected abstract void InitUIElements(); // UI 요소 초기화 함수

    protected virtual void BindUIEvents() // UI 이벤트 할당 함수
    {
        BindButtonEvent();
        BindPanelEvent();
    }

    protected abstract void BindButtonEvent(); // 버튼 이벤트 할당 함수

    protected abstract void BindPanelEvent();  // 패널 내부 및 데이터 이벤트 할당 함수

    protected virtual void OnDestroy()
    {
        UnbindUIEvents();
    }

    protected virtual void UnbindUIEvents() // UI 이벤트 해제 함수
    {
        UnbindButtonEvent();
        UnbindPanelEvent();
    }

    protected virtual void UnbindButtonEvent() { } // 버튼 이벤트 해제 함수 

    protected virtual void UnbindPanelEvent() { } // 패널 내부 및 데이터 이벤트 해제 함수

    protected void PlayButtonSound() // 버튼 소리 재생 함수
    {
        SoundManager.instance?.Play(SoundKey.ButtonClick);
    }
}
