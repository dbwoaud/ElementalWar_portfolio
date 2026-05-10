using UnityEngine;
using UnityEngine.UI;
using System;
using System.Collections.Generic;
using Photon.Realtime;

public class LobbyUIManager : BaseUIManager<LobbyUIManager>
{
    [Header("UI 설정")]
    [SerializeField] private Text nicknameText;
    [SerializeField] private Button randomRoomEntryButton;
    [SerializeField] private Button roomCreateButton;

    [Header("하위 패널 관리")]
    [SerializeField] private RoomCreatePanel roomCreatePanel;
    [SerializeField] private PasswordInputPanel passwordInputPanel;
    [SerializeField] private List<UIPanel> uiPanels = new List<UIPanel>();

    [Header("하위 컨테이너 관리")]
    [SerializeField] private RoomListContainer roomListContainer;

    public event Action OnClickRandomButton;
    public event Action<RoomInfo> OnClickRoomItem;
    public event Action<string, string> OnRoomCreateSubmit;
    public event Action<string> OnPasswordSubmit;
    public event Action OnPasswordCancel;


    protected override void InitUIElements()
    {
        uiPanels.Add(roomCreatePanel);
        uiPanels.Add(passwordInputPanel);
        HideAllPanelsImmediate();
    }

    private void HideAllPanelsImmediate() // 모든 하위 패널을 즉시 비활성화시키는 함수
    {
        foreach (var panel in uiPanels)
        {
            panel.HideImmediate();
        }
    }

    protected override void BindButtonEvent()
    {
        roomCreateButton?.onClick.AddListener(OnClickRoomCreateButton);
        randomRoomEntryButton?.onClick.AddListener(OnClickRandomRoomEntryButton);
    }

    protected override void BindPanelEvent()
    {
        if(roomCreatePanel != null)
        {
            roomCreatePanel.OnCreateSubmit += HandleCreateSubmit;
        }
        if(passwordInputPanel != null)
        {
            passwordInputPanel.OnSubmitPassword += HandlePasswordSubmit;
            passwordInputPanel.OnCancelClicked += HandlePasswordCancel;
        }
        if(roomListContainer != null)
        {
            roomListContainer.OnRoomItemClicked += HandleRoomItemClicked;
        }
    }

    protected override void UnbindButtonEvent()
    {
        roomCreateButton?.onClick.RemoveListener(OnClickRoomCreateButton);
        randomRoomEntryButton?.onClick.RemoveListener(OnClickRandomRoomEntryButton);
    }

    protected override void UnbindPanelEvent()
    {
        if(roomCreatePanel != null)
        {
            roomCreatePanel.OnCreateSubmit -= HandleCreateSubmit;
        }
        if(passwordInputPanel)
        {
            passwordInputPanel.OnSubmitPassword -= HandlePasswordSubmit;
            passwordInputPanel.OnCancelClicked -= HandlePasswordCancel;
        }

        if( roomListContainer != null)
        {
            roomListContainer.OnRoomItemClicked -= HandleRoomItemClicked;
        }
    }

    private void OnClickRoomCreateButton() // 방 생성 버튼 클릭 시 실행되는 함수
    {
        PlayButtonSound();
        roomCreatePanel.Show();
    }

    private void OnClickRandomRoomEntryButton() // 랜덤 방 입장 버튼 클릭 시 실행되는 함수  
    {
        PlayButtonSound();
        OnClickRandomButton?.Invoke();
    }

    private void HandleCreateSubmit(string name, string pw) // 방 생성 패널에서 방 생성 버튼 클릭 시 실행되는 함수
    {
        OnRoomCreateSubmit?.Invoke(name, pw);
    }

    private void HandlePasswordSubmit(string pw) // 비밀번호 패널에서 확인 버튼 클릭 시 실행되는 함수
    {
        OnPasswordSubmit?.Invoke(pw);
    }

    private void HandlePasswordCancel() // 비밀번호 패널에서 취소 버튼 클릭 시 실행되는 함수
    {
        OnPasswordCancel?.Invoke();
    }

    private void HandleRoomItemClicked(RoomInfo info) // 방 버튼 클릭 시 실행되는 함수
    {
        OnClickRoomItem?.Invoke(info);
    }

    public void SetNicknameUI(string nickname) // 닉네임을 출력하는 함수
    {
        nicknameText.text = nickname;
    }

    public void UpdateRoomListUI(List<RoomInfo> roomList) // 방 목록 UI를 업데이트하는 함수
    {
        roomListContainer?.UpdateRoomList(roomList);
    }

    public List<int> GetCurrentRoomNumbers() // 현재 방 번호를 얻는 함수
    {
        return roomListContainer.GetCurrentRoomNumbers();
    }

    public void HideRoomCreatePanel() // 방 생성 패널을 비활성화하는 함수
    {
        roomCreatePanel.Hide();
    }

    public void ShowPasswordPanel() // 비밀번호 패널을 활성화하는 함수
    {
        passwordInputPanel.Show();
    }

    public void HidePasswordPanel() // 비밀번호 패널을 비활성화하는 함수
    {
        passwordInputPanel.Hide();
    }
}
