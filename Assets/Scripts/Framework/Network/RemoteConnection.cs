using System;
using System.Collections;
using System.Collections.Generic;
using System.Net.Sockets;
using UnityEngine;

/// <summary>
/// 网络连接
/// </summary>
public class RemoteConnection
{
    public RemoteConnection()
    {
        m_read = 0;
        m_curSession = 0;
        m_locker = new object();
        m_recvBuffer = new byte[0x2000];
        m_sendStream = new SpStream();
        m_listeners = new ArrayList();
        m_packQueue = new PackageQueue();
        m_cachePacks = new List<CachePackInfo>();
        m_streamlist = new List<SpStream>();
        m_netStateList = new List<NetStateParam>();
        m_curNetState = NetState.Disconnect;
        recvCallbackFunc = new AsyncCallback(recvCallback);
        m_heartbeat = new Heartbeat(this);
    }

    public bool Initialize(string s2c, string c2s, string head, OnRecvData recvCallback, bool encrytor = false)
    {
        try
        {
            m_rpc = SpRpc.Create(s2c, head);
            m_rpc.Attach(c2s);
            onRecvDataFunc = recvCallback;
            if (onRecvDataFunc == null) throw (new NullReferenceException("onRecvdataFunc == null"));


            enableCrypt = encrytor;
            return true;
        }
        catch (Exception exp)
        {
            GameLogger.LogError(exp.ToString());
            return false;
        }
    }

    public void AddNetStateListener(INetStateListener listener)
    {
        if (m_listeners != null)
            m_listeners.Add(listener);
    }

    public bool Connect(string strHost, int port, bool isReconnect = false)
    {
        try
        {
            Close();
            m_passTime = 0f;
            m_connectCounter = 0f;
            m_isReconnect = isReconnect;
            // TODO iOS可能需要处理一下ipv6的连接
            
            m_socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            m_socket.SendTimeout = 500;
            m_iAsyncResult = m_socket.BeginConnect(strHost, port, null, m_socket);
            GameLogger.Log("ClientNet, connect: " + strHost + " port: " + port + " handle: " + m_socket.Handle);

            AddNetState(NetState.ConnectStart, null);
        }
        catch (Exception exception)
        {
            Close();
            m_bConnect = false;
            GameLogger.LogError(exception.ToString());
            AddNetState(NetState.ConnectFail, null);
        }

        return m_bConnect;
    }

    public void Send(string proto, SpObject arg)
    {
        lock (m_locker)
        {
            //try
            {
                int length = 0;
                byte[] data = null;
                m_curSession++;
                cachePack(proto, arg);
                if (!m_isReconnect)
                {
                    obj2Stream(proto, arg, m_curSession, out data, out length);
                    send(m_curSession, proto, data, length);

                    LuaFramework.NetworkManager.OnSendData(m_curSession, proto);
                }
            }
            //catch (Exception ex)
            //{
            //    GameLogger.LogWarning(string.Format("Send error, protoName:{0}, exp:{1} ", proto, ex));
            //    Util.DumpObject(arg);
            //}
        }
    }

    /// <summary>
    /// Lua 发送使用接口
    /// </summary>
    /// <param name="proto">协议名</param>
    /// <param name="srcData">消息数据</param>
    /// <param name="srcLen">消息长度</param>
    /// <param name="session"></param>
    /// <param name="tag"></param>
    public void Send(string proto, byte[] srcData, int srcLen, int session, int tag)
    {
        lock (m_locker)
        {
            try
            {
                //SpStream stream = SpStreamCache.Get();
                //stream.Write(srcData);
                //SpRpcResult result = m_rpc.DispatchTest(stream);
                m_rpc.AddSession(session, null, tag);

                //都不需要缓存
                //if (!IgnoreResendMsgDef.isIgnore(proto))
                //{
                //    m_cachePacks.Add(new CachePackInfo(session, proto, null));
                //}

                int length = 0;
                byte[] data = null;
                EncryptyData(srcData, srcLen, out data, out length);
                send(session, proto, data, length);

                LuaFramework.NetworkManager.OnSendData(session, proto);

            }
            catch (Exception ex)
            {
                GameLogger.LogWarning(string.Format("Send error, protoName:{0}, exp:{1} ", proto, ex));
            }
        }
    }

    public void Resend(bool isUseNewSession = false)
    {
        int length = 0;
        byte[] data = null;
        m_resendCounter = 0f;
        AddNetState(NetState.ResendStart, null);
        for (int i = 0; i < m_cachePacks.Count; ++i)
        {
            SprotoUtil.DumpObject(m_cachePacks[i].spObj);
            if (isUseNewSession) m_cachePacks[i].session = m_curSession++;
            GameLogger.LogYellow("resend: " + m_cachePacks[i].proto + ", " + m_cachePacks[i].session);
            obj2Stream(m_cachePacks[i].proto, m_cachePacks[i].spObj, m_cachePacks[i].session, out data, out length);
            send(m_cachePacks[i].session, m_cachePacks[i].proto, data, length);
        }
    }

