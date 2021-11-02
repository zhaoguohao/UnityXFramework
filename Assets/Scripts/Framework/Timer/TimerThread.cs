using System.Threading;
using System.Collections.Generic;

public enum TimerID
{
    OneSecond,      //1秒钟事件//
}

/// <summary>
/// 定时器线程
/// </summary>
public class TimerThread
{
    public const string LANG_KEY = "TimerThread";

    public const uint MAX_TMR_EVENT = 9999;

    private Timer[] m_tmrArray;

    /// <summary>
    /// 定时器事件存储器
    /// </summary>
    private Dictionary<uint, TagTimer> m_timerStoriage;

    public struct TagTimer
    {
        public long dueTime;            //多长时间后出发,ms//
        public uint loopTimes;          //重复多少次//
        public bool isForever;              //是否无限循环//
        public uint tmrIndex;               //tmr数组索引//
        public object[] param;                   //附加参数//
    }

    public struct TagTimerEvent
    {
        public uint idTimeEvent;
        public object[] param;
    }

    private TimerThread()
    {
    }

    private static TimerThread s_instance = null;
    public static TimerThread instance
    {
        get
        {
            if (s_instance == null)
            {
                s_instance = new TimerThread();
            }

            return s_instance;
        }
    }

    public bool Init()
    {
        m_tmrArray = new Timer[MAX_TMR_EVENT];
        m_timerStoriage = new Dictionary<uint, TagTimer>();

        for (uint i = 0; i < MAX_TMR_EVENT; ++i)
        {
            m_tmrArray[i] = null;
        }
        SetTimer_Loop((int)TimerID.OneSecond, 1000);
        return true;
    }

    /// <summary>
    ///无限重复触发
    /// </summary>
    public void SetTimer_Loop(uint idTimeEvent, long dueTime, params object[] dwParam)
    {
        TagTimerEvent timerEvent = new TagTimerEvent();
        timerEvent.idTimeEvent = idTimeEvent;
        timerEvent.param = dwParam;
        TagTimer m_tagTimer = new TagTimer();
        int dwTmrIndex = FindNullIndex();
        if (dwTmrIndex == -1)
        {
            return;
        }
        if (!m_timerStoriage.ContainsKey(idTimeEvent))
        {
            m_tagTimer.dueTime = dueTime;
            m_tagTimer.loopTimes = 999;
            m_tagTimer.isForever = true;
            m_tagTimer.tmrIndex = (uint)dwTmrIndex;
            m_tagTimer.param = dwParam;
            m_timerStoriage.Add(idTimeEvent, m_tagTimer);
            m_tmrArray[dwTmrIndex] = new Timer(OnEventTimer, timerEvent, dueTime, dueTime);
            //GameLogger.Log("SetTimer_Loop:" + IDTimeEvent);
        }
    }

    /// <summary>
    /// 定时触发一次
    /// </summary>
    public void SetTimer_Once(uint idTimeEvent, long dueTime, params object[] dwParam)
    {
        TagTimerEvent timerEvent = new TagTimerEvent();
        timerEvent.idTimeEvent = idTimeEvent;
        timerEvent.param = dwParam;
        TagTimer m_tagTimer = new TagTimer();
        int dwTmrIndex = FindNullIndex();
        if (dwTmrIndex == -1) return;
        if (!m_timerStoriage.ContainsKey(idTimeEvent))
        {
            m_tagTimer.dueTime = dueTime;
            m_tagTimer.loopTimes = 1;
            m_tagTimer.isForever = false;
            m_tagTimer.tmrIndex = (uint)dwTmrIndex;
            m_tagTimer.param = dwParam;
            m_timerStoriage.Add(idTimeEvent, m_tagTimer);
            m_tmrArray[dwTmrIndex] = new Timer(OnEventTimer, timerEvent, dueTime, 0);
        }
        else
        {
            foreach (KeyValuePair<uint, TagTimer> item in m_timerStoriage)
            {
                if (item.Key == idTimeEvent)
                {
                    m_tagTimer = item.Value;
                    if (m_tagTimer.isForever == false)
                    {
                        m_tagTimer.loopTimes += 1;
                        m_timerStoriage.Remove(idTimeEvent);
                        m_timerStoriage.Add(idTimeEvent, m_tagTimer);
                        m_tmrArray[dwTmrIndex] = new Timer(OnEventTimer, timerEvent, dueTime, 0);
                        return;
                    }
                }
            }
        }
    }

