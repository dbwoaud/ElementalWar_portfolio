using UnityEngine;

public class PlayerSlotContainer : MonoBehaviour
{
    [Header("플레이어 슬롯")]
    [SerializeField] private PlayerSlot[] playerSlots = new PlayerSlot[2];

    public void ClearAllSlots() // 모든 플레이어 슬롯을 초기화하는 함수
    {
        for (int i = 0; i < playerSlots.Length; i++)
            ClearSlot(i);
    }

    public void ClearSlot(int slotIndex) // 플레이어 슬롯을 초기화하는 함수
    {
        if (CheckValidIndex(slotIndex))
            playerSlots[slotIndex].ClearSlot();    
    }

    private bool CheckValidIndex(int slotIndex) // 유효한 인덱스를 확인하는 함수
    {
        return slotIndex >= 0 && slotIndex < playerSlots.Length;
    }

    public void UpdatePlayerSlot(int slotIndex, string name, bool isMaster, bool isReady) // 플레이어 슬롯 정보를 업데이트하는 함수
    {
        if (CheckValidIndex(slotIndex))
            playerSlots[slotIndex].SetSlot(name, isMaster, isReady);
    }
}