    private void cachePack(string proto, SpObject arg)
    {
        //if (IgnoreResendMsgDef.isIgnore (proto)) return;
        SpObject cacheSpObj = arg == null ? arg : arg.DeepClone();
        m_cachePacks.Add(new CachePackInfo(m_curSession, proto, cacheSpObj));
    }

    private void obj2Stream(string proto, SpObject arg, int session, out byte[] data, out int length)
    {
        data = null;
        length = 0;
        m_sendStream.Reset();
        m_sendStream.Write((short)0);
        m_rpc.Request(proto, arg, session, m_sendStream);
        int len = m_sendStream.Length - 2;
        m_sendStream.Buffer[0] = (byte)((len >> 8) & 0xff);
        m_sendStream.Buffer[1] = (byte)(len & 0xff);

        // 加密
        // if (m_encrypt != null)
        // {
        //     byte[] out_data;
        //     int out_data_len = m_encrypt.EncryptProtoBeforeSend(m_sendStream, out out_data);
        //     data = out_data;
        //     length = out_data_len;
        //     return;
        // }

        length = m_sendStream.Length;
        data = m_sendStream.Buffer;
    }

    public void EncryptyData(byte[] src, int srcLen, out byte[] data, out int length)
    {
        m_sendStream.Reset();
        m_sendStream.Write((short)0);
        int len = srcLen;
        m_sendStream.Buffer[0] = (byte)((len >> 8) & 0xff);
        m_sendStream.Buffer[1] = (byte)(len & 0xff);
        m_sendStream.Write(src);

        // 加密
        // if (m_encrypt != null)
        // {
        //     byte[] out_data;
        //     int out_data_len = m_encrypt.EncryptProtoBeforeSend(m_sendStream, out out_data);
        //     data = out_data;
        //     length = out_data_len;
        //     return;
        // }

        length = m_sendStream.Length;
        data = m_sendStream.Buffer;
    }

    public void Update()
    {
        invokeListener();
        if (isConnecting())//连接中
        {
            if (isConnectTimeout())
            {
                GameLogger.LogWarning("connect timeout");
                connectFailed();
                return;
            }

            if (isConnectOk())
            {
                try
                {
                    beginReceive();
                }
                catch (System.Exception ex)
                {
                    GameLogger.LogWarning("beginReceive error," + ex.ToString());
                    connectFailed();
                }
            }
        }
        else//消息回调
        {
            if (isResending())
            {
                if (isResendTimeout())
                {
                    resendFailed();
                }
                else if (isResendOk())
                {
                    resendSuccess();
                }
            }

            if (isTime2GetMessage())
            {
                getMessage();
                dispatchMessage();
            }
        }
    }

    private bool isConnecting()
    {
        return m_curNetState == NetState.ConnectStart;
    }

    private bool isConnectTimeout()
    {
        m_connectCounter += Time.deltaTime;
        return m_connectCounter > 10f;/// 10秒超时 ///
    }

    private bool isConnectOk()
    {
        if (m_socket == null) return false;
        return m_socket.Poll(1000, SelectMode.SelectWrite);
    }

    private void beginReceive()
    {
        m_recvOffset = 0;
        m_bConnect = true;
        m_packQueue.Clear();
        zeroMemory(m_recvBuffer);
        m_socket.EndConnect(m_iAsyncResult);
        m_socket.BeginReceive(m_recvBuffer, 0, m_recvBuffer.Length,
            SocketFlags.None, recvCallbackFunc, this);

        // 加密
        // if (m_encrypt != null)
        // {
        //     byte[] data = m_encrypt.Init();
        //     int ret = m_socket.Send(data);
        //     Debug.Log("<color=yellow>   m_socket.Send(data); == " + ret + " </color>");
        // }

        m_lastRecvTick = DateTime.Now.Ticks;
        m_lastSendTick = DateTime.Now.Ticks;
        m_heartbeat.Start();
        AddNetState(NetState.ConnectSuccess, null);
    }

    private void connectFailed()
    {
        Close();
        GameLogger.Log("connect failed");
        AddNetState(NetState.ConnectFail, null);
    }

    private bool isResending()
    {
        return m_curNetState == NetState.ResendStart;
    }

    private bool isResendTimeout()
    {
        m_resendCounter += Time.deltaTime;
        return m_resendCounter >= 15f; //resend timeout
    }

    private bool isResendOk()
    {
        return m_cachePacks.Count == 0;
    }

    private void resendSuccess()
    {
        m_isReconnect = false;
        AddNetState(NetState.ResendSuccess, null);
    }

