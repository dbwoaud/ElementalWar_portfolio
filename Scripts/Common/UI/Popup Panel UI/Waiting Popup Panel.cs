using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;


public class WaitingPopupPanel : BasePopupPanel
{
    [Header("UI 요소")]
    [SerializeField] private Button cancelButton;
    [SerializeField] private string message;

    [Header("버튼 클릭 이벤트")]
    private Action onCancelAction;

    [Header("로딩 연출 코루틴")]
    private Coroutine textAnimationCoroutine;

    protected override void InitializeListener()
    {
        cancelButton?.onClick.AddListener(OnClickCancelButton);
    }

    protected override void UnregisterListener()
    {
        cancelButton?.onClick.RemoveListener(OnClickCancelButton);
    }

    protected override void ResetUI()
    {
        messageText.text = "";
    }

    private void OnEnable()
    {
        if (textAnimationCoroutine != null)
        {
            StopCoroutine(textAnimationCoroutine);
        }

        if (!string.IsNullOrEmpty(message))
        {
            textAnimationCoroutine = StartCoroutine(AnimateTextCoroutine());
        }
    }

    private void OnDisable()
    {
        if (textAnimationCoroutine != null)
        {
            StopCoroutine(textAnimationCoroutine);
            textAnimationCoroutine = null;
        }
    }

    public override void SetMessage(string message) // 메시지 설정 함수
    {
        this.message = message;
        if (messageText != null)
            messageText.text = this.message;  
    }

    private void OnClickCancelButton() // 취소 버튼 클릭 시 실행되는 함수
    {
        SoundManager.instance?.Play(SoundKey.ButtonClick);
        onCancelAction?.Invoke();
        Hide();
    }

    public void Setup(string message, Action onCancel = null) // 로딩 팝업 패널을 활성화하는 함수
    {
        onCancelAction = onCancel;
        ShowPopup(message);
    }

    public void Close() // 로딩 완료 시 강제로 실행하는 함수
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
