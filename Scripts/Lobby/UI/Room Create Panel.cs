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

    public event Action<string, string> OnCreateSubmit; // 방 생성 버튼 클릭 이벤트


    protected override void RegisterListener() // UI 리스너를 등록하는 함수
    {
        RegisterButtonListener();
        RegisterToggleListener();
    }

    protected override void UnregisterListener() // UI 리스너를 해제하는 함수
    {
        UnregisterButtonListener();
        UnRegisterToggleListener();
    }

    protected override void ResetUI() // UI를 리셋시키는 함수
    {
        roomNameInputField.text = "";
        roomPasswordInputField.text = "";
        publicRoomToggle.isOn = true;
        privateRoomToggle.isOn = false;
        roomPasswordInputField.interactable = false;
    }

    public override void Hide() // 패널을 비활성화하는 함수
    {
        ResetUI();
        base.Hide();
    }

    public override void HideImmediate() // 패널을 즉시 비활성화하는 함수
    {
        ResetUI();
        base.HideImmediate();
    }

    private void RegisterButtonListener() // 버튼 리스너를 등록하는 함수
    {
        confirmButton?.onClick.AddListener(OnClickConfirmButton);
        cancelButton?.onClick.AddListener(OnClickCancelButton);
    }

    private void RegisterToggleListener() // 토글 리스너를 등록하는 함수
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
        SoundManager.Instance?.Play(SoundKey.ButtonClick);
        OnCreateSubmit?.Invoke(roomNameInputField.text, roomPasswordInputField.text);
    }

    private void OnClickCancelButton() // 취소 버튼 클릭 시 실행되는 함수
    {
        SoundManager.Instance?.Play(SoundKey.ButtonClick);
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
}