    private void resendFailed()
    {
        Close();
        GameLogger.Log("resend failed");
        AddNetState(NetState.ResendFail, null);
    }

    private bool isTime2GetMessage()
    {
        m_passTime += Time.deltaTime;
        return m_passTime >= CMD_QUEUE_CHECK_INTERVAL;
    }

    private void getMessage()
    {
        m_passTime = 0f;
        m_packQueue.getCmdListAndClear(m_streamlist);
    }

    private void dispatchMessage()
    {
        for (int i = 0; i < m_streamlist.Count; ++i)
        {
            if (m_streamlist[i] != null)
            {
                SpRpcResult result = stream2Obj(m_streamlist[i]);
                if (result != null && result.Protocol != null)
                {
                    //if(result.Op == SpRpcOp.Request)
                    //	GameLogger.LogGreen(result.Protocol.Name + ", " + DateTime.Now.ToString());
                    if (removeCachePack(result))
                    {
                        //已经被处理过了， 不需要往下传递了
                        if (!result.bProcess)
                        {
                            onRecvDataFunc(result);
                        }

                        //网络统计
                        NetDataAnalysis.RecieveData(result, m_streamlist[i]);
                    }
                }
            }
        }

        m_streamlist.Clear();
    }

    private SpRpcResult stream2Obj(SpStream stream)
    {
        SpRpcResult result = null;
        try
        {
            result = m_rpc.Dispatch(stream);
        }
        catch (Exception exp)
        {
            GameLogger.LogError("SpRpcResult: " + exp);
            GameLogger.LogError("SpRpcResult: " + exp.StackTrace);
        }

        return result;
    }

    public void Disconnect()
    {
        Close();
        GameLogger.Log("disconnect.");
        AddNetState(NetState.Disconnect, null);
    }

    public void Close()
    {
        lock (m_locker)
        {
            m_curNetState = NetState.Disconnect;
            if (m_heartbeat != null)
            {
                m_heartbeat.Stop();
            }
            if (m_socket == null) return;

            try
            {
                m_socket.Shutdown(SocketShutdown.Both);
                m_socket.Close();
            }
            catch (Exception exception)
            {
                GameLogger.LogWarning("NetSocket.close, exp: " + exception.Message);
            }

            GameLogger.LogYellow("close socket");
            m_bConnect = false;
            m_connectCounter = 0f;
            m_socket = null;
        }
    }

    private void send(int session, string proto, byte[] data, int length)
    {
        try
        {
            if (data == null || length <= 0) return;
            s_sendCount += length;
            if (m_socket != null)
            {
                m_lastSendTick = DateTime.Now.Ticks;
                m_socket.Send(data, length, SocketFlags.None);
                NetDataAnalysis.SendData(proto, data);
            }
        }
        catch (Exception ex)
        {
            Close();
            GameLogger.LogWarning(string.Format("send error, protoName:{0}, session:{1}, exp:{2} ", proto, session, ex));
            AddNetState(NetState.Error, null);
        }
    }

    private bool removeCachePack(SpRpcResult ret)
    {
        if (ret.Op == SpRpcOp.Request) return true;
        //if(IgnoreResendMsgDef.isIgnore(ret.Protocol.Name))
        //          return true;

        if (ret.Op == SpRpcOp.Response)
        {
            for (int i = 0; i < m_cachePacks.Count; ++i)
            {
                if (ret.Session == m_cachePacks[i].session)
                {
                    m_cachePacks.RemoveAt(i);
                    return true;
                }
            }
        }

        try
        {
            if (ret.bProcess)
            {
                //Lua 已经处理 C# 不需要处理
                return true;
            }
            //lua C#也需要同时处理
            //有些是lua发送的协议， 但是lua 不处理 留个C#处理~
            return true;
            //Util.DumpObject(ret.Arg);
        }
        catch (System.Exception ex)
        {
            GameLogger.LogError("removeCachePack, " + ex);
        }

        return false;
    }

