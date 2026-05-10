public interface ISoundService
{
    void Play(SoundKey key, float volumeScale = 1f);
    void Stop(SoundChannel channel);
    void StopAll();
}