using System;
using UnityEngine;

public enum SoundKey
{
    None = 0,

    ButtonClick = 100,

    FireCannon = 200,
    Explosion = 201,
    CastleHit = 202,

    EnergyConsume = 300,

    UnitSpawn = 400,
    UnitCoolTimeEnd = 401,
    UnitAttack = 402,
    UnitHit = 403,
    UnitDie = 404,

    GameStartCue = 500,
    GameWin = 501,
    GameLose = 502,

    MainMenuBGM = 1000,
    LobbyBGM = 1001,
    RoomBGM = 1002,
    UnitSettingBGM = 1003,
}

public enum SoundChannel
{
    SFX = 0,
    BGM = 1,
}

[Serializable]
public struct SoundEntry
{
    [Tooltip("사운드 식별자")]
    public SoundKey key;

    [Tooltip("재생할 오디오 클립")]
    public AudioClip clip;

    [Tooltip("재생할 오디오 소스")]
    public SoundChannel channel;

    [Tooltip("기본 볼륨")]
    [Range(0f, 4f)]
    public float defaultVolume;
}