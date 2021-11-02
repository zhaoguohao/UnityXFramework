using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class AudioCache
{
    public AudioCache()
    {
        m_name2Clip = new Dictionary<string, AudioClip>();
    }

    public AudioClip GetAudioClip(string audioName)
    {
        if (m_name2Clip.ContainsKey(audioName))
        {
            return m_name2Clip[audioName];
        }
        var cfgItem = AudioMgr.instance.GetCfgItem(audioName);
        if (null == cfgItem) return null;
        var clip = ResourceManager.instance.Instantiate<AudioClip>(cfgItem.id);
        m_name2Clip.Add(audioName, clip);
        return clip;
    }

    public AudioClip GetExistAudioClip(string audioName)
    {
        AudioClip clip = null;
        m_name2Clip.TryGetValue(audioName, out clip);
        return clip;
    }

    private Dictionary<string, AudioClip> m_name2Clip;
}


