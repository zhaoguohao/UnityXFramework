using UnityEngine;


public class AudioMgr
{
    public AudioMgr()
    {
        m_cfg = new AudioCfg();
        m_channalMgr = new AudioChannels();
        m_cache = new AudioCache();
    }

    public void Init()
    {
        m_cfg.Initialize();
        m_channalMgr.Initialize();

        m_soundVolumeFactor = PlayerPrefs.GetFloat("SOUND_VOLUME_FACTOR", 1f);
        m_musicVolumeFactor = PlayerPrefs.GetFloat("MUSIC_VOLUME_FACTOR", 1f);
        UpdateSoundVolume(m_soundVolumeFactor);
        UpdateMusicVolume(m_musicVolumeFactor);
    }

    /// <summary>
    /// 播放音效
    /// </summary>
    /// <param name="audioName">声音名字，需要带后缀</param>
    public void PlaySound(string audioName)
    {
        var clip = m_cache.GetAudioClip(audioName);
        if (null == clip) return;
        m_channalMgr.PlaySound(clip);
    }

    /// <summary>
    /// 播放音效
    /// </summary>
    /// <param name="audioName">声音名字，需要带后缀</param>
    /// <param name="loop">是否循环</param>
    /// <param name="fadeIn">是否渐入</param>
    public void PlaySoundEx(string audioName, bool loop, bool fadeIn)
    {
        var clip = m_cache.GetAudioClip(audioName);
        if (null == clip) return;
        m_channalMgr.PlaySound(clip, loop, fadeIn);
    }

    /// <summary>
    /// 播放音乐，比如背景音乐
    /// </summary>
    /// <param name="audioName">声音名字，需要带后缀</param>
    /// <param name="loop">是否循环</param>
    /// <param name="fadeIn">是否渐入</param>
    /// <param name="pauseOther">是否停止其他背景音乐</param>
    public void PlayMusic(string audioName, bool loop, bool fadeIn, bool pauseOther)
    {
        var cfgItem = m_cfg.GetAudioCfg(audioName);
        if (null == cfgItem) return;
        var clip = m_cache.GetAudioClip(audioName);
        if (null == clip) return;
        m_channalMgr.PlayMusic(clip, loop, fadeIn, cfgItem.channel, pauseOther);
    }

    public void StopSound(string audioName)
    {
        var clip = m_cache.GetExistAudioClip(audioName);
        if (null != clip)
        {
            m_channalMgr.StopSound(clip);
        }
    }

    public void PauseSound(string audioName)
    {
        var clip = m_cache.GetExistAudioClip(audioName);
        if (null != clip)
        {
            m_channalMgr.PauseSound(clip);
        }
    }

    public void StopAll()
    {
        m_channalMgr.StopAll();
    }


    public AudioCfgItem GetCfgItem(string audioName)
    {
        return m_cfg.GetAudioCfg(audioName);
    }

    /// <summary>
    /// 调节音效音量
    /// </summary>
    /// <param name="factor">0到1</param>
    public void UpdateSoundVolume(float factor)
    {
        m_soundVolumeFactor = factor;
        m_channalMgr.UpdateSoundVolume(factor);
        PlayerPrefs.SetFloat("SOUND_VOLUME_FACTOR", factor);

    }

    /// <summary>
    /// 调节音乐音量
    /// </summary>
    /// <param name="factor">0到1</param>
    public void UpdateMusicVolume(float factor)
    {
        m_musicVolumeFactor = factor;
        m_channalMgr.UpdateMusicVolume(factor);
        PlayerPrefs.SetFloat("MUSIC_VOLUME_FACTOR", factor);
    }

    private AudioCfg m_cfg;
    private AudioChannels m_channalMgr;
    private AudioCache m_cache;
    /// <summary>
    /// 音效音量
    /// </summary>
    private float m_soundVolumeFactor = 1f;
    /// <summary>
    /// 音乐音量
    /// </summary>
    private float m_musicVolumeFactor = 1f;


    private static AudioMgr s_instance;
    public static AudioMgr instance
    {
        get
        {
            if (null == s_instance)
                s_instance = new AudioMgr();
            return s_instance;
        }
    }
}
