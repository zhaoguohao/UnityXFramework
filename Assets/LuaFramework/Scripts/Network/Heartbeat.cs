using System;
using System.Threading;
using System.Collections;

/// <summary>
/// 网络连接的心跳逻辑
/// </summary>
public class Heartbeat
{
    public Heartbeat(RemoteConnection connection)
    {
        m_locker = new object();
        m_connection = connection;
    }

    public void Start()
    {
        m_countNextSendTime = 0;
        m_countNextRecvTime = 0;
        m_timer = new Timer(callbackAfterPer5s, null, 1, 5000);
    }

    public void Stop()
    {
        lock (m_locker)
        {
            if (m_timer != null)
            {
                m_timer.Dispose();
                m_timer = null;
            }
        }
    }

    private void callbackAfterPer5s(Object state)
    {
        m_countNextSendTime+=5;
        m_countNextRecvTime+=5;

        if (m_countNextSendTime >= SEND_HEARTBEAT_INTERVAL)
        {
            m_countNextSendTime = 0;
            if (m_connection.isConnect)
                m_connection.Send("heartbeat", null);
        }

        int elapsed = getElapsedByTick(m_connection.lastRecvTick);
        if (elapsed >= NETWORK_TIMOUT)
        {
            Stop();
            m_countNextRecvTime = 0;
            m_connection.Disconnect();
        }
    }

    private int getElapsedByTick(long tick)
    {
        TimeSpan ts = new TimeSpan(DateTime.Now.Ticks - tick);
        return (int)(ts.TotalSeconds);
    }

    private int m_countNextSendTime, m_countNextRecvTime;
    private Timer m_timer;
    private object m_locker;
    private RemoteConnection m_connection;
    
    private const int NETWORK_TIMOUT = 40;
    private const int SEND_HEARTBEAT_INTERVAL = 15;
}
