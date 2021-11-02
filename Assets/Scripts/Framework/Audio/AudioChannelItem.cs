using System.Collections;
using UnityEngine;


public class AudioChannelItem : MonoBehaviour
{
    private void Awake()
    {
        baseVolume = 1;
        m_source = gameObject.AddComponent<AudioSource>();
    }


    public void Play(AudioClip clip, bool loop, bool fadeIn)
    {
        m_source.clip = clip;
        curClip = clip;
        m_source.loop = loop;
        m_source.Play();
        if (fadeIn)
        {
            baseVolume = 0;
            StopFade();
            m_fadeItr = FadeIn();
            StartCoroutine(m_fadeItr);
        }
    }

    public void Stop(bool fadeOut)
    {
        if (fadeOut)
        {
            StopFade();
            m_fadeItr = FadeOut(() =>
            {
                m_source.Stop();
            });
            StartCoroutine(m_fadeItr);
        }
        else
        {
            m_source.Stop();
        }
    }

    public void Pause(bool fadeOut)
    {
        if (fadeOut)
        {
            StopFade();
            m_fadeItr = FadeOut(() =>
            {
                m_source.Pause();
            });
            StartCoroutine(m_fadeItr);
        }
        else
        {
            m_source.Pause();
        }
    }

    public bool IsPlaying()
    {
        return m_source.isPlaying;
    }

    public void UpdateVolume(float factor)
    {
        if (null == m_source) return;
        m_source.volume = baseVolume * factor;
    }

    private IEnumerator FadeIn()
    {
        for (int i = 0; i <= 50; ++i)
        {
            baseVolume = i * 0.02f;
            yield return new WaitForSeconds(0.01f);
        }
        m_fadeItr = null;
    }

    private IEnumerator FadeOut(System.Action cb)
    {
        for (int i = 50; i >= 0; --i)
        {
            baseVolume = i * 0.02f;
            yield return new WaitForSeconds(0.01f);
        }
        if (null != cb)
            cb();
        m_fadeItr = null;
    }

    private void StopFade()
    {
        if (null != m_fadeItr)
        {
            StopCoroutine(m_fadeItr);
            m_fadeItr = null;
        }
    }

    private IEnumerator m_fadeItr;

    public AudioType audioType { get; set; }

    public float baseVolume
    {
        get { return m_baseVolume; }
        set
        {
            m_baseVolume = value;
        }
    }

    /// <summary>
    /// 基础音量
    /// </summary>
    private float m_baseVolume;

    private AudioSource m_source;
    public AudioClip curClip { get; private set; }

    public enum AudioType
    {
        Sound,
        Music,
    }
}
