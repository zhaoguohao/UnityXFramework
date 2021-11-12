using UnityEngine;

/// <summary>
/// 粒子播放管理
/// </summary>
public class ParticleManager
{
    /// <summary>
    /// 播放粒子特效
    /// </summary>
    /// <param name="resId">资源ID</param>
    /// <param name="duration">持续事件</param>
    /// <param name="cache">是否缓存到对象池中</param>
    /// <returns></returns>
    public GameObject PlayParticle(int resId, float duration, bool cache)
    {
        // 取缓存 
        ParticleTimer unit = m_particleCache.Get(resId);
        GameObject particle = unit != null ? unit.gameObject : null;
        if (particle == null)
        {
            if (null != unit)
                m_particleCache.Remove(unit);
            // 缓存没有 ///		
            particle = ResourceManager.instance.Instantiate<GameObject>(resId);

        }

        if (particle == null) return null;

        ResourceConfigItem item = ResourceManager.instance.GetResourceItem(resId);
        if (duration > 0f && cache)
        {
            if (unit == null)
            {
                unit = particle.AddComponent<ParticleTimer>();
                unit.id = resId;
            }

            unit.onFinishedCallBack = (id, timer) =>
             {
                 // 塞回缓存里
                 m_particleCache.Add(id, timer);
             };
            unit.LimitTime = duration;
            unit.gameObject.SetActive(true);
        }
        else
        {
            if (duration > 0f) UnityEngine.Object.Destroy(particle, duration);
        }
        CreateRoot();
        particle.transform.SetParent(m_particleRoot, false);
        return particle;
    }

    private void CreateRoot()
    {
        if (null == m_particleRoot)
        {
            var rootObj = new GameObject("ParticleRoot");
            m_particleRoot = rootObj.transform;
        }
    }

    public void ClearCache()
    {
        if(null != m_particleRoot)
            Object.Destroy(m_particleRoot.gameObject);
        m_particleCache.Clear();
    }


    private ParticleCache m_particleCache = new ParticleCache();
    private Transform m_particleRoot;


    private static ParticleManager s_instance;
    public static ParticleManager instance
    {
        get
        {
            if (null == s_instance)
                s_instance = new ParticleManager();
            return s_instance;

        }
    }
}