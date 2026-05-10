using System;

public class GameStateModel
{
    public bool IsGameOver { get; private set; } = false;
    public bool LocalPlayerWon { get; private set; } = false;
    public event Action<bool> OnGameOver;

    public void DeclareGameOver(bool localPlayerWon) // 게임 종료 시 실행되는 함수
    {
        if (IsGameOver) 
            return;

        IsGameOver = true;
        LocalPlayerWon = localPlayerWon;

        OnGameOver?.Invoke(localPlayerWon);
    }

    public void Reset() // 게임 결과를 리셋하는 함수
    {
        IsGameOver = false;
        LocalPlayerWon = false;
    }
}
