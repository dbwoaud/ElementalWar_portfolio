using System;
using UnityEngine;

public class DeckHotkeyHandler : MonoBehaviour
{
    [Header("입력 활성화 여부")]
    [SerializeField] private bool isEnabled = true;
    public bool IsEnabled
    {
        get => isEnabled;
        set => isEnabled = value;
    }

    public event Action<int> OnSlotHotkeyPressed;


    private void Update()
    {
        if (!isEnabled)
            return;

        DetectHotkey();
    }

    private void DetectHotkey() // 단축키 입력을 감지해 이벤트를 발행하는 함수
    {
        if (InputGate.IsBlocked)
            return;

        var keys = InputBindings.UnitNumberKeys;
        for (int i = 0; i < keys.Length; i++)
        {
            if (Input.GetKeyDown(keys[i]))
            {
                OnSlotHotkeyPressed?.Invoke(i);
                break;
            }
        }
    }
}