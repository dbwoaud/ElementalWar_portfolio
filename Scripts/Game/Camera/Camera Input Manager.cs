using System;
using UnityEngine;

public class CameraInputManager : MonoBehaviour
{
    [Header("스크롤 설정 변수")]
    [SerializeField] private float edgeThreshold = 30f;

    [Header("조작 상태 설정 변수")]
    public bool IsInputEnabled { get; set; } = true;

    public event Action<float> OnScrollInput;


    private void Update()
    {
        if (!IsInputEnabled || !Application.isFocused)
            return;

        float mouseX = Input.mousePosition.x;
        float direction = 0f;

        if (CheckOffScreenRight(mouseX))
            direction = 1f;
        else if (CheckOffScreenLeft(mouseX))
            direction = -1f;

        if (direction != 0f)
            OnScrollInput?.Invoke(direction);
    }

    private bool CheckOffScreenRight(float mouseX) // 마우스 좌표가 화면의 오른쪽 경계를 벗어났는지 확인하는 함수
    {
        return mouseX >= Screen.width - edgeThreshold;
    }

    private bool CheckOffScreenLeft(float mouseX) // 마우스 좌표가 화면의 왼쪽 경계를 벗어났는지 확인하는 함수
    {
        return mouseX <= edgeThreshold;
    }
}
