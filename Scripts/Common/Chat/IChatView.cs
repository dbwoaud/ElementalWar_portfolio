using System;

public interface IChatView
{
    event Action<string> OnSendMessageRequest; // 메시지 발신 요청 이벤트

    void AppendMessage(string formattedMessage); // 메시지를 추가하는 함수
}