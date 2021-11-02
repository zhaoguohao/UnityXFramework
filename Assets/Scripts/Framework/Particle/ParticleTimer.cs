using UnityEngine;


/// <summary>
/// 粒子生命周期器
/// </summary>
public class ParticleTimer : MonoBehaviour
{
    public delegate void OnFinished(int id, ParticleTimer target);

    public OnFinished onFinishedCallBack;

    public float LimitTime
    {
        get { return m_time; }
        set { m_time = value; }
    }

    public int id
    {
        get { return m_id; }
        set { m_id = value; }
    }

    private float m_time;

    private float m_runTime;

    private int m_id;

    private bool isAlreadyCall = false;

    void OnEnable()
    {
        m_runTime = 0f;
        isAlreadyCall = false;
    }

    void Update()
    {
        m_runTime += Time.deltaTime;
        if (m_runTime >= m_time)
        {
            m_runTime = -100000;
            DoEvent();
        }
    }

    
    void OnDisable()
    {
        if (DoCallBack()) return;
        gameObject.SetActive(false);
        
    }

    private void DoEvent()
    {
        if (DoCallBack()) return;

        gameObject.SetActive(false);
    }

    private bool DoCallBack()
    {
        if (isAlreadyCall) return true;
        isAlreadyCall = true;
        if (onFinishedCallBack != null)
            onFinishedCallBack(m_id, this);
        onFinishedCallBack = null;
        
        return false;
    }
}
