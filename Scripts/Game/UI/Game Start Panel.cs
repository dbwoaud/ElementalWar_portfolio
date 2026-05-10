using UnityEngine;
using UnityEngine.UI;

public class GameStartPanel : UIPanel
{
    [Header("UI 요소")]
    [SerializeField] private Text player1NameText;
    [SerializeField] private Text player2NameText;


    protected override void InitializeListener() { }

    protected override void ResetUI() { }

    public void SetPlayerNames(string p1Name, string p2Name) // 플레이어 이름을 설정하는 함수
    {
        player1NameText.text = p1Name;
        player2NameText.text = p2Name;
    }
}