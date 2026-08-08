using UnityEngine;

public static class InputBindings
{
    public static readonly KeyCode[] UnitNumberKeys = // 유닛 단축키
    {
        KeyCode.Alpha1, KeyCode.Alpha2, KeyCode.Alpha3, KeyCode.Alpha4, KeyCode.Alpha5,
        KeyCode.Alpha6, KeyCode.Alpha7, KeyCode.Alpha8, KeyCode.Alpha9, KeyCode.Alpha0
    };
    public const int DeckSize = 10;
    public static readonly KeyCode CannonFireKey = KeyCode.Space; // 대포 발사 키
    public static readonly KeyCode[] EnergyUpgradeKeys = { KeyCode.LeftShift, KeyCode.RightShift }; // 에너지 업그레이드 키
}