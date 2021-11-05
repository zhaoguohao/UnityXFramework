using System;
using UnityEngine;


public delegate void OnRecvData(SpRpcResult result);

/// <summary>
/// 客户端网络连接
/// </summary>
public class ClientNet : MonoBehaviour
{
    void Awake()
    {
        if (s_instance == null)
        {
            s_instance = this;
        }
        else
        {
            GameLogger.LogError("clientNet ctor error.");
        }
    }

    public void Init()
    {
        m_connection.Initialize(s2c.sproto, c2s.sproto, "package", new ProtocolProcessor().onRecvData, true);
    }

    public bool Connect(string ip, int port, bool isReconnect = false)
    {
        return m_connection.Connect(ip, port, isReconnect);
    }

    public void Send(string proto, SpObject arg)
    {
        m_connection.Send(proto, arg);
    }

    public void Send(string proto, SpObject arg, Action<string, SpObject> cb)
    {
        ProtocolProcessor.AddCallback(SpRpcOp.Response, proto, cb);
        m_connection.Send(proto, arg);
    }

    /// <summary>
    /// 发送消息，给Lua 使用的接口
    /// </summary>
    public void Send(string proto, byte[] data, int length, int session, int tag)
    {
        m_connection.Send(proto, data, length, session, tag);
    }

    public void Resend(bool isUseNewSession = false)
    {
        m_connection.Resend(isUseNewSession);
    }

    public int GetNextSession()
    {
        return m_connection.GetNextSession();
    }

    public void AddNetStateListener(INetStateListener listener)
    {
        m_connection.AddNetStateListener(listener);
    }

    void Update()
    {
        m_connection.Update();
    }

    /// <summary>
    /// 关闭连接,并触发Disconnect回调
    /// </summary>
    public void Disconnect()
    {
        m_connection.Disconnect();
    }

    /// <summary>
    /// 关闭连接,不触发Disconnect回调
    /// </summary>
    public void Close()
    {
        m_connection.Close();
    }

    private void OnDestroy()
    {
        if (m_connection != null) m_connection.Close();
    }

    private ClientNet()
    {
        m_connection = new RemoteConnection();
    }

    public bool checkIfHasCachePacks()
    {
        if (m_connection.getCachPacksLen() > 0) return true;
        return false;
    }

    public void ClearMsgQueue()
    {
        m_connection.ClearMsgQueue();
    }

    public bool isConnect { get { return m_connection.isConnect; } }

    public string resendTarget { get { return m_resendtarget; } set { m_resendtarget = value; } }

    public bool enableCrypt { get { return m_connection.enableCrypt; } set { m_connection.enableCrypt = value; } }

    public static ClientNet instance
    {
        get
        {
            if (null == s_instance)
            {
                GameObject clientNetObj = new GameObject("ClientNet");
                s_instance = clientNetObj.AddComponent<ClientNet>();
            }
            return s_instance;
        }
    }

    private static ClientNet s_instance;
    private RemoteConnection m_connection;
    private string m_resendtarget;
}