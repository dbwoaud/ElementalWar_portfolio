using UnityEngine;
using UnityEngine.UI;

public abstract class BasePopupPanel : UIPanel
{
    [Header("팝업 패널 공통 UI 요소")]
    [SerializeField] protected Text messageText;


    public virtual void SetMessage(string message) // 메시지 설정 함수
    {
        if(!string.IsNullOrEmpty(message) && messageText != null)
            messageText.text = message;
    }

    protected virtual void ShowPopup(string message) // 팝업 패널 활성화 함수
    {
        SetMessage(message);
        transform.SetAsLastSibling();
        Show();
    }
}
