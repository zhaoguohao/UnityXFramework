using System;
using System.Collections.Generic;



/// <summary>
/// 游戏总的消息处理中心
/// </summary>
public class ProtocolProcessor
{
    public ProtocolProcessor()
    {
        s_onSvrRequestCallback = new Dictionary<string, List<Action<string, SpObject>>>();
        s_onSvrResponseCallback = new Dictionary<string, List<Action<string, SpObject>>>();
    }

    public void onRecvData(SpRpcResult result)
    {
#if !UNITY_EDITOR
        try
        {
#endif
        switch (result.Op)
        {
            case SpRpcOp.Request:
                {
                    invokeCallback(s_onSvrRequestCallback, result.Protocol.Name, result.Arg, result.Session);
                    break;
                }

            case SpRpcOp.Response:
                {
                    invokeCallback(s_onSvrResponseCallback, result.Protocol.Name, result.Arg, result.Session);
                    break;
                }
            default:
                GameLogger.LogError("onRecvData error,result.Op:" + result.Op);
                break;
        }
#if !UNITY_EDITOR
        }
        catch (System.Exception ex)
        {
            GameLogger.LogError("exception: " + ex);
            GameLogger.LogError("environment stackTrace: " + Environment.StackTrace);
        }
#endif
    }

    /// <summary>
    ///  加入消息回调
    /// </summary>
    /// <param name="operation">Request监听由服务器主动推送的消息,Response监听客户端主动请求服务器返回的消息</param>
    /// <param name="protocolName">需要监听的具体消息</param>
    /// <param name="callback">具体的回调处理函数</param>

    public static void AddCallback(SpRpcOp operation, string protocolName, Action<string, SpObject> callback)
    {
        Dictionary<string, List<Action<string, SpObject>>> callbacks = operation == SpRpcOp.Request ? s_onSvrRequestCallback : s_onSvrResponseCallback;
        List<Action<string, SpObject>> list = null;
        if (!callbacks.TryGetValue(protocolName, out list))
        {
            list = new List<Action<string, SpObject>>();
            callbacks.Add(protocolName, list);
        }
        if (list.Contains(callback))
        {
            list.Remove(callback);
        }
        list.Add(callback);
    }

    /// <summary>
    /// 移除消息回调,与AddCallback函数对应
    /// </summary>
    /// <param name="operation">Request监听由服务器主动推送的消息,Response监听客户端主动请求服务器返回的消息</param>
    /// <param name="protocolName">需要监听的具体消息</param>
    /// <param name="callback">具体的回调处理函数</param>
    public static void RemoveCallback(SpRpcOp operation, string protocolName, Action<string, SpObject> callback)
    {
        Dictionary<string, List<Action<string, SpObject>>> callbacks = operation == SpRpcOp.Request ? s_onSvrRequestCallback : s_onSvrResponseCallback;
        List<Action<string, SpObject>> list = null;
        if (callbacks.TryGetValue(protocolName, out list))
        {
            list.RemoveAll((cb) => { return cb == callback; });
        }
    }

    private void invokeCallback(Dictionary<string, List<Action<string, SpObject>>> callbacks, string protocolName, SpObject arg, int session)
    {
        if (callbacks.ContainsKey(protocolName))
        {
            List<Action<string, SpObject>> tcbs = new List<Action<string, SpObject>>();
            var cbs = callbacks[protocolName];
            tcbs.AddRange(cbs);
            for (int i = 0, cnt = tcbs.Count; i < cnt; ++i)
            {
                var cb = tcbs[i];
                if (null != cb) cb(protocolName, arg);
            }

            cbs.RemoveAll((cb) => { return cb == null; });
        }
    }

    private static Dictionary<string, List<Action<string, SpObject>>> s_onSvrRequestCallback;
    private static Dictionary<string, List<Action<string, SpObject>>> s_onSvrResponseCallback;
}