    /// <summary>
    /// 定时触发N次
    /// </summary>
    public void SetTimer_NTims(uint idTimeEvent, long dueTime, uint loopTimes, params object[] dwParam)
    {
        TagTimerEvent timerEvent = new TagTimerEvent();
        timerEvent.idTimeEvent = idTimeEvent;
        timerEvent.param = dwParam;
        TagTimer m_tagTimer = new TagTimer();
        int dwTmrIndex = FindNullIndex();
        if (dwTmrIndex == -1) return;
        if (!m_timerStoriage.ContainsKey(idTimeEvent))
        {
            m_tagTimer.dueTime = dueTime;
            m_tagTimer.loopTimes = loopTimes;
            m_tagTimer.isForever = false;
            m_tagTimer.tmrIndex = (uint)dwTmrIndex;
            m_tagTimer.param = dwParam;
            m_timerStoriage.Add(idTimeEvent, m_tagTimer);
        }
        m_tmrArray[dwTmrIndex] = new Timer(OnEventTimer, timerEvent, dueTime, 0);
    }

    public void KillTimer(uint idTimeEvent)
    {
        if (m_timerStoriage.ContainsKey(idTimeEvent))
        {
            m_tmrArray[m_timerStoriage[idTimeEvent].tmrIndex] = null;
            m_timerStoriage.Remove(idTimeEvent);
        }
    }

    /// <summary>
    /// 定时器响应接口
    /// </summary>
    public void OnEventTimer(object timeEvent)
    {
        TagTimerEvent timerEvent = (TagTimerEvent)timeEvent;
        uint idTimeEvent = timerEvent.idTimeEvent;
        if (!m_timerStoriage.ContainsKey(idTimeEvent))
        {
            return;
        }
        else
        {
            TagTimer m_tagTimer = m_timerStoriage[idTimeEvent];
            if ((m_tagTimer.isForever == false) && (m_tagTimer.loopTimes == 0))
            {
                m_tmrArray[m_tagTimer.tmrIndex] = null;
                m_timerStoriage.Remove(idTimeEvent);
                return;
            }
        }

        //m_OnTimerEvent.Add(t_TimerEvent);
        //事件触发//
        onTimerEvent(timerEvent);

        foreach (KeyValuePair<uint, TagTimer> item in m_timerStoriage)
        {
            if (item.Key == idTimeEvent)
            {
                TagTimer m_tagTimer = item.Value;
                if (m_tagTimer.isForever == false)
                {
                    if (m_tagTimer.loopTimes > 1)
                    {
                        m_tagTimer.loopTimes -= 1;
                        m_timerStoriage.Remove(idTimeEvent);
                        m_timerStoriage.Add(idTimeEvent, m_tagTimer);
                        SetTimer_NTims(idTimeEvent, m_tagTimer.dueTime, m_tagTimer.loopTimes);
                        return;
                    }
                    else
                    {
                        m_timerStoriage.Remove(idTimeEvent);
                        return;
                    }
                }
            }
        }
    }

    private int FindNullIndex()
    {
        for (int i = 0; i < MAX_TMR_EVENT; ++i)
        {
            if (m_tmrArray[i] == null)
                return i;
        }
        return -1;
    }

    private void onTimerEvent(TagTimerEvent timerEvent)
    {
        switch (timerEvent.idTimeEvent)
        {
            case (uint)TimerID.OneSecond:
                {
                    // GameLogger.LogGreen("tick");
              
                    break;
                }
        }
    }
}

