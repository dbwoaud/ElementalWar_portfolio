using UnityEngine;
using UnityEngine.UI;

public class PlayerSlot : MonoBehaviour
{
    [Header("UI 요소")]
    [SerializeField] private GameObject playerSlot;
    [SerializeField] private Text playerNameText;
    [SerializeField] private Text readyStateText;
    [SerializeField] private Image roomManagerIcon;


    public void ClearSlot() // 플레이어 슬롯 정보를 초기화하는 함수
    {
        playerNameText.text = "";
        playerNameText.gameObject.SetActive(false);
        readyStateText.gameObject.SetActive(false);
        roomManagerIcon.gameObject.SetActive(false);
    }

    public void SetSlot(string name, bool isMaster, bool isReady) // 플레이어 슬롯 정보를 설정하는 함수
    {
        playerNameText.text = name;
        playerNameText.gameObject.SetActive(true);
        readyStateText.gameObject.SetActive(isReady);
        roomManagerIcon.gameObject.SetActive(isMaster);
    }
}
