public static class ChatMessageFormatter
{
    public static string FormatPlayerMessage(string sender, string message, bool isMine) // 플레이어 메시지를 색상 태그로 포맷하는 함수
    {
        var colorEnum = isMine
            ? ChattingSystem.Color.ChatColor.Blue
            : ChattingSystem.Color.ChatColor.Black;
        string colorTag = ChattingSystem.Color.GetColor(colorEnum);
        return $"<color={colorTag}>{sender} : {message}</color>";
    }

    public static string FormatSystemMessage(string message) // 시스템 메시지를 색상 태그로 포맷하는 함수
    {
        string colorTag = ChattingSystem.Color.GetColor(ChattingSystem.Color.ChatColor.Purple);
        return $"<color={colorTag}>[System] : {message}</color>";
    }
}