    private void recvCallback(IAsyncResult ar)
    {
        try
        {
            int len = 0;
            lock (m_locker)
            {
                if (m_socket != null) len = m_socket.EndReceive(ar);
                if (len > 0) m_lastRecvTick = DateTime.Now.Ticks;
            }

            if (len > 0)
            {
                s_recvCount += len;
                m_recvOffset += len;
                m_read = 0;

                while (m_recvOffset - m_read >= 2)
                {
                    int size = 0;
                    try
                    {
                        size = (m_recvBuffer[m_read + 1] | (m_recvBuffer[m_read + 0] << 8));
                        if (size > m_recvBuffer.Length)
                            GameLogger.LogError(string.Format("out of size!!!!!recv data size:{0}", size));
                        // if (m_encrypt != null && size % 8 != 0)
                        //     GameLogger.LogError(string.Format("Error size % 8 != 0 size={0}", size));
                    }
                    catch (Exception exp)
                    {
                        GameLogger.LogError("out of size: " + exp.Message);
                    }

                    if (m_recvOffset >= (m_read + 2 + size))
                    {
                        SpStream stream;
                        // if (m_encrypt != null)
                        // {
                        //     stream = m_encrypt.DecryptAsSpStream(m_recvBuffer, m_read + 2, size);
                        // }
                        // else
                        {
                            stream = new SpStream(size);
                            stream.Write(m_recvBuffer, m_read + 2, size);
                        }

                        stream.Position = 0;
                        m_packQueue.pushCmd(stream);
                        m_read = m_read + 2 + size;
                    }
                    else
                    {
                        break;
                    }
                }

                if (m_read > 0)
                {
                    for (int i = m_read; i < m_recvOffset; ++i)
                    {
                        m_recvBuffer[i - m_read] = m_recvBuffer[i];
                    }

                    m_recvOffset -= m_read;
                }

                lock (m_locker)
                {
                    if (m_socket != null)
                    {
                        m_socket.BeginReceive(m_recvBuffer, m_recvOffset,
                            m_recvBuffer.Length - m_recvOffset, SocketFlags.None, recvCallbackFunc, this);
                    }
                }
            }
        }
        catch (Exception exception)
        {
            Close();
            string errMsg = "NetSocket.recvCallback, exp: " + exception.Message;
            Debug.LogError(errMsg);
            AddNetState(NetState.Error, errMsg);
        }
    }

    public void AddNetState(NetState state, object param = null)
    {
        lock (m_locker)
        {
            m_curNetState = state;
            NetStateParam item = new NetStateParam();
            item.netstate = state;
            item.param = param;
            m_netStateList.Add(item);
        }
    }

    private void invokeListener()
    {
        lock (m_locker)
        {
            if (m_netStateList.Count > 0)
            {
                foreach (INetStateListener lisenter in m_listeners)
                {
                    if (lisenter != null)
                        lisenter.OnNetStateChanged(m_netStateList[0].netstate, m_netStateList[0].param);
                }

                m_netStateList.RemoveAt(0);
            }
        }
    }

    private void zeroMemory(byte[] buffer)
    {
        if (buffer != null)
        {
            for (int i = 0; i < buffer.Length; ++i)
                buffer[i] = 0;
        }
    }

    public int getCachPacksLen()
    {
        return m_cachePacks.Count;
    }

    public void ClearMsgQueue()
    {
        m_cachePacks.Clear();
        m_packQueue.Clear();
        m_streamlist.Clear();
    }

    public bool enableCrypt
    {
        get { return m_isEnableCrypt; }

        set
        {
            m_isEnableCrypt = value;
            if (m_isEnableCrypt)
            {
                if (m_encrypt == null) m_encrypt = new ProtoEncrypt();
            }
            else
            {
                m_encrypt = null;
            }
        }
    }

    public bool isConnect { get { return m_bConnect; } }
    public long lastSendTick { get { return m_lastSendTick; } }
    public long lastRecvTick { get { return m_lastRecvTick; } }
    public static int recvCount { get { return s_recvCount; } }
    public static int sendCount { get { return s_sendCount; } }
    public int GetNextSession()
    {
        m_curSession++;
        return m_curSession;
    }

    private static int s_recvCount = 0;
    private static int s_sendCount = 0;

    private int m_read;
    private int m_curSession;
    private int m_recvOffset;
    private bool m_bConnect;
    private bool m_isReconnect;
    private long m_lastRecvTick;
    private long m_lastSendTick;
    private object m_locker;
    private float m_passTime;
    private float m_connectCounter;
    private float m_resendCounter;
    private byte[] m_recvBuffer;
    private SpRpc m_rpc;
    private Socket m_socket;
    private SpStream m_sendStream;
    private ArrayList m_listeners;
    private Heartbeat m_heartbeat;
    private PackageQueue m_packQueue;
    private List<SpStream> m_streamlist;
    private NetState m_curNetState;
    private OnRecvData onRecvDataFunc;
    private IAsyncResult m_iAsyncResult;
    private List<CachePackInfo> m_cachePacks;
    private AsyncCallback recvCallbackFunc;
    private List<NetStateParam> m_netStateList;
    private bool m_isEnableCrypt;
    private ProtoEncrypt m_encrypt = null;
    private const float CMD_QUEUE_CHECK_INTERVAL = 0.06f;


    /// <summary>
    /// 回调参数
    /// </summary>
    private struct NetStateParam
    {
        public NetState netstate;
        public object param;
    }

    private class CachePackInfo
    {
        public CachePackInfo(int session, string proto, SpObject spObj)
        {
            this.session = session;
            this.proto = proto;
            this.spObj = spObj;
        }

        public int session;
        public string proto;
        public SpObject spObj;
    }
}