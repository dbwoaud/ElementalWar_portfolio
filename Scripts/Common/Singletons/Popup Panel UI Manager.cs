using System;
using System.Collections.Generic;
using UnityEngine;

public enum PopupType { Error, Confirm, Selection, Waiting}

public class PopupPanelUIManager : Singleton<PopupPanelUIManager>
{
    [Header("팝업 패널 프리팹")]
    [SerializeField] private ErrorPopupPanel errorPanel;
    [SerializeField] private ConfirmPopupPanel confirmPanel;
    [SerializeField] private SelectionPopupPanel selectionPanel;
    [SerializeField] private WaitingPopupPanel waitingPanel;

    [Header("부모 캔버스")]
    [SerializeField] private Transform canvasRoot;

    private Dictionary<PopupType, BasePopupPanel> popupCache = new Dictionary<PopupType, BasePopupPanel>();


    public void ShowError(string message, Action action = null) // 에러 팝업 패널 활성화 함수
    {
        GetOrCreatePopup<ErrorPopupPanel>(PopupType.Error).Setup(message, action);
    }

    public void ShowConfirm(string message, Action action = null) // 확인 팝업 패널 활성화 함수
    {
        GetOrCreatePopup<ConfirmPopupPanel>(PopupType.Confirm).Setup(message, action);
    }

    public void ShowSelection(string message, Action onYes, Action onNo) // 선택 팝업 패널 활성화 함수
    {
        GetOrCreatePopup<SelectionPopupPanel>(PopupType.Selection).Setup(message, onYes, onNo);
    }

    public void ShowWaiting(string message, Action onCancel = null) // 대기 팝업 패널 활성화 함수
    {
        GetOrCreatePopup<WaitingPopupPanel>(PopupType.Waiting).Setup(message, onCancel);
    }

    public void HideWaiting() // 대기 팝업 패널 비활성화 함수
    {
        GetOrCreatePopup<WaitingPopupPanel>(PopupType.Waiting).Close();
    }

    private T GetOrCreatePopup<T>(PopupType type) where T : BasePopupPanel // 팝업 종류에 맞는 프리팹을 캐시에서 반환하거나 생성하는 함수
    {
        if (popupCache.TryGetValue(type, out var existingPopup))
            return existingPopup as T;

        T prefab = GetPopupPrefab<T>(type);
        if (prefab != null)
        {
            T newPopup = Instantiate(prefab, canvasRoot);
            newPopup.HideImmediate();
            popupCache.Add(type, newPopup);
            return newPopup;
        }

        return null;
    }

    private T GetPopupPrefab<T>(PopupType type) where T : BasePopupPanel // 팝업 종류에 맞는 팝업 프리팹을 반환하는 함수
    {
        return type switch
        {
            PopupType.Error => errorPanel as T,
            PopupType.Confirm => confirmPanel as T,
            PopupType.Selection => selectionPanel as T,
            PopupType.Waiting => waitingPanel as T,
            _ => null
        };
    }
}
