using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;

public class AudioManager : Singleton<AudioManager>
{
    [Header("BGM 设置")]
    [SerializeField, ChineseLabel("当前BGM")] private AudioSource currentBGM;
    [SerializeField, ChineseLabel("BGM 混合器")] private AudioMixerGroup bgmMixerGroup;

    


    protected override void Awake()
    {
        base.Awake();

        //设置BGM
        if (currentBGM == null)
        {
            currentBGM = gameObject.AddComponent<AudioSource>();
            currentBGM.outputAudioMixerGroup = bgmMixerGroup;
            currentBGM.loop = true;
        }
        

        // 预先创建一些 AudioSource 以供使用
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

    #region BGM控制相关
    /// <summary>
    /// 切换BGM
     /// <para> 如果当前正在播放的BGM与传入的相同，则不进行切换 </para>
     /// <para> 如果传入的BGM为 null，则停止当前BGM </para>
     /// <para> 否则，切换到新的BGM </para>
    /// </summary>
    public void SwitchBGM(AudioClip bgmClip)
    {
        if (bgmClip == null) return;

        currentBGM.Stop();
        currentBGM.clip = bgmClip;
        currentBGM.Play();
    }

    /// <summary>
    /// 播放当前BGM
    /// </summary>
    public void PlayBGM()
    {
        if (currentBGM.clip == null) return;

        currentBGM.Play();
    }

    /// <summary>
    /// 停止当前BGM
    /// </summary>
    public void StopBGM()
    {
        currentBGM.Stop();
    }

    #endregion

    #region SFX控制相关

    [Header("音效设置")]
    [SerializeField, ChineseLabel("同时播放的最大音效数量")] private int maxSimultaneousSFX = 15;

    /// <summary>
    /// 当前正在播放的音效数量
     /// <para> 用于限制同时播放的音效数量，防止过多音效导致性能问题 </para>
     /// <para> 在 PlaySFX 中增加，在 RecycleSFX 中减少 </para>
     /// <para> 通过 maxSimultaneousSFX 控制最大数量 </para>
    /// </summary>
    private int currentSimultaneousSFX = 0;

    [SerializeField, ChineseLabel("音效 混合器")] private AudioMixerGroup audioMixerGroup;

    /// <summary>
    /// 音效池
    /// </summary>
    private Queue<AudioController> SFXaudioSourcePool;

    /// <summary>
    /// 每种音效最多播放的数量
    /// </summary>
    private Dictionary<AudioClip, int> maxPlayCountPerSFX = new();

    /// <summary>
    /// 每种音效当前播放的数量
    /// </summary>
    private Dictionary<AudioClip, int> currentPlayCountPerSFX = new();

    ///<summary>
    /// 每个音效的冷却时间
    /// </summary>
    private Dictionary<AudioClip, float> cooldownPerSFX = new();

    /// <summary>
    /// 每个音效的上一次播放时间
    /// </summary>
    private Dictionary<AudioClip, float> lastPlayTimePerSFX = new();

    /// <summary>
    /// 建立音效池
    /// </summary>
    public void CreateSFXPool(AudioClip clip, int maxCount, float cooldown = 0.2f)
    {
        if (clip == null || maxCount <= 0) return;

        maxPlayCountPerSFX[clip] = maxCount;
        currentPlayCountPerSFX[clip] = 0;
        SetSFXCooldown(clip, cooldown);
    }

    /// <summary>
    /// 设置音效的冷却时间
    /// </summary>
    public void SetSFXCooldown(AudioClip clip, float cooldown)
    {
        if (clip == null || cooldown < 0) return;

        cooldownPerSFX[clip] = cooldown;
        lastPlayTimePerSFX[clip] = -cooldown; // 初始化为负数，表示可以立即播放
    }

    /// <summary>
    /// 播放音效
    /// <para> 如果音效池中没有可用的 AudioSource 或者该音效已经达到最大播放数量，则不播放 </para>
    /// <para> 否则，从池中取出一个 AudioSource 播放</para>
    /// </summary>
    public void PlaySFX(AudioClip clip, int maxCount = 0, float cooldown = 0.2f)
    {
        if (clip == null) return;

        // 如果没注册，自动注册
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

    /// <summary>
    /// 回收音效
    /// </summary>
    public void RecycleSFX(AudioClip clip, AudioController audioController)
    {
        if (!currentPlayCountPerSFX.ContainsKey(clip)) return;
        currentPlayCountPerSFX[clip] = Mathf.Max(0, currentPlayCountPerSFX[clip] - 1);
        currentSimultaneousSFX = Mathf.Max(0, currentSimultaneousSFX - 1);
        SFXaudioSourcePool.Enqueue(audioController);
    }

    ///<summary>
    /// 检测是否可以播放指定音效
    /// </summary>
    private bool CanPlaySFX(AudioClip clip)
    {
        // 检测音效是否存在
        if (clip == null) return false;

        // 字典中是否存在该音效的播放限制
        if (!maxPlayCountPerSFX.ContainsKey(clip) || !currentPlayCountPerSFX.ContainsKey(clip)) return false;

        // 检测该音效是否已经达到最大播放数量
        if (currentPlayCountPerSFX[clip] >= maxPlayCountPerSFX[clip]) return false;

        // 检测当前同时播放的音效数量是否已经达到全局限制
        if (currentSimultaneousSFX >= maxSimultaneousSFX) return false;

        // 检测池中是否有可用的 AudioController
        if (SFXaudioSourcePool.Count == 0) return false;

        // 检测音效是否处于冷却状态
        if (lastPlayTimePerSFX.TryGetValue(clip, out float lastPlayTime))
        {
            if (cooldownPerSFX.TryGetValue(clip, out float cooldown))
            {
                if (Time.time - lastPlayTime < cooldown) return false;
            }
        }

        return true;
    }

    #endregion

}
