using UnityEngine;
using UnityEngine.UI;
using System;

public class GameResultPanel : UIPanel
{
    [Header("UI 요소")]
    [SerializeField] private Text resultText;           
    [SerializeField] private Button goToRoomButton;
    [SerializeField] private Button goToLobbyButton;

    public event Action OnReturnToRoomRequested; // 방으로 돌아가기 버튼 클릭 이벤트
    public event Action OnReturnToLobbyRequested; // 로비로 돌아가기 버튼 클릭 이벤트


    protected override void RegisterListener() // UI 리스너를 등록하는 함수
    {
        goToRoomButton?.onClick.AddListener(HandleReturnToRoomRequest);
        goToLobbyButton?.onClick.AddListener(HandleReturnToLobbyRequest);
    }

    protected override void UnregisterListener() // UI 리스너를 해제하는 함수
    {
        goToRoomButton?.onClick.RemoveListener(HandleReturnToRoomRequest);
        goToLobbyButton?.onClick.RemoveListener(HandleReturnToLobbyRequest);
    }

    protected override void ResetUI() // UI를 리셋하는 함수
    {
        if (resultText != null)
            resultText.text = "";
    }

    private void HandleReturnToRoomRequest() // 방으로 돌아가기 버튼 클릭을 처리하는 함수
    {
        OnReturnToRoomRequested?.Invoke();
    }

    private void HandleReturnToLobbyRequest() // 로비로 돌아가기 버튼 클릭을 처리하는 함수
    {
        OnReturnToLobbyRequested?.Invoke();
    }

    public void DisplayResult(string playerName, bool isWinner) // 게임의 결과를 출력하는 함수
    {
        if (resultText != null)
            resultText.text = GameSystem.GameResult.GetGameResultText(playerName, isWinner);
    }
}