using System.Collections.Generic;
using UnityEngine;

public class SoundManager : Singleton<SoundManager>
{
    [Header("오디오 소스")]
    [SerializeField] private AudioSource bgmAudioSource;
    [SerializeField] private AudioSource sfxAudioSource;

    [Header("오디오 클립")]
    [SerializeField] private SoundEntry[] entries;
    private readonly Dictionary<SoundKey, SoundEntry> soundTable = new Dictionary<SoundKey, SoundEntry>();


    protected override void Awake()
    {
        base.Awake();
        if (instance != this)
            return;

        BuildSoundTableData();
    }

    private void BuildSoundTableData() // 인스펙터에 등록된 사운드 정보들을 딕셔너리로 변환하는 함수
    {
        soundTable.Clear();
        if (entries == null)
            return;

        for (int i = 0; i < entries.Length; i++)
        {
            var entry = entries[i];
            if (entry.key == SoundKey.None || entry.clip == null)
                continue;

            if (!soundTable.ContainsKey(entry.key))
                soundTable.Add(entry.key, entry);
        }
    }

    public void Play(SoundKey key, float volumeScale = 1f) // 키에 매핑된 사운드를 재생하는 함수
    {
        if (!soundTable.TryGetValue(key, out SoundEntry entry))
            return;

        switch (entry.channel)
        {
            case SoundChannel.SFX:
                PlaySFX(entry, volumeScale);
                break;
            case SoundChannel.BGM:
                PlayBGM(entry, volumeScale);
                break;
        }
    }

    public void Stop(SoundChannel channel) // 지정된 채널의 사운드를 멈추는 함수
    {
        switch (channel)
        {
            case SoundChannel.SFX:
                if (sfxAudioSource != null) 
                    sfxAudioSource.Stop();
                break;
            case SoundChannel.BGM:
                if (bgmAudioSource != null) 
                    bgmAudioSource.Stop();
                break;
        }
    }

    public void StopAll() // 모든 사운드를 멈추는 함수
    {
        Stop(SoundChannel.SFX);
        Stop(SoundChannel.BGM);
    }

    private void PlaySFX(SoundEntry entry, float volumeScale) // SFX 채널에 사운드를 재생하는 함수
    {
        if (sfxAudioSource == null || entry.clip == null)
            return;

        sfxAudioSource.PlayOneShot(entry.clip, entry.defaultVolume * volumeScale);
    }

    private void PlayBGM(SoundEntry entry, float volumeScale) // BGM 채널에 사운드를 재생하는 함수
    {
        if (bgmAudioSource == null || entry.clip == null)
            return;

        if (bgmAudioSource.clip == entry.clip && bgmAudioSource.isPlaying)
            return;

        bgmAudioSource.Stop();
        bgmAudioSource.clip = entry.clip;
        bgmAudioSource.volume = entry.defaultVolume * volumeScale;
        bgmAudioSource.Play();
    }

    public void PlayDynamicBGM(AudioClip clip, float volume = 1f) // 맵에 따른 동적 배경음악을 재생하는 함수
    {
        if (clip == null || bgmAudioSource == null)
            return;
        if (bgmAudioSource.clip == clip && bgmAudioSource.isPlaying)
            return;
        bgmAudioSource.Stop();
        bgmAudioSource.clip = clip;
        bgmAudioSource.volume = volume;
        bgmAudioSource.Play();
    }
}
