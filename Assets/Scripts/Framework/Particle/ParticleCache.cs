using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// 特效缓存
/// </summary>
public class ParticleCache
{
    public ParticleCache()
    {
        m_particleCacheMap = new Dictionary<int, List<ParticleTimer>>();
    }

    public void Add(int id, ParticleTimer p)
    {
        List<ParticleTimer> list = null;
        if (m_particleCacheMap.TryGetValue(id, out list))
        {
            list.Add(p);
        }
        else
        {
            List<ParticleTimer> objList = new List<ParticleTimer>();
            objList.Add(p);
            m_particleCacheMap.Add(id, objList);
        }
    }

    public ParticleTimer Get(int id)
    {
        List<ParticleTimer> list = null;
        if (m_particleCacheMap.TryGetValue(id, out list))
        {
            if (list.Count > 0)
            {
                ParticleTimer p = list[0];
                list.RemoveAt(0);

                return p;
            }
        }

        return null;
    }

    public void Remove(ParticleTimer timer)
    {
        List<ParticleTimer> list = null;
        if (m_particleCacheMap.TryGetValue(timer.id, out list))
        {
            for (int i = 0, cnt = list.Count; i < cnt; ++i)
            {
                if (list[i] == timer)
                {
                    list.RemoveAt(i);
                }
            }
        }
    }

    public void Clear()
    {
        foreach (var p in m_particleCacheMap)
        {
            List<ParticleTimer> list = p.Value;
            while (list.Count > 0)
            {
                ParticleTimer timer = list[0];
                if (timer != null && timer.gameObject != null)
                    Object.Destroy(timer.gameObject);
                list.RemoveAt(0);
            }
        }
    }

    private Dictionary<int, List<ParticleTimer>> m_particleCacheMap;

}