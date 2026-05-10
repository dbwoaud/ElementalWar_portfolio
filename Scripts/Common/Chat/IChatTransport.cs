using System;

public interface IChatTransport
{
    event Action<string, string> OnMessageReceived;
    event Action<string> OnSystemMessage;

    void Connect();

    void Disconnect();

    void Send(string message);
}