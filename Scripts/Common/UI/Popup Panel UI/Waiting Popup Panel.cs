using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class WaitingPopupPanel : BasePopupPanel
{
    [Header("UI 요소")]
    [SerializeField] private Button cancelButton;
    [SerializeField] private string message;

    private Action onCancelAction; // 취소 버튼 클릭 이벤트

    private Coroutine textAnimationCoroutine;


    protected override void RegisterListener() // UI 리스너를 등록하는 함수
    {
        cancelButton?.onClick.AddListener(OnClickCancelButton);
    }

    protected override void UnregisterListener() // UI 리스너를 해제하는 함수
    {
        cancelButton?.onClick.RemoveListener(OnClickCancelButton);
    }

    protected override void ResetUI() // UI를 리셋하는 함수
    {
        messageText.text = "";
    }

    private void OnEnable()
    {
        if (textAnimationCoroutine != null)
            StopCoroutine(textAnimationCoroutine);

        if (!string.IsNullOrEmpty(message))
            textAnimationCoroutine = StartCoroutine(AnimateTextCoroutine());
    }

    private void OnDisable()
    {
        if (textAnimationCoroutine != null)
        {
            StopCoroutine(textAnimationCoroutine);
            textAnimationCoroutine = null;
        }
    }

    public override void SetMessage(string message) // 로딩 메시지를 설정하는 함수
    {
        base.SetMessage(message);
        this.message = message;
    }

    private void OnClickCancelButton() // 취소 버튼 클릭 시 실행되는 함수
    {
        SoundManager.Instance?.Play(SoundKey.ButtonClick);
        onCancelAction?.Invoke();
        Hide();
    }

    public void Setup(string message, Action onCancel = null) // 로딩 팝업 패널을 설정하는 함수
    {
        onCancelAction = onCancel;
        ShowPopup(message);
    }

    public void Close() // 로딩 완료 시 패널 닫기를 실행하는 함수
    {
        HideImmediate();
    }

    private IEnumerator AnimateTextCoroutine() // 텍스트 애니메이션을 연출하는 코루틴
    {
        int dotCount = 0;
        WaitForSeconds waitTime = new WaitForSeconds(0.4f);
        while (true)
        {
            string dots = new string('.', dotCount);
            if (messageText != null)
                messageText.text = message + dots;
            
            dotCount = (dotCount + 1) % 4;
            yield return waitTime;
        }
    }
}
