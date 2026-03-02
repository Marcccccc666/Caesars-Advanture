using UnityEngine;
using System.Collections;

public class AudioController : MonoBehaviour
{
    private AudioSource audioSource;
    private AudioManager audioManager => AudioManager.Instance;

    /// <summary>
    /// 播放指定音效
    /// </summary>
    /// <param name="clip">要播放的音效</param>
    public void Play(AudioClip clip)
    {
        if (clip == null) return;

        audioSource.clip = clip;
        audioSource.Play();
        StartCoroutine(PlaySFXCoroutine());
    }

    public void SetAudioSource(AudioSource source)
    {
        audioSource = source;
    }

    /// <summary>
    /// 协程播放音效，等待音效播放完成后将 AudioSource 归还给 AudioManager
    /// </summary>
    private IEnumerator PlaySFXCoroutine()
    {
        yield return new WaitForSeconds(audioSource.clip.length);
        audioManager.RecycleSFX(audioSource.clip, this);
    }
}
