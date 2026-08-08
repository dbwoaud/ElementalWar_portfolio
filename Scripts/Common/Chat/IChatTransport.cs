using System;

public interface IChatTransport
{
    event Action<string, string> OnPlayerMessageReceived; // 메시지 수신 이벤트
    event Action<string> OnSystemMessageReceived; // 시스템 메시지 이벤트

    void Connect(); // 채팅 서버 연결 시 실행되는 함수

    void Disconnect(); // 채팅 서버 연결 해제 시 실행되는 함수

    void Send(string message); // 메시지 발신 함수
}