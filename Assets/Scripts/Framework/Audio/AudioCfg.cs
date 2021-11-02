using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AudioCfg
{
    public void Initialize()
    {
        m_audioCfg = new ConfigFile<AudioCfgItem>("audioConfig");
    }

    public AudioCfgItem GetAudioCfg(string audioName)
    {
        if (null == m_audioCfg) return null;
        var cfgItem = m_audioCfg.GetItem(audioName);
        if (null == cfgItem)
        {
            GameLogger.LogError("null == audioCfg, name: " + audioName);
        }
        return cfgItem;
    }


    private ConfigFile<AudioCfgItem> m_audioCfg;

}
