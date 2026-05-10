using UnityEngine;
using UnityEngine.UI;
using System;

public class GameResultPanel : UIPanel
{
    [Header("UI 요소")]
    [SerializeField] private Text resultText;           
    [SerializeField] private Button goToRoomButton;
    [SerializeField] private Button goToLobbyButton;

    public event Action OnReturnToRoomRequested;
    public event Action OnReturnToLobbyRequested;


    protected override void InitializeListener()
    {
        goToRoomButton?.onClick.AddListener(HandleReturnToRoomRequest);
        goToLobbyButton?.onClick.AddListener(HandleReturnToLobbyRequest);
    }

    protected override void UnregisterListener()
    {
        goToRoomButton?.onClick.RemoveListener(HandleReturnToRoomRequest);
        goToLobbyButton?.onClick.RemoveListener(HandleReturnToLobbyRequest);
    }

    private void HandleReturnToRoomRequest() // 방으로 돌아가기 버튼 클릭 시 실행되는 함수
    {
        OnReturnToRoomRequested?.Invoke();
    }

    private void HandleReturnToLobbyRequest() // 로비로 돌아가기 버튼 클릭 시 실행되는 함수
    {
        OnReturnToLobbyRequested?.Invoke();
    }

    protected override void ResetUI()
    {
        if (resultText != null)
            resultText.text = "";
    }

    public void DisplayResult(string playerName, bool isWinner) // 게임의 결과를 출력하는 함수
    {
        if (resultText != null)
            resultText.text = GameSystem.Gameresult.GetGameResultText(playerName, isWinner);
    }
}
