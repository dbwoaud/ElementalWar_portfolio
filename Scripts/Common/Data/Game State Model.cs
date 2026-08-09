using System;

public class GameStateModel
{
    public bool IsGameOver { get; private set; } = false;
    public bool LocalPlayerWon { get; private set; } = false;

    public event Action<bool> OnGameOver; // 게임 종료 시 실행되는 이벤트


    public void DeclareGameOver(bool localPlayerWon) // 게임 종료를 선언하는 함수
    {
        if (IsGameOver) 
            return;

        IsGameOver = true;
        LocalPlayerWon = localPlayerWon;

        OnGameOver?.Invoke(localPlayerWon);
    }
}