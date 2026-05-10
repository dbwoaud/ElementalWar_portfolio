using System;

public interface IChatView
{
    event Action<string> OnSendMessageRequest;

    void AppendMessage(string formattedMessage);
}