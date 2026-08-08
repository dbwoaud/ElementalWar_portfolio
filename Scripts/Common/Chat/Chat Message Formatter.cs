public static class ChatMessageFormatter
{
    public static string GetFormattedPlayerMessage(string sender, string message, bool isMine) // 색상 태그를 포함한 플레이어 메시지를 반환하는 함수
    {
        var colorEnum = isMine
            ? ChattingSystem.Color.ChatColor.Blue
            : ChattingSystem.Color.ChatColor.Black;
        string colorTag = ChattingSystem.Color.GetColor(colorEnum);
        return $"<color={colorTag}>{sender} : {message}</color>";
    }

    public static string GetFormattedSystemMessage(string message) // 색상 태그를 포함한 시스템 메시지를 반환하는 함수
    {
        string colorTag = ChattingSystem.Color.GetColor(ChattingSystem.Color.ChatColor.Purple);
        return $"<color={colorTag}>[System] : {message}</color>";
    }
}
