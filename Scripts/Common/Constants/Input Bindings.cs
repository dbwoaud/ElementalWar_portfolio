using UnityEngine;

public static class InputBindings
{
    public static readonly KeyCode[] UnitNumberKeys =
    {
        KeyCode.Alpha1, KeyCode.Alpha2, KeyCode.Alpha3, KeyCode.Alpha4, KeyCode.Alpha5,
        KeyCode.Alpha6, KeyCode.Alpha7, KeyCode.Alpha8, KeyCode.Alpha9, KeyCode.Alpha0
    };
    public const int DeckSize = 10;
    public static readonly KeyCode CannonFireKey = KeyCode.Space;
    public static readonly KeyCode[] EnergyUpgradeKeys = { KeyCode.LeftShift, KeyCode.RightShift };
}