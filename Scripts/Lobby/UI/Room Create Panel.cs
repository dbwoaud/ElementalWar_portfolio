using UnityEngine;
using UnityEngine.UI;
using System;

public class RoomCreatePanel : UIPanel
{
    [Header("UI 요소")]
    [SerializeField] private InputField roomNameInputField;
    [SerializeField] private Toggle publicRoomToggle;
    [SerializeField] private Toggle privateRoomToggle;
    [SerializeField] private InputField roomPasswordInputField;
    [SerializeField] private Button confirmButton;
    [SerializeField] private Button cancelButton;

    public event Action<string, string> OnCreateSubmit;


    protected override void InitializeListener()
    {
        InitializeButtonListener();
        InitializeToggleListener();
    }

    protected override void UnregisterListener()
    {
        UnregisterButtonListener();
        UnRegisterToggleListener();
    }

    private void InitializeButtonListener() // 버튼 리스너를 초기화하는 함수
    {
        confirmButton?.onClick.AddListener(OnClickConfirmButton);
        cancelButton?.onClick.AddListener(OnClickCancelButton);
    }

    private void InitializeToggleListener() // 토글 리스너를 초기화하는 함수
    {
        publicRoomToggle?.onValueChanged.AddListener(OnPublicToggleChanged);
        privateRoomToggle?.onValueChanged.AddListener(OnPrivateToggleChanged);
    }

    private void UnregisterButtonListener() // 버튼 리스너를 해제하는 함수
    {
        confirmButton?.onClick.RemoveListener(OnClickConfirmButton);
        cancelButton?.onClick.RemoveListener(OnClickCancelButton);
    }

    private void UnRegisterToggleListener() // 토글 리스너를 해제하는 함수
    {
        publicRoomToggle?.onValueChanged.RemoveListener(OnPublicToggleChanged);
        privateRoomToggle?.onValueChanged.RemoveListener(OnPrivateToggleChanged);
    }

    private void OnClickConfirmButton() // 확인 버튼 클릭 시 실행되는 함수
    {
        SoundManager.instance?.Play(SoundKey.ButtonClick);
        OnCreateSubmit?.Invoke(roomNameInputField.text, roomPasswordInputField.text);
    }

    private void OnClickCancelButton() // 취소 버튼 클릭 시 실행되는 함수
    {
        SoundManager.instance?.Play(SoundKey.ButtonClick);
        Hide();
    }

    private void OnPublicToggleChanged(bool isOn) // 공개방 토글 클릭 시 실행되는 함수
    {
        if (isOn)
        {
            privateRoomToggle.SetIsOnWithoutNotify(false);
            roomPasswordInputField.interactable = false;
            roomPasswordInputField.text = "";
        }
    }

    private void OnPrivateToggleChanged(bool isOn) // 비공개방 토글 클릭 시 실행되는 함수
    {
        if (isOn)
        {
            publicRoomToggle.SetIsOnWithoutNotify(false);
            roomPasswordInputField.interactable = true;
        }
        else
        {
            roomPasswordInputField.interactable = false;
            roomPasswordInputField.text = "";
        }
    }

    public override void Hide()
    {
        ResetUI();
        base.Hide();
    }

    public override void HideImmediate()
    {
        ResetUI();
        base.HideImmediate();
    }

    protected override void ResetUI()
    {
        roomNameInputField.text = "";
        roomPasswordInputField.text = "";
        publicRoomToggle.isOn = true;
        privateRoomToggle.isOn = false;
        roomPasswordInputField.interactable = false;
    }
}
