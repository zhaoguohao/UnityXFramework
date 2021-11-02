
using System.Collections.Generic;
using UnityEngine;


public class AudioChannels
{
    public void Initialize()
    {
        m_rootObj = new GameObject("AudioChannelRoot");
        var rootTrans = m_rootObj.transform;
        for (int i = 0; i < SOUND_CHANNEL_CNT; ++i)
        {
            var channelObj = new GameObject("SoundChannel_" + i);
            channelObj.transform.SetParent(rootTrans, false);
            var channel = channelObj.AddComponent<AudioChannelItem>();
            m_soundList.Add(channel);
        }

        for (int i = 0; i < MUSIC_CHANNEL_CNT; ++i)
        {
            var channelObj = new GameObject("MusicChannel_" + i);
            channelObj.transform.SetParent(rootTrans, false);
            var channel = channelObj.AddComponent<AudioChannelItem>();
            m_musicList.Add(channel);
        }
    }

    public void PlaySound(AudioClip clip, bool loop = false, bool fadeIn = false)
    {
        var channel = GetIdleSoundChannel();
        if (null != channel)
            channel.Play(clip, loop, fadeIn);
    }

    public void PlayMusic(AudioClip clip, bool loop, bool fadeIn, int channelId, bool pauseOther)
    {
        if (pauseOther)
        {
            for (int i = 0; i < MUSIC_CHANNEL_CNT; ++i)
            {
                if (channelId != i)
                {
                    m_musicList[i].Stop(false);
                }
            }
        }
        var channel = GetIdleMusicChannel(channelId);
        if (null != channel)
            channel.Play(clip, loop, fadeIn);
    }

    public void StopSound(AudioClip clip)
    {
        for (int i = 0; i < SOUND_CHANNEL_CNT; ++i)
        {
            var channel = m_soundList[i];
            if (clip == channel.curClip)
            {
                if (channel.IsPlaying())
                {
                    channel.Stop(false);
                }
            }
        }
    }

    public void StopAll()
    {
        for (int i = 0; i < SOUND_CHANNEL_CNT; ++i)
        {
            m_soundList[i].Stop(false);
        }
        for (int i = 0; i < MUSIC_CHANNEL_CNT; ++i)
        {
            m_musicList[i].Stop(false);
        }
    }

    public void PauseSound(AudioClip clip)
    {
        for (int i = 0; i < SOUND_CHANNEL_CNT; ++i)
        {
            var channel = m_soundList[i];
            if (clip == channel.curClip)
            {
                if (channel.IsPlaying())
                {
                    channel.Pause(false);
                }
            }
        }
    }

    /// <summary>
    /// 获取一个空闲的Sound轨道
    /// </summary>
    /// <returns></returns>
    private AudioChannelItem GetIdleSoundChannel()
    {
        for (int i = 0; i < SOUND_CHANNEL_CNT; ++i)
        {
            if (!m_soundList[i].IsPlaying())
            {
                return m_soundList[i];
            }
        }
        return null;
    }

    /// <summary>
    /// 获取一个空闲的Music轨道
    /// </summary>
    /// <param name="channel"></param>
    /// <returns></returns>
    public AudioChannelItem GetIdleMusicChannel(int channel = -1)
    {
        if (-1 == channel)
        {
            for (int i = 0; i < MUSIC_CHANNEL_CNT; ++i)
            {
                if (!m_musicList[i].IsPlaying())
                {
                    return m_musicList[i];
                }
            }
        }
        else if (channel < MUSIC_CHANNEL_CNT)
        {
            return m_musicList[channel];
        }
        else
        {
            GameLogger.LogError("GetIdleMusicChannel Error, channel: " + channel);
        }
        return null;
    }

    /// <summary>
    /// 更新Sound的音量
    /// </summary>
    /// <param name="factor"></param>
    public void UpdateSoundVolume(float factor)
    {
        for (int i = 0; i < SOUND_CHANNEL_CNT; ++i)
        {
            m_soundList[i].UpdateVolume(factor);
        }
    }

    /// <summary>
    /// 更新Music的音量
    /// </summary>
    /// <param name="factor"></param>
    public void UpdateMusicVolume(float factor)
    {
        for (int i = 0; i < MUSIC_CHANNEL_CNT; ++i)
        {
            m_musicList[i].UpdateVolume(factor);
        }
    }

    private const int SOUND_CHANNEL_CNT = 8;
    private const int MUSIC_CHANNEL_CNT = 4;

    private List<AudioChannelItem> m_soundList = new List<AudioChannelItem>();
    private List<AudioChannelItem> m_musicList = new List<AudioChannelItem>();

    private GameObject m_rootObj;
}
