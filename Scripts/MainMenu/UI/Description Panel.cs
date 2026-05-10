using UnityEngine;
using UnityEngine.UI;

public class DescriptionPanel : UIPanel
{
    [Header("UI 요소")]
    [SerializeField] private Button closeButton;


    protected override void InitializeListener()
    {
        closeButton?.onClick.AddListener(OnClickCloseButton);
    }

    protected override void UnregisterListener()
    {
        closeButton?.onClick.RemoveListener(OnClickCloseButton);
    }

    private void OnClickCloseButton() // 닫기 버튼 클릭 시 실행되는 함수
    {
        SoundManager.instance?.Play(SoundKey.ButtonClick);
        Hide();
    }

    protected override void ResetUI()
    {

    }
}
