using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.SceneManagement;

public class AudioManager : Singleton<AudioManager>
{
    [Header("BGM Settings")]
    [SerializeField, ChineseLabel("当前BGM")] private AudioSource currentBGM;
    [SerializeField, ChineseLabel("BGM 混合器")] private AudioMixerGroup bgmMixerGroup;
    [SerializeField, ChineseLabel("Level 1 BGM")] private AudioClip level1BGM;
    [SerializeField, ChineseLabel("Level 2 BGM")] private AudioClip level2BGM;

    [Header("音效设置")]
    [SerializeField, ChineseLabel("同时播放的最大音效数量")] private int maxSimultaneousSFX = 15;
    [SerializeField, ChineseLabel("音效 混合器")] private AudioMixerGroup audioMixerGroup;

    private int currentSimultaneousSFX = 0;
    private Queue<AudioController> SFXaudioSourcePool;
    private Dictionary<AudioClip, int> maxPlayCountPerSFX = new();
    private Dictionary<AudioClip, int> currentPlayCountPerSFX = new();
    private Dictionary<AudioClip, float> cooldownPerSFX = new();
    private Dictionary<AudioClip, float> lastPlayTimePerSFX = new();

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    private static void Bootstrap()
    {
        _ = Instance;
    }

    protected override void Awake()
    {
        base.Awake();

        if (!bgmMixerGroup || !audioMixerGroup)
        {
            AudioMixer mixer = Resources.Load<AudioMixer>("Audio/MainAudioMixer");
            if (mixer != null)
            {
                bgmMixerGroup = mixer.FindMatchingGroups("BGM")[0];
                audioMixerGroup = mixer.FindMatchingGroups("SFX")[0];
            }
            else
            {
                Debug.LogError("未找到音频混合器，请确认 Resources/Audio/MainAudioMixer 存在");
            }
        }

        if (currentBGM == null)
        {
            currentBGM = gameObject.AddComponent<AudioSource>();
            currentBGM.outputAudioMixerGroup = bgmMixerGroup;
            currentBGM.loop = true;
            currentBGM.playOnAwake = false;
        }

        if (level1BGM == null)
        {
            level1BGM = Resources.Load<AudioClip>("music/BGM/mx_lvl1_maintheme_cueV1");
        }

        if (level2BGM == null)
        {
            level2BGM = Resources.Load<AudioClip>("music/BGM/mx_lvl2_maintheme_cueV1");
        }

        ApplySceneBGM(SceneManager.GetActiveScene().name);

        SFXaudioSourcePool = new Queue<AudioController>();
        for (int i = 0; i < maxSimultaneousSFX; i++)
        {
            AudioController newAudioController = gameObject.AddComponent<AudioController>();
            AudioSource newAudioSource = gameObject.AddComponent<AudioSource>();
            newAudioSource.playOnAwake = false;
            newAudioSource.outputAudioMixerGroup = audioMixerGroup;
            newAudioController.SetAudioSource(newAudioSource);
            SFXaudioSourcePool.Enqueue(newAudioController);
        }
    }

    protected override void OnEnable()
    {
        base.OnEnable();
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    protected override void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
        base.OnDisable();
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode loadSceneMode)
    {
        ApplySceneBGM(scene.name);
    }

    private void ApplySceneBGM(string sceneName)
    {
        switch (sceneName)
        {
            case "Level 1":
                SwitchBGM(level1BGM);
                break;
            case "Level 2":
                SwitchBGM(level2BGM);
                break;
            default:
                StopBGM();
                break;
        }
    }

    public void SwitchBGM(AudioClip bgmClip)
    {
        if (bgmClip == null)
        {
            StopBGM();
            currentBGM.clip = null;
            return;
        }

        if (currentBGM.clip == bgmClip)
        {
            if (!currentBGM.isPlaying)
            {
                currentBGM.Play();
            }
            return;
        }

        currentBGM.Stop();
        currentBGM.clip = bgmClip;
        currentBGM.Play();
    }

    public void PlayBGM()
    {
        if (currentBGM.clip == null) return;

        currentBGM.Play();
    }

    public void StopBGM()
    {
        currentBGM.Stop();
    }

    public void CreateSFXPool(AudioClip clip, int maxCount, float cooldown = 0.2f)
    {
        if (clip == null || maxCount <= 0) return;

        maxPlayCountPerSFX[clip] = maxCount;
        currentPlayCountPerSFX[clip] = 0;
        SetSFXCooldown(clip, cooldown);
    }

    public void SetSFXCooldown(AudioClip clip, float cooldown)
    {
        if (clip == null || cooldown < 0) return;

        cooldownPerSFX[clip] = cooldown;
        lastPlayTimePerSFX[clip] = -cooldown;
    }

    public void PlaySFX(AudioClip clip, int maxCount = 0, float cooldown = 0.2f)
    {
        if (clip == null) return;

        if (!maxPlayCountPerSFX.ContainsKey(clip))
        {
            if (maxCount <= 0)
            {
                Debug.LogWarning($"音效 {clip.name} 未注册 maxCount");
                return;
            }

            CreateSFXPool(clip, maxCount, cooldown);
        }

        if (!CanPlaySFX(clip)) return;

        AudioController controller = SFXaudioSourcePool.Dequeue();

        currentPlayCountPerSFX[clip]++;
        currentSimultaneousSFX++;
        lastPlayTimePerSFX[clip] = Time.time;

        controller.Play(clip);
    }

    public void RecycleSFX(AudioClip clip, AudioController audioController)
    {
        if (!currentPlayCountPerSFX.ContainsKey(clip)) return;

        currentPlayCountPerSFX[clip] = Mathf.Max(0, currentPlayCountPerSFX[clip] - 1);
        currentSimultaneousSFX = Mathf.Max(0, currentSimultaneousSFX - 1);
        SFXaudioSourcePool.Enqueue(audioController);
    }

    private bool CanPlaySFX(AudioClip clip)
    {
        if (clip == null) return false;
        if (!maxPlayCountPerSFX.ContainsKey(clip) || !currentPlayCountPerSFX.ContainsKey(clip)) return false;
        if (currentPlayCountPerSFX[clip] >= maxPlayCountPerSFX[clip]) return false;
        if (currentSimultaneousSFX >= maxSimultaneousSFX) return false;
        if (SFXaudioSourcePool.Count == 0) return false;

        if (lastPlayTimePerSFX.TryGetValue(clip, out float lastPlayTime) &&
            cooldownPerSFX.TryGetValue(clip, out float cooldown) &&
            Time.time - lastPlayTime < cooldown)
        {
            return false;
        }

        return true;
    }
}
