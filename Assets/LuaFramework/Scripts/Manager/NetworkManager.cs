using System.Collections.Generic;
using LuaInterface;


namespace LuaFramework
{
    /// <summary>
    /// 与Lua对接的网络管理器
    /// </summary>
    public class NetworkManager : INetStateListener
    {
        private static NetworkManager _instance;
        public static NetworkManager GetInstance()
        {
            if (_instance == null)
            {
                _instance = new NetworkManager();
            }

            return _instance;
        }

        private NetworkManager()
        {
            // 网络状态监听
            ClientNet.instance.AddNetStateListener(this);
        }


        // 协议处理tag:proto定义的标记 |flag 0:只C#层处理(不需要添加)，1:lua C#共同处理 2:只lua 处理
        static Dictionary<int, int> m_s2cDic = new Dictionary<int, int>();
        static Dictionary<int, int> m_c2sDic = new Dictionary<int, int>();
        static Dictionary<int, string> m_session2ProtoDic = new Dictionary<int, string>();

        // 收到服务器消息后的lua处理方法
        private static LuaFunction OnRequestDataFun = null;
        public static void SetOnRequestDataFun(LuaFunction requestFun)
        {
            OnRequestDataFun = requestFun;
        }

        private static LuaFunction OnResponseDataFun = null;
        public static void SetOnResponseDataFun(LuaFunction responseFun)
        {
            OnResponseDataFun = responseFun;
        }

        public static int GetNextSession()
        {
            return ClientNet.instance.GetNextSession();
        }

        public static void AddLuaProcessS2C(int protocolTag, int flag)
        {
            int existFlag;
            if (!m_s2cDic.TryGetValue(protocolTag, out existFlag))
            {
                m_s2cDic.Add(protocolTag, flag);
            }
            else
            {
                GameLogger.LogError("NetworkManager AddLuaProcess repeate tag " + protocolTag);
            }
        }

        public static void AddLuaProcessC2S(int protocolTag, int flag)
        {
            int existFlag;
            if (!m_c2sDic.TryGetValue(protocolTag, out existFlag))
            {
                m_c2sDic.Add(protocolTag, flag);
            }
            else
            {
                GameLogger.LogError("NetworkManager AddLuaProcess repeate tag " + protocolTag);
            }
        }

        public static void RemoveLuaProcessS2C(int protocolTag)
        {
            m_s2cDic.Remove(protocolTag);
        }
        public static void RemoveLuaProcessC2S(int protocolTag)
        {
            m_c2sDic.Remove(protocolTag);
        }

        public static string GetProtoBySession(int session)
        {
            string protoName = null;
            if (m_session2ProtoDic.TryGetValue(session, out protoName))
            {
                return protoName;
            }
            return null;
        }

        [NoToLuaAttribute]
        public void OnInit()
        {
            CallMethod("OnInit");

            OnRequestDataFun = Util.GetLuaFunc("Network", "OnRequestDataFun");

            OnResponseDataFun = Util.GetLuaFunc("Network", "OnResponseDataFun");
        }

        [NoToLuaAttribute]
        public void Unload()
        {

            if (OnRequestDataFun != null)
            {
                OnRequestDataFun.Dispose();
            }

            if (OnResponseDataFun != null)
            {
                OnResponseDataFun.Dispose();
            }
        }

        /// <summary>
        /// 执行Lua方法
        /// </summary>
        [NoToLuaAttribute]
        public object[] CallMethod(string func, params object[] args)
        {
            return LuaCall.CallFunc(ManagerName.Network + "." + func, args);
        }

        ///---------------------------------接入本地的网络处理---------------------------------------------------
        [NoToLuaAttribute]
        public void OnNetStateChanged(NetState state, object param = null)
        {
            CallMethod("OnNetStateChanged", (int)state, param);
        }

        /// <summary>
        /// 发送的时候缓存一下session
        /// </summary>
        /// <param name="session">session</param>
        /// <param name="protoName">协议名</param>
        public static void OnSendData(int session, string protoName)
        {
            m_session2ProtoDic.Add(session, protoName);
        }

        /// <summary>
        /// 消息返回
        /// </summary>
        /// <param name="spStream"></param>
        /// <param name="type"></param>
        /// <returns>return true传递下去，继续处理，false后续不用在处理</returns>
        [NoToLuaAttribute]
        public static bool OnRequestData(SpStream spStream, int type)
        {
            int processType = 0;
            if (m_s2cDic.TryGetValue(type, out processType))
            {
                if (processType > 0)
                {
                    if (OnRequestDataFun != null)
                    {
                        //OnRequestDataFun.Call(spStream.Buffer);

                        object[] objs = OnRequestDataFun.Call(spStream, spStream.Length);
                        if (objs != null && objs.Length >= 1)
                        {
                            return (bool)objs[0];
                        }
                    }

                    return processType <= 1;
                }
            }
            return true;
        }

        /// <summary>
        /// 消息返回
        /// </summary>
        /// <param name="spStream"></param>
        /// <param name="protocol"></param>
        /// <param name="session">session</param>
        /// <returns>return true传递下去，继续处理，false后续不用在处理</returns>
        [NoToLuaAttribute]
        public static bool OnResponseData(SpStream spStream, SpProtocol protocol, int session)
        {
            bool bRet = true;

            if (protocol == null)
            {
                // C#部分未处理的协议，传入到lua 处理
                if (OnResponseDataFun != null)
                {
                    string protoName = null;
                    if (!m_session2ProtoDic.TryGetValue(session, out protoName))
                    {
                        GameLogger.LogError("NetworkManager OnResponseData protocol is nil session not exist " + session);
                    }

                    object[] objs = OnResponseDataFun.Call(spStream, spStream.Length, protoName);
                    if (objs != null && objs.Length >= 1)
                    {
                        bRet = (bool)objs[0];
                    }
                    else
                    {
                        bRet = false;
                    }

                }
            }
            else
            {
                int processType = 0;
                if (m_c2sDic.TryGetValue(protocol.Tag, out processType) && processType > 0)
                {
                    bRet = processType <= 1;
                    if (OnResponseDataFun != null)
                    {
                        string protoName = null;
                        if (!m_session2ProtoDic.TryGetValue(session, out protoName))
                        {
                            GameLogger.LogError("NetworkManager OnResponseData session not exist " + session + " tag = " + protocol.Tag);
                        }

                        object[] objs = OnResponseDataFun.Call(spStream, spStream.Length, protoName);
                        if (objs != null && objs.Length >= 1)
                        {
                            bRet = (bool)objs[0];
                        }
                    }
                }
            }

            m_session2ProtoDic.Remove(session);


            return bRet;
        }

        /// <summary>
        /// 提供给lua发送数据接口
        /// </summary>
        /// <param name="proto"></param>
        /// <param name="session"></param>
        /// <param name="tag"></param>
        /// <param name="data"></param>
        [ExportToLuaAttribute]
        public static void SendData(string proto, int session, int tag, LuaByteBuffer data)
        {
            ClientNet.instance.Send(proto, data.buffer, data.buffer.Length, session, tag);
        }



        /// <summary>
        /// 析构函数
        /// </summary>
        [NoToLuaAttribute]
        public void OnDestroy()
        {
            if (OnRequestDataFun != null)
            {
                OnRequestDataFun.Dispose();
            }

            if (OnResponseDataFun != null)
            {
                OnResponseDataFun.Dispose();
            }

            GameLogger.Log("~NetworkManager was destroy");
        }
    }
}