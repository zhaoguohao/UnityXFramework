/*******************************************************************
Description:  事件管理
********************************************************************/


using UnityEngine;
using System.Collections.Generic;

public delegate void MyEventHandler(params object[] objs);

public class EventDispatcher
{
    /// <summary>
    /// 注册事件
    /// </summary>
    /// <param name="evt">事件名</param>
    /// <param name="handler">响应函数</param>
    public void Regist(string evt, MyEventHandler handler)
    {
        if (handler == null)
            return;

        if (listeners.ContainsKey(evt))
        {
            //这里涉及到Dispath过程中反注册问题，必须使用listeners[type]+=..
            listeners[evt] += handler;
        }
        else
        {
            listeners.Add(evt, handler);
        }
    }

    /// <summary>
    /// 注销事件
    /// </summary>
    /// <param name="evt">事件名</param>
    /// <param name="handler">响应函数</param>
    public void UnRegist(string evt, MyEventHandler handler)
    {
        if (handler == null)
            return;

        if (listeners.ContainsKey(evt))
        {
            //这里涉及到Dispath过程中反注册问题，必须使用listeners[type]-=..
            listeners[evt] -= handler;
            if (listeners[evt] == null)
            {
                //已经没有监听者了，移除.
                listeners.Remove(evt);
            }
        }
    }

    /// <summary>
    /// 抛出事件
    /// </summary>
    /// <param name="evt">事件名</param>
    /// <param name="objs">参数</param>
    public void DispatchEvent(string evt, params object[] objs)
    {
        try
        {
            if (listeners.ContainsKey(evt))
            {
                MyEventHandler handler = listeners[evt];
                if (handler != null)
                    handler(objs);
            }
        }
        catch (System.Exception ex)
        {
            Debug.LogErrorFormat(szErrorMessage, evt, ex.Message, ex.StackTrace);
        }
    }


    public void ClearEvents(string key)
    {
        if (listeners.ContainsKey(key))
        {
            listeners.Remove(key);
        }
    }

    private Dictionary<string, MyEventHandler> listeners = new Dictionary<string, MyEventHandler>();
    private readonly string szErrorMessage = "DispatchEvent Error, Event:{0}, Error:{1}, {2}";

    private static EventDispatcher s_instance;
    public static EventDispatcher instance
    {
        get
        {
            if (null == s_instance)
                s_instance = new EventDispatcher();
            return s_instance;
        }
    }
}

