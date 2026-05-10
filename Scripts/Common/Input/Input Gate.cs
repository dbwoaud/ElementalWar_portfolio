using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public static class InputGate
{
    private static int cachedFrame = -1;
    private static bool cachedResult;
    public static bool IsBlocked
    {
        get
        {
            int currentFrame = Time.frameCount;
            if (cachedFrame == currentFrame)
                return cachedResult;

            cachedFrame = currentFrame;
            cachedResult = IsTextInputFocused();
            return cachedResult;
        }
    }

    private static bool IsTextInputFocused() // 키보드 포커스가 UI 입력 필드에 있는지 확인하는 함수
    {
        if (EventSystem.current == null)
            return false;

        GameObject selected = EventSystem.current.currentSelectedGameObject;
        if (selected == null)
            return false;

        if (selected.GetComponent<InputField>() != null)
            return true;

        return false;
    }